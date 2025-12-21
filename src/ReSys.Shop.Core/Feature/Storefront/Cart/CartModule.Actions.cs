using MapsterMapper;
using ReSys.Shop.Core.Domain.Orders;
using ReSys.Shop.Core.Domain.Promotions.Promotions;

namespace ReSys.Shop.Core.Feature.Storefront.Cart;

public static partial class CartModule
{
    public static class Actions
    {
        public static class Empty
        {
            public sealed record Command(string? Token = null) : ICommand<Models.CartDetail>;

            public sealed class CommandHandler(IApplicationDbContext dbContext, IMapper mapper, IUserContext userContext)
                : ICommandHandler<Command, Models.CartDetail>
            {
                public async Task<ErrorOr<Models.CartDetail>> Handle(Command command, CancellationToken ct)
                {
                    var cart = await GetCartAsync(dbContext, userContext, command.Token, ct);

                    if (cart == null) return Error.NotFound("Cart.NotFound", "Cart not found.");

                    var result = cart.Empty();
                    if (result.IsError) return result.Errors;

                    await dbContext.SaveChangesAsync(ct);
                    return mapper.Map<Models.CartDetail>(cart);
                }
            }
        }

        public static class ApplyCoupon
        {
            public record Request(string CouponCode);
            public sealed record Command(Request Request, string? Token = null) : ICommand<Models.CartDetail>;

            public sealed class CommandHandler(IApplicationDbContext dbContext, IMapper mapper, IUserContext userContext)
                : ICommandHandler<Command, Models.CartDetail>
            {
                public async Task<ErrorOr<Models.CartDetail>> Handle(Command command, CancellationToken ct)
                {
                    var cart = await GetCartAsync(dbContext, userContext, command.Token, ct);

                    if (cart == null) return Error.NotFound("Cart.NotFound", "Cart not found.");

                    var promotion = await dbContext.Set<Promotion>()
                        .Include(p => p.PromotionRules)
                        .FirstOrDefaultAsync(p => p.PromotionCode == command.Request.CouponCode.ToUpperInvariant() && p.Active, ct);

                    if (promotion == null) return Promotion.Errors.InvalidCode;

                    var result = cart.ApplyPromotion(promotion, command.Request.CouponCode);
                    if (result.IsError) return result.Errors;

                    await dbContext.SaveChangesAsync(ct);
                    return mapper.Map<Models.CartDetail>(cart);
                }
            }
        }

        public static class Associate
        {
            public sealed record Command(string? Token = null) : ICommand<Models.CartDetail>;

            public sealed class CommandHandler(IApplicationDbContext dbContext, IMapper mapper, IUserContext userContext)
                : ICommandHandler<Command, Models.CartDetail>
            {
                public async Task<ErrorOr<Models.CartDetail>> Handle(Command command, CancellationToken ct)
                {
                    var userId = userContext.UserId;
                    if (userId == null) return Error.Unauthorized("User must be logged in to associate cart.");

                    var guestCart = await GetCartAsync(dbContext, userContext, command.Token, ct);
                    if (guestCart == null) return Error.NotFound("Guest cart not found.");

                    // Merge guest cart into user cart or assign guest cart to user
                    var userCart = await dbContext.Set<Order>()
                        .Where(o => o.UserId == userId && o.State == Order.OrderState.Cart)
                        .FirstOrDefaultAsync(ct);

                    if (userCart != null && userCart.Id != guestCart.Id)
                    {
                        // Logic to merge line items from guestCart to userCart
                        // For now, let's just assign guestCart to user if user has no cart
                        return Error.Conflict("User already has an active cart. Merging not implemented.");
                    }

                    guestCart.AssignToUser(userId);
                    await dbContext.SaveChangesAsync(ct);

                    return mapper.Map<Models.CartDetail>(guestCart);
                }
            }
        }
    }
}
