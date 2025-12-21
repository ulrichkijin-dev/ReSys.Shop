using MapsterMapper;

using ReSys.Shop.Core.Domain.Orders;

namespace ReSys.Shop.Core.Feature.Storefront.Cart;

public static partial class CartModule
{
    public static class Create
    {
        public record Request
        {
            public Guid StoreId { get; init; }
            public string Currency { get; init; } = "USD";
        }

        public sealed record Command(Request Request) : ICommand<Models.CartDetail>;

        public sealed class CommandHandler(IApplicationDbContext dbContext, IMapper mapper, IUserContext userContext)
            : ICommandHandler<Command, Models.CartDetail>
        {
            public async Task<ErrorOr<Models.CartDetail>> Handle(Command command, CancellationToken ct)
            {
                var userId = userContext.UserId;
                var adhocCustomerId = userContext.AdhocCustomerId;
                
                // Check if user already has a cart
                var existingCart = await dbContext.Set<Order>()
                    .Where(o => o.UserId == userId && o.State == Order.OrderState.Cart)
                    .AnyAsync(ct);

                if (existingCart)
                    return Error.Conflict("Cart.AlreadyExists", "User already has an active cart.");

                var result = Order.Create(
                    command.Request.StoreId,
                    command.Request.Currency,
                    userId,
                    adhocCustomerId);

                if (result.IsError) return result.Errors;

                dbContext.Set<Order>().Add(result.Value);
                await dbContext.SaveChangesAsync(ct);

                return mapper.Map<Models.CartDetail>(result.Value);
            }
        }
    }
}
