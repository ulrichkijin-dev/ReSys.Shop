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
                        .Include(o => o.Shipments)
                            .ThenInclude(s => s.InventoryUnits)
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

                    var result = order.AddItemToShipment(command.ShipmentId, variant, command.Request.Quantity, isBackordered);
                    if (result.IsError) return result.Errors;

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
                        .Include(o => o.Shipments)
                            .ThenInclude(s => s.InventoryUnits)
                        .FirstOrDefaultAsync(o => o.Id == command.OrderId, ct);

                    if (order == null) return Order.Errors.NotFound(command.OrderId);

                    var result = order.RemoveItemFromShipment(command.ShipmentId, command.Request.VariantId, command.Request.Quantity);
                    if (result.IsError) return result.Errors;

                    await dbContext.SaveChangesAsync(ct);
                    return Result.Success;
                }
            }
        }
    }
}