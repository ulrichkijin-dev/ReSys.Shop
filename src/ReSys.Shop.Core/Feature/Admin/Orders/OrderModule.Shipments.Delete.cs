using ReSys.Shop.Core.Domain.Orders.Shipments;

namespace ReSys.Shop.Core.Feature.Admin.Orders;

public static partial class OrderModule
{
    public static partial class Shipments
    {
        public static class Delete
        {
            public sealed record Command(Guid OrderId, Guid ShipmentId) : ICommand<Deleted>;

            public sealed class CommandValidator : AbstractValidator<Command>
            {
                public CommandValidator()
                {
                    RuleFor(x => x.OrderId).NotEmpty();
                    RuleFor(x => x.ShipmentId).NotEmpty();
                }
            }

            public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Deleted>
            {
                public async Task<ErrorOr<Deleted>> Handle(Command command, CancellationToken ct)
                {
                    var shipment = await dbContext.Set<Shipment>()
                        .Include(s => s.InventoryUnits)
                        .FirstOrDefaultAsync(s => s.Id == command.ShipmentId && s.OrderId == command.OrderId, ct);

                    if (shipment == null) return Shipment.Errors.NotFound(command.ShipmentId);

                    var result = shipment.Cancel();
                    if (result.IsError) return result.Errors;

                    dbContext.Set<Shipment>().Remove(shipment);
                    await dbContext.SaveChangesAsync(ct);

                    return Result.Deleted;
                }
            }
        }
    }
}