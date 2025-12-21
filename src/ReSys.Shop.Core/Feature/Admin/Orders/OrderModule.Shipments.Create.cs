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

                    var fulfillmentItems = command.Request.Items.Select(i => FulfillmentItem.Create(
                        lineItemId: i.LineItemId,
                        variantId: i.VariantId,
                        quantity: i.Quantity
                    ).Value).ToList();

                    var shipmentResult = order.AddShipment(command.Request.StockLocationId, fulfillmentItems);
                    if (shipmentResult.IsError) return shipmentResult.Errors;

                    await dbContext.SaveChangesAsync(ct);
                    return mapper.Map<Result>(shipmentResult.Value);
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
                        var shipmentResult = order.AddShipment(shipmentPlan.FulfillmentLocationId, shipmentPlan.Items);
                        if (shipmentResult.IsError) return shipmentResult.Errors;
                        createdShipments.Add(shipmentResult.Value);
                    }

                    await dbContext.SaveChangesAsync(ct);
                    return mapper.Map<List<Result>>(createdShipments);
                }
            }
        }
    }
}