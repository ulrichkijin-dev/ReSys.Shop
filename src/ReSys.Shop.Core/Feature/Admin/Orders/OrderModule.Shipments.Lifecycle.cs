using ReSys.Shop.Core.Domain.Orders.Shipments;

namespace ReSys.Shop.Core.Feature.Admin.Orders;

public static partial class OrderModule
{
    public static partial class Shipments
    {
        public static class Ready
        {
            public sealed record Command(Guid OrderId, Guid ShipmentId) : ICommand<Success>;
            public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Success>
            {
                public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
                {
                    var shipment = await dbContext.Set<Shipment>().FirstOrDefaultAsync(s => s.Id == command.ShipmentId && s.OrderId == command.OrderId, ct);
                    if (shipment == null) return Shipment.Errors.NotFound(command.ShipmentId);

                    var result = shipment.Ready();
                    if (result.IsError) return result.Errors;

                    await dbContext.SaveChangesAsync(ct);
                    return Result.Success;
                }
            }
        }

        public static class Ship
        {
            public record Request { public string? TrackingNumber { get; init; } }
            public sealed record Command(Guid OrderId, Guid ShipmentId, Request Request) : ICommand<Success>;
            public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Success>
            {
                public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
                {
                    var shipment = await dbContext.Set<Shipment>()
                        .Include(s => s.InventoryUnits)
                        .FirstOrDefaultAsync(s => s.Id == command.ShipmentId && s.OrderId == command.OrderId, ct);

                    if (shipment == null) return Shipment.Errors.NotFound(command.ShipmentId);

                    var result = shipment.Ship(command.Request.TrackingNumber);
                    if (result.IsError) return result.Errors;

                    await dbContext.SaveChangesAsync(ct);
                    return Result.Success;
                }
            }
        }

        public static class Deliver
        {
            public sealed record Command(Guid OrderId, Guid ShipmentId) : ICommand<Success>;
            public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Success>
            {
                public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
                {
                    var shipment = await dbContext.Set<Shipment>().FirstOrDefaultAsync(s => s.Id == command.ShipmentId && s.OrderId == command.OrderId, ct);
                    if (shipment == null) return Shipment.Errors.NotFound(command.ShipmentId);

                    var result = shipment.Deliver();
                    if (result.IsError) return result.Errors;

                    await dbContext.SaveChangesAsync(ct);
                    return Result.Success;
                }
            }
        }

        public static class Cancel
        {
            public sealed record Command(Guid OrderId, Guid ShipmentId) : ICommand<Deleted>;
            public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Deleted>
            {
                public async Task<ErrorOr<Deleted>> Handle(Command command, CancellationToken ct)
                {
                    var shipment = await dbContext.Set<Shipment>().FirstOrDefaultAsync(s => s.Id == command.ShipmentId && s.OrderId == command.OrderId, ct);
                    if (shipment == null) return Shipment.Errors.NotFound(command.ShipmentId);

                    var result = shipment.Cancel();
                    if (result.IsError) return result.Errors;

                    await dbContext.SaveChangesAsync(ct);
                    return Result.Deleted;
                }
            }
        }

        public static class Resume
        {
            public sealed record Command(Guid OrderId, Guid ShipmentId) : ICommand<Success>;
            public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Success>
            {
                public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
                {
                    var shipment = await dbContext.Set<Shipment>().FirstOrDefaultAsync(s => s.Id == command.ShipmentId && s.OrderId == command.OrderId, ct);
                    if (shipment == null) return Shipment.Errors.NotFound(command.ShipmentId);

                    var result = shipment.Resume();
                    if (result.IsError) return result.Errors;

                    await dbContext.SaveChangesAsync(ct);
                    return Result.Success;
                }
            }
        }

        public static class ToPending
        {
            public sealed record Command(Guid OrderId, Guid ShipmentId) : ICommand<Success>;
            public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Success>
            {
                public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
                {
                    var shipment = await dbContext.Set<Shipment>().FirstOrDefaultAsync(s => s.Id == command.ShipmentId && s.OrderId == command.OrderId, ct);
                    if (shipment == null) return Shipment.Errors.NotFound(command.ShipmentId);

                    var result = shipment.ToPending();
                    if (result.IsError) return result.Errors;

                    await dbContext.SaveChangesAsync(ct);
                    return Result.Success;
                }
            }
        }
    }
}