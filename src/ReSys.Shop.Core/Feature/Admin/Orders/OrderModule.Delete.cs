using ReSys.Shop.Core.Domain.Orders;

namespace ReSys.Shop.Core.Feature.Admin.Orders;

public static partial class OrderModule
{
    public static class Delete
    {
        public sealed record Command(Guid Id) : ICommand<Deleted>;

        public sealed class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Id).NotEmpty();
            }
        }

        public sealed class CommandHandler(IApplicationDbContext dbContext) : ICommandHandler<Command, Deleted>
        {
            public async Task<ErrorOr<Deleted>> Handle(Command command, CancellationToken ct)
            {
                var order = await dbContext.Set<Order>().FirstOrDefaultAsync(o => o.Id == command.Id, ct);
                if (order == null) return Order.Errors.NotFound(command.Id);

                // Check if can be deleted (usually only if in Cart or Canceled state)
                if (order.State != Order.OrderState.Cart && order.State != Order.OrderState.Canceled)
                {
                    return Error.Validation(code: "Order.CannotDelete", description: "Only orders in Cart or Canceled state can be deleted.");
                }

                dbContext.Set<Order>().Remove(order);
                await dbContext.SaveChangesAsync(ct);

                return Result.Deleted;
            }
        }
    }
}