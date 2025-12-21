using ReSys.Shop.Core.Domain.Orders;
using ReSys.Shop.Core.Domain.Promotions.Promotions;

namespace ReSys.Shop.Core.Feature.Admin.Orders;

public static partial class OrderModule
{
    public static class Actions
    {
        public static class Advance
        {
            public sealed record Command(Guid Id) : ICommand<Success>;

            public sealed class CommandValidator : AbstractValidator<Command>
            {
                public CommandValidator() { RuleFor(x => x.Id).NotEmpty(); }
            }

            public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Success>
            {
                public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
                {
                    var order = await dbContext.Set<Order>()
                        .Include(o => o.LineItems)
                        .Include(o => o.ShipAddress)
                        .Include(o => o.BillAddress)
                        .Include(o => o.Shipments)
                        .Include(o => o.Payments)
                        .FirstOrDefaultAsync(o => o.Id == command.Id, ct);

                    if (order == null) return Order.Errors.NotFound(command.Id);

                    var result = order.Next();
                    if (result.IsError) return result.Errors;

                    await dbContext.SaveChangesAsync(ct);
                    return Result.Success;
                }
            }
        }

        public static class Next
        {
            public sealed record Command(Guid Id) : ICommand<Success>;

            public sealed class CommandValidator : AbstractValidator<Command>
            {
                public CommandValidator() { RuleFor(x => x.Id).NotEmpty(); }
            }

            public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Success>
            {
                public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
                {
                    var order = await dbContext.Set<Order>()
                        .Include(o => o.LineItems)
                        .Include(o => o.ShipAddress)
                        .Include(o => o.BillAddress)
                        .Include(o => o.Shipments)
                        .Include(o => o.Payments)
                        .FirstOrDefaultAsync(o => o.Id == command.Id, ct);

                    if (order == null) return Order.Errors.NotFound(command.Id);

                    var result = order.Next();
                    if (result.IsError) return result.Errors;

                    await dbContext.SaveChangesAsync(ct);
                    return Result.Success;
                }
            }
        }

        public static class Complete
        {
            public sealed record Command(Guid Id) : ICommand<Success>;

            public sealed class CommandValidator : AbstractValidator<Command>
            {
                public CommandValidator() { RuleFor(x => x.Id).NotEmpty(); }
            }

            public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Success>
            {
                public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
                {
                    var order = await dbContext.Set<Order>()
                        .Include(o => o.LineItems)
                        .Include(o => o.ShipAddress)
                        .Include(o => o.BillAddress)
                        .Include(o => o.Shipments)
                        .Include(o => o.Payments)
                        .FirstOrDefaultAsync(o => o.Id == command.Id, ct);

                    if (order == null) return Order.Errors.NotFound(command.Id);

                    // If not already in Confirm state, try to transition to it
                    while (order.State < Order.OrderState.Confirm)
                    {
                        var next = order.Next();
                        if (next.IsError) return next.Errors;
                    }

                    var result = order.Next(); // Transition from Confirm to Complete
                    if (result.IsError) return result.Errors;

                    await dbContext.SaveChangesAsync(ct);
                    return Result.Success;
                }
            }
        }

        public static class Empty
        {
            public sealed record Command(Guid Id) : ICommand<Success>;

            public sealed class CommandValidator : AbstractValidator<Command>
            {
                public CommandValidator() { RuleFor(x => x.Id).NotEmpty(); }
            }

            public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Success>
            {
                public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
                {
                    var order = await dbContext.Set<Order>()
                        .Include(o => o.LineItems)
                        .FirstOrDefaultAsync(o => o.Id == command.Id, ct);

                    if (order == null) return Order.Errors.NotFound(command.Id);

                    var result = order.Empty();
                    if (result.IsError) return result.Errors;

                    await dbContext.SaveChangesAsync(ct);
                    return Result.Success;
                }
            }
        }

        public static class Approve
        {
            public sealed record Command(Guid Id) : ICommand<Success>;

            public sealed class CommandValidator : AbstractValidator<Command>
            {
                public CommandValidator() { RuleFor(x => x.Id).NotEmpty(); }
            }

            public sealed class CommandHandler(IApplicationDbContext dbContext, IUserContext userContext)
                : ICommandHandler<Command, Success>
            {
                public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
                {
                    var order = await dbContext.Set<Order>().FirstOrDefaultAsync(o => o.Id == command.Id, ct);
                    if (order == null) return Order.Errors.NotFound(command.Id);

                    var result = order.Approve(userContext.UserId ?? "System");
                    if (result.IsError) return result.Errors;

                    await dbContext.SaveChangesAsync(ct);
                    return Result.Success;
                }
            }
        }

        public static class Cancel
        {
            public sealed record Command(Guid Id) : ICommand<Success>;

            public sealed class CommandValidator : AbstractValidator<Command>
            {
                public CommandValidator() { RuleFor(x => x.Id).NotEmpty(); }
            }

            public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Success>
            {
                public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
                {
                    var order = await dbContext.Set<Order>()
                        .Include(o => o.Shipments)
                        .FirstOrDefaultAsync(o => o.Id == command.Id, ct);

                    if (order == null) return Order.Errors.NotFound(command.Id);

                    var result = order.Cancel();
                    if (result.IsError) return result.Errors;

                    await dbContext.SaveChangesAsync(ct);
                    return Result.Success;
                }
            }
        }

        public static class ApplyCoupon
        {
            public record Request(string CouponCode);

            public sealed record Command(Guid Id, Request Request) : ICommand<Success>;

            public sealed class CommandValidator : AbstractValidator<Command>
            {
                public CommandValidator()
                {
                    RuleFor(x => x.Id).NotEmpty();
                    RuleFor(x => x.Request.CouponCode).NotEmpty();
                }
            }

            public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Success>
            {
                public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
                {
                    var order = await dbContext.Set<Order>()
                        .Include(o => o.LineItems)
                        .Include(o => o.OrderAdjustments)
                        .FirstOrDefaultAsync(o => o.Id == command.Id, ct);

                    if (order == null) return Order.Errors.NotFound(command.Id);

                    var promotion = await dbContext.Set<Promotion>()
                        .Include(p => p.PromotionRules)
                        .FirstOrDefaultAsync(
                            p => p.PromotionCode == command.Request.CouponCode.ToUpperInvariant() && p.Active, ct);

                    if (promotion == null) return Promotion.Errors.InvalidCode;

                    var result = order.ApplyPromotion(promotion, command.Request.CouponCode);
                    if (result.IsError) return result.Errors;

                    await dbContext.SaveChangesAsync(ct);
                    return Result.Success;
                }
            }
        }
    }
}