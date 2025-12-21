using MapsterMapper;

namespace ReSys.Shop.Core.Feature.Storefront.Cart;

public static partial class CartModule
{
    public static class Get
    {
        public sealed record Query(string? Token = null) : IQuery<Models.CartDetail>;

        public sealed class QueryHandler(IApplicationDbContext dbContext, IMapper mapper, IUserContext userContext)
            : IQueryHandler<Query, Models.CartDetail>
        {
            public async Task<ErrorOr<Models.CartDetail>> Handle(Query request, CancellationToken ct)
            {
                var cart = await GetCartAsync(dbContext, userContext, request.Token, ct);

                if (cart == null)
                    return Error.NotFound("Cart.NotFound", "No active cart found.");

                return mapper.Map<Models.CartDetail>(cart);
            }
        }
    }
}