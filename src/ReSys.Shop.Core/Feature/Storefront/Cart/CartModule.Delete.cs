using ReSys.Shop.Core.Domain.Orders;

namespace ReSys.Shop.Core.Feature.Storefront.Cart;

public static partial class CartModule
{
    public static class Delete
    {
        public sealed record Command(string? Token = null) : ICommand<Deleted>;

        public sealed class CommandHandler(IApplicationDbContext dbContext, IUserContext userContext)
            : ICommandHandler<Command, Deleted>
        {
            public async Task<ErrorOr<Deleted>> Handle(Command command, CancellationToken ct)
            {
                var cart = await GetCartAsync(dbContext, userContext, command.Token, ct);

                if (cart == null) return Error.NotFound("Cart.NotFound", "Cart not found.");

                dbContext.Set<Order>().Remove(cart);
                await dbContext.SaveChangesAsync(ct);

                return Result.Deleted;
            }
        }
    }
}
