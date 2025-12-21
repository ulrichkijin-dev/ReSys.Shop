using ReSys.Shop.Core.Domain.Orders.Shipments;

namespace ReSys.Shop.Core.Feature.Admin.Orders;

public static partial class OrderModule
{
    public static partial class Shipments
    {
        public static class Update
        {
            public record Request
            {
                public string? TrackingNumber { get; init; }
            }

            public sealed record Command(Guid OrderId, Guid ShipmentId, Request Request) : ICommand<Success>;

            public sealed class CommandValidator : AbstractValidator<Command>
            {
                public CommandValidator()
                {
                    RuleFor(x => x.OrderId).NotEmpty();
                    RuleFor(x => x.ShipmentId).NotEmpty();
                }
            }

            public sealed class CommandHandler(IApplicationDbContext dbContext)
                : ICommandHandler<Command, Success>
            {
                public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
                {
                    var shipment = await dbContext.Set<Shipment>()
                        .FirstOrDefaultAsync(s => s.Id == command.ShipmentId && s.OrderId == command.OrderId, ct);

                    if (shipment == null) return Shipment.Errors.NotFound(command.ShipmentId);

                    if (command.Request.TrackingNumber != null)
                    {
                        var result = shipment.UpdateTrackingNumber(command.Request.TrackingNumber);
                        if (result.IsError) return result.Errors;
                    }

                    await dbContext.SaveChangesAsync(ct);
                    return Result.Success;
                }
            }
        }
    }
}
