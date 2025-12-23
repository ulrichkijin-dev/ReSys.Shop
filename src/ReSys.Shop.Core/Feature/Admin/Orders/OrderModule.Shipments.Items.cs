using ReSys.Shop.Core.Domain.Orders;
using ReSys.Shop.Core.Domain.Catalog.Products.Variants;
using ReSys.Shop.Core.Domain.Orders.Shipments;
using ReSys.Shop.Core.Domain.Inventories.Stocks;

namespace ReSys.Shop.Core.Feature.Admin.Orders;

public static partial class OrderModule
{
    public static partial class Shipments
    {
        public static class AddItem
        {
            public record Request(Guid VariantId, int Quantity);
            public sealed record Command(Guid OrderId, Guid ShipmentId, Request Request) : ICommand<Success>;

            public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Success>
            {
                public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
                {
                    var order = await dbContext.Set<Order>()
                        .Include(o => o.LineItems)
                            .ThenInclude(li => li.Adjustments)
                        .Include(o => o.OrderAdjustments)
                        .Include(o => o.Shipments)
                            .ThenInclude(s => s.InventoryUnits)
                        .Include(o => o.Promotion)
                            .ThenInclude(p => p!.PromotionRules)
                        .Include(o => o.Promotion)
                            .ThenInclude(p => p!.Action)
                        .FirstOrDefaultAsync(o => o.Id == command.OrderId, ct);

                    if (order == null) return Order.Errors.NotFound(command.OrderId);

                    var shipment = order.Shipments.FirstOrDefault(s => s.Id == command.ShipmentId);
                    if (shipment == null) return Shipment.Errors.NotFound(command.ShipmentId);

                    var variant = await dbContext.Set<Variant>().FirstOrDefaultAsync(v => v.Id == command.Request.VariantId, ct);
                    if (variant == null) return Error.NotFound("Variant.NotFound", "Variant not found.");

                    // Validate stock at the shipment's location
                    var stockItem = await dbContext.Set<StockItem>()
                        .FirstOrDefaultAsync(si => si.VariantId == variant.Id && si.StockLocationId == shipment.StockLocationId, ct);

                    bool isBackordered = stockItem == null || stockItem.CountAvailable < command.Request.Quantity;

                    if (isBackordered && (stockItem == null || !stockItem.Backorderable))
                    {
                        return Error.Validation("Shipment.InsufficientStock", "Insufficient stock and item is not backorderable at this location.");
                    }

                    // Ensure line item exists and has correct quantity
                    var addLineResult = order.AddLineItem(variant, command.Request.Quantity);
                    if (addLineResult.IsError) return addLineResult.Errors;
                    
                    var lineItem = order.LineItems.First(li => li.VariantId == variant.Id);

                    var initialState = isBackordered ? InventoryUnit.InventoryUnitState.Backordered : InventoryUnit.InventoryUnitState.OnHand;
                    for (int i = 0; i < command.Request.Quantity; i++)
                    {
                        var unitResult = InventoryUnit.Create(lineItem.VariantId, lineItem.Id, shipment.Id, initialState);
                        if (unitResult.IsError) return unitResult.Errors;
                        shipment.InventoryUnits.Add(unitResult.Value);
                        lineItem.InventoryUnits.Add(unitResult.Value);
                    }

                    // Re-apply promotion if exists
                    if (order.Promotion != null)
                    {
                        var calcResult = ReSys.Shop.Core.Domain.Promotions.Calculations.PromotionCalculator.Calculate(order.Promotion, order);
                        if (!calcResult.IsError)
                        {
                            order.ApplyPromotionAdjustments(order.Promotion.Id, calcResult.Value);
                        }
                        else
                        {
                            // If it fails now (e.g. min quantity not met anymore?), remove it
                            order.RemovePromotion();
                        }
                    }

                    order.AddDomainEvent(new Order.Events.ShipmentItemUpdated(order.Id, shipment.Id, lineItem.VariantId));

                    await dbContext.SaveChangesAsync(ct);
                    return Result.Success;
                }
            }
        }

        public static class RemoveItem
        {
            public record Request(Guid VariantId, int Quantity);
            public sealed record Command(Guid OrderId, Guid ShipmentId, Request Request) : ICommand<Success>;

            public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Success>
            {
                public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
                {
                    var order = await dbContext.Set<Order>()
                        .Include(o => o.LineItems)
                            .ThenInclude(li => li.Adjustments)
                        .Include(o => o.OrderAdjustments)
                        .Include(o => o.Shipments)
                            .ThenInclude(s => s.InventoryUnits)
                        .Include(o => o.Promotion)
                            .ThenInclude(p => p!.PromotionRules)
                        .Include(o => o.Promotion)
                            .ThenInclude(p => p!.Action)
                        .FirstOrDefaultAsync(o => o.Id == command.OrderId, ct);

                    if (order == null) return Order.Errors.NotFound(command.OrderId);

                    var shipment = order.Shipments.FirstOrDefault(s => s.Id == command.ShipmentId);
                    if (shipment == null) return Shipment.Errors.NotFound(command.ShipmentId);

                    var unitsToRemove = shipment.InventoryUnits
                        .Where(u => u.VariantId == command.Request.VariantId && u.IsPreShipment)
                        .Take(command.Request.Quantity)
                        .ToList();

                    if (unitsToRemove.Count < command.Request.Quantity)
                        return Error.Validation(code: "Order.InsufficientUnitsInShipment", description: "Not enough shippable units of this variant in the shipment.");

                    foreach (var unit in unitsToRemove)
                    {
                        shipment.InventoryUnits.Remove(unit);
                        var lineItem = order.LineItems.FirstOrDefault(li => li.Id == unit.LineItemId);
                        if (lineItem != null)
                        {
                            lineItem.InventoryUnits.Remove(unit);
                            
                            if (lineItem.Quantity > 1)
                            {
                                lineItem.UpdateQuantity(lineItem.Quantity - 1);
                            }
                            else
                            {
                                order.LineItems.Remove(lineItem);
                            }
                        }
                    }

                    // Re-apply promotion if exists
                    if (order.Promotion != null)
                    {
                        var calcResult = ReSys.Shop.Core.Domain.Promotions.Calculations.PromotionCalculator.Calculate(order.Promotion, order);
                        if (!calcResult.IsError)
                        {
                            order.ApplyPromotionAdjustments(order.Promotion.Id, calcResult.Value);
                        }
                        else
                        {
                            // If it fails now (e.g. min quantity not met anymore?), remove it
                            order.RemovePromotion();
                        }
                    }

                    order.RecalculateTotals();
                    order.AddDomainEvent(new Order.Events.ShipmentItemUpdated(order.Id, shipment.Id, command.Request.VariantId));

                    await dbContext.SaveChangesAsync(ct);
                    return Result.Success;
                }
            }
        }
    }
}