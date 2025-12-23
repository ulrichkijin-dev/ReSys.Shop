using MapsterMapper;
using ReSys.Shop.Core.Domain.Orders;
using ReSys.Shop.Core.Domain.Inventories.FulfillmentStrategies;
using ReSys.Shop.Core.Domain.Orders.Shipments;

namespace ReSys.Shop.Core.Feature.Admin.Orders;

public static partial class OrderModule
{
    public static partial class Shipments
    {
        public static class Create
        {
            public record Request
            {
                public Guid StockLocationId { get; init; }
                public List<FulfillmentItemRequest> Items { get; init; } = [];
            }

            public record FulfillmentItemRequest(Guid LineItemId, Guid VariantId, int Quantity);

            public record Result : Models.ShipmentItem;
            public sealed record Command(Guid OrderId, Request Request) : ICommand<Result>;

            public sealed class CommandValidator : AbstractValidator<Command>
            {
                public CommandValidator()
                {
                    RuleFor(x => x.OrderId).NotEmpty();
                    RuleFor(x => x.Request.StockLocationId).NotEmpty();
                    RuleFor(x => x.Request.Items).NotEmpty();
                }
            }

            public sealed class CommandHandler(IApplicationDbContext dbContext, IMapper mapper)
                : ICommandHandler<Command, Result>
            {
                public async Task<ErrorOr<Result>> Handle(Command command, CancellationToken ct)
                {
                    var order = await dbContext.Set<Order>()
                        .Include(o => o.LineItems)
                        .ThenInclude(li => li.InventoryUnits)
                        .Include(o => o.Shipments)
                        .FirstOrDefaultAsync(o => o.Id == command.OrderId, ct);

                    if (order == null)
                        return Order.Errors.NotFound(command.OrderId);

                    // Refactored: Create Shipment and its units in Handler
                    var shipmentResult = Shipment.Create(order.Id, command.Request.StockLocationId);
                    if (shipmentResult.IsError) return shipmentResult.Errors;
                    var shipment = shipmentResult.Value;

                    foreach (var i in command.Request.Items)
                    {
                        var lineItem = order.LineItems.FirstOrDefault(li => li.Id == i.LineItemId);
                        if (lineItem == null) continue;
                        
                        for (int k = 0; k < i.Quantity; k++)
                        {
                            var unitResult = InventoryUnit.Create(lineItem.VariantId, lineItem.Id, shipment.Id, InventoryUnit.InventoryUnitState.OnHand);
                            if (unitResult.IsError) return unitResult.Errors;
                            shipment.InventoryUnits.Add(unitResult.Value);
                            lineItem.InventoryUnits.Add(unitResult.Value);
                        }
                        order.AddDomainEvent(new Order.Events.ShipmentItemUpdated(order.Id, shipment.Id, lineItem.VariantId));
                    }

                    order.AddShipment(shipment);

                    await dbContext.SaveChangesAsync(ct);
                    return mapper.Map<Result>(shipment);
                }
            }
        }

        public static class AutoPlan
        {
            public record Request(string Strategy = "HighestStock");
            public record Result : Models.ShipmentItem;
            public sealed record Command(Guid OrderId, Request Request) : ICommand<List<Result>>;

            public sealed class CommandHandler(IApplicationDbContext dbContext, IMapper mapper, IFulfillmentPlanner fulfillmentPlanner)
                : ICommandHandler<Command, List<Result>>
            {
                public async Task<ErrorOr<List<Result>>> Handle(Command command, CancellationToken ct)
                {
                    var order = await dbContext.Set<Order>()
                        .Include(o => o.LineItems)
                        .Include(o => o.Shipments)
                        .FirstOrDefaultAsync(o => o.Id == command.OrderId, ct);

                    if (order == null) return Order.Errors.NotFound(command.OrderId);

                    var planResult = await fulfillmentPlanner.PlanFulfillment(order, command.Request.Strategy, ct);
                    if (planResult.IsError) return planResult.Errors;

                    var createdShipments = new List<Shipment>();
                    foreach (var shipmentPlan in planResult.Value.Shipments)
                    {
                        // Refactored: Create Shipment and its units in Handler
                        var shipmentResult = Shipment.Create(order.Id, shipmentPlan.FulfillmentLocationId);
                        if (shipmentResult.IsError) return shipmentResult.Errors;
                        var shipment = shipmentResult.Value;

                        foreach (var fulfillmentItem in shipmentPlan.Items)
                        {
                            var lineItem = order.LineItems.FirstOrDefault(li => li.Id == fulfillmentItem.LineItemId);
                            if (lineItem == null) continue;
                            
                            var initialState = fulfillmentItem.IsBackordered ? InventoryUnit.InventoryUnitState.Backordered : InventoryUnit.InventoryUnitState.OnHand;
                            for (int k = 0; k < fulfillmentItem.Quantity; k++)
                            {
                                var unitResult = InventoryUnit.Create(lineItem.VariantId, lineItem.Id, shipment.Id, initialState);
                                if (unitResult.IsError) return unitResult.Errors;
                                shipment.InventoryUnits.Add(unitResult.Value);
                                lineItem.InventoryUnits.Add(unitResult.Value);
                            }
                            order.AddDomainEvent(new Order.Events.ShipmentItemUpdated(order.Id, shipment.Id, lineItem.VariantId));
                        }

                        order.AddShipment(shipment);
                        createdShipments.Add(shipment);
                    }

                    await dbContext.SaveChangesAsync(ct);
                    return mapper.Map<List<Result>>(createdShipments);
                }
            }
        }
    }
}