using ReSys.Shop.Core.Domain.Orders;
using ReSys.Shop.Core.Domain.Orders.Shipments;
using ReSys.Shop.Core.Domain.Inventories.FulfillmentStrategies;

namespace ReSys.Shop.Core.Feature.Admin.Orders;

public static partial class OrderModule
{
    public static partial class Shipments
    {
        public static class TransferToShipment
        {
            public record Request(Guid TargetShipmentId, Guid VariantId, int Quantity);
            public sealed record Command(Guid OrderId, Guid SourceShipmentId, Request Request) : ICommand<Success>;

            public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Success>
            {
                public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
                {
                    var source = await dbContext.Set<Shipment>()
                        .Include(s => s.InventoryUnits)
                        .FirstOrDefaultAsync(s => s.Id == command.SourceShipmentId && s.OrderId == command.OrderId, ct);
                    
                    var target = await dbContext.Set<Shipment>()
                        .Include(s => s.InventoryUnits)
                        .FirstOrDefaultAsync(s => s.Id == command.Request.TargetShipmentId && s.OrderId == command.OrderId, ct);

                    if (source == null) return Shipment.Errors.NotFound(command.SourceShipmentId);
                    if (target == null) return Shipment.Errors.NotFound(command.Request.TargetShipmentId);

                    var units = source.InventoryUnits
                        .Where(u => u.VariantId == command.Request.VariantId && u.IsPreShipment)
                        .Take(command.Request.Quantity)
                        .ToList();

                    if (units.Count < command.Request.Quantity)
                        return Error.Validation(code: "Shipment.InsufficientUnits", description: "Not enough shippable units in source shipment.");

                    foreach (var unit in units)
                    {
                        unit.ChangeShipment(target.Id);
                        source.InventoryUnits.Remove(unit);
                        target.InventoryUnits.Add(unit);
                    }

                    source.AddDomainEvent(new Order.Events.ShipmentItemUpdated(source.OrderId, source.Id, command.Request.VariantId));
                    target.AddDomainEvent(new Order.Events.ShipmentItemUpdated(target.OrderId, target.Id, command.Request.VariantId));

                    await dbContext.SaveChangesAsync(ct);
                    return Result.Success;
                }
            }
        }

        public static class TransferToLocation
        {
            public record Request(Guid TargetLocationId, Guid VariantId, int Quantity);
            public sealed record Command(Guid OrderId, Guid SourceShipmentId, Request Request) : ICommand<Success>;

            public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Success>
            {
                public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
                {
                    var order = await dbContext.Set<Order>()
                        .Include(o => o.Shipments)
                            .ThenInclude(s => s.InventoryUnits)
                        .Include(o => o.LineItems)
                        .FirstOrDefaultAsync(o => o.Id == command.OrderId, ct);

                    if (order == null) return Order.Errors.NotFound(command.OrderId);

                    var source = order.Shipments.FirstOrDefault(s => s.Id == command.SourceShipmentId);
                    if (source == null) return Shipment.Errors.NotFound(command.SourceShipmentId);

                    var units = source.InventoryUnits
                        .Where(u => u.VariantId == command.Request.VariantId && u.IsPreShipment)
                        .Take(command.Request.Quantity)
                        .ToList();

                    if (units.Count < command.Request.Quantity)
                        return Error.Validation(code: "Shipment.InsufficientUnits", description: "Not enough shippable units in source shipment.");

                    // Create new shipment at target location
                    var fulfillmentItems = new List<FulfillmentItem> { 
                        FulfillmentItem.Create(units[0].LineItemId, command.Request.VariantId, command.Request.Quantity).Value 
                    };
                    
                    var shipmentResult = order.AddShipment(command.Request.TargetLocationId, fulfillmentItems);
                    if (shipmentResult.IsError) return shipmentResult.Errors;
                    
                    // Remove units from source
                    foreach (var unit in units)
                    {
                        source.InventoryUnits.Remove(unit);
                        dbContext.Set<InventoryUnit>().Remove(unit);
                    }

                    order.AddDomainEvent(new Order.Events.ShipmentItemUpdated(order.Id, source.Id, command.Request.VariantId));

                    await dbContext.SaveChangesAsync(ct);
                    return Result.Success;
                }
            }
        }
    }
}