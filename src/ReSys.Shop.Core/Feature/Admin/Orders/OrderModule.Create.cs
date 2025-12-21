using MapsterMapper;
using ReSys.Shop.Core.Domain.Orders;

namespace ReSys.Shop.Core.Feature.Admin.Orders;

public static partial class OrderModule
{
    public static class Create
    {
        public record Request
        {
            public Guid StoreId { get; init; }
            public string Currency { get; init; } = "USD";
            public string? UserId { get; init; }
            public string? Email { get; init; }
        }

        public record Result : Models.ListItem;
        public sealed record Command(Request Request) : ICommand<Result>;

        public sealed class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Request.StoreId).NotEmpty();
                RuleFor(x => x.Request.Currency).NotEmpty().Length(3);
            }
        }

        public sealed class CommandHandler(IApplicationDbContext dbContext, IMapper mapper)
            : ICommandHandler<Command, Result>
        {
            public async Task<ErrorOr<Result>> Handle(Command command, CancellationToken ct)
            {
                var result = Order.Create(
                    command.Request.StoreId,
                    command.Request.Currency,
                    command.Request.UserId,
                    null,
                    command.Request.Email);

                if (result.IsError) return result.Errors;

                dbContext.Set<Order>().Add(result.Value);
                await dbContext.SaveChangesAsync(ct);

                return mapper.Map<Result>(result.Value);
            }
        }
    }
}