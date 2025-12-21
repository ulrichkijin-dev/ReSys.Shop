using MapsterMapper;
using ReSys.Shop.Core.Domain.Orders;

namespace ReSys.Shop.Core.Feature.Admin.Orders;

public static partial class OrderModule
{
    public static class Update
    {
        public record Request
        {
            public string? Email { get; init; }
            public string? SpecialInstructions { get; init; }
        }

        public record Result : Models.ListItem;
        public sealed record Command(Guid Id, Request Request) : ICommand<Result>;

        public sealed class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Id).NotEmpty();
            }
        }

        public sealed class CommandHandler(IApplicationDbContext dbContext, IMapper mapper)
            : ICommandHandler<Command, Result>
        {
            public async Task<ErrorOr<Result>> Handle(Command command, CancellationToken ct)
            {
                var order = await dbContext.Set<Order>().FirstOrDefaultAsync(o => o.Id == command.Id, ct);
                if (order == null) return Order.Errors.NotFound(command.Id);

                if (command.Request.Email != null) order.Email = command.Request.Email;
                if (command.Request.SpecialInstructions != null) order.SpecialInstructions = command.Request.SpecialInstructions;

                order.UpdatedAt = DateTimeOffset.UtcNow;
                await dbContext.SaveChangesAsync(ct);

                return mapper.Map<Result>(order);
            }
        }
    }
}
