using MapsterMapper;

using ReSys.Shop.Core.Domain.Catalog.Products.Variants;

namespace ReSys.Shop.Core.Feature.Storefront.Cart;

public static partial class CartModule
{
    public static class Items
    {
        public static class Add
        {
            public record Request(Guid VariantId, int Quantity);
            public sealed record Command(Request Request, string? Token = null) : ICommand<Models.CartDetail>;

            public sealed class CommandHandler(IApplicationDbContext dbContext, IMapper mapper, IUserContext userContext)
                : ICommandHandler<Command, Models.CartDetail>
            {
                public async Task<ErrorOr<Models.CartDetail>> Handle(Command command, CancellationToken ct)
                {
                    var cart = await GetCartAsync(dbContext, userContext, command.Token, ct);

                    if (cart == null) return Error.NotFound("Cart.NotFound", "Cart not found.");

                    var variant = await dbContext.Set<Variant>()
                        .Include(v => v.Product)
                        .FirstOrDefaultAsync(v => v.Id == command.Request.VariantId, ct);

                    if (variant == null) return Error.NotFound("Variant.NotFound", "Variant not found.");

                    var result = cart.AddLineItem(variant, command.Request.Quantity);
                    if (result.IsError) return result.Errors;

                    await dbContext.SaveChangesAsync(ct);
                    return mapper.Map<Models.CartDetail>(cart);
                }
            }
        }

        public static class SetQuantity
        {
            public record Request(Guid LineItemId, int Quantity);
            public sealed record Command(Request Request, string? Token = null) : ICommand<Models.CartDetail>;

            public sealed class CommandHandler(IApplicationDbContext dbContext, IMapper mapper, IUserContext userContext)
                : ICommandHandler<Command, Models.CartDetail>
            {
                public async Task<ErrorOr<Models.CartDetail>> Handle(Command command, CancellationToken ct)
                {
                    var cart = await GetCartAsync(dbContext, userContext, command.Token, ct);

                    if (cart == null) return Error.NotFound("Cart.NotFound", "Cart not found.");

                    var result = cart.UpdateLineItemQuantity(command.Request.LineItemId, command.Request.Quantity);
                    if (result.IsError) return result.Errors;

                    await dbContext.SaveChangesAsync(ct);
                    return mapper.Map<Models.CartDetail>(cart);
                }
            }
        }

        public static class Remove
        {
            public sealed record Command(Guid LineItemId, string? Token = null) : ICommand<Models.CartDetail>;

            public sealed class CommandHandler(IApplicationDbContext dbContext, IMapper mapper, IUserContext userContext)
                : ICommandHandler<Command, Models.CartDetail>
            {
                public async Task<ErrorOr<Models.CartDetail>> Handle(Command command, CancellationToken ct)
                {
                    var cart = await GetCartAsync(dbContext, userContext, command.Token, ct);

                    if (cart == null) return Error.NotFound("Cart.NotFound", "Cart not found.");

                    var result = cart.RemoveLineItem(command.LineItemId);
                    if (result.IsError) return result.Errors;

                    await dbContext.SaveChangesAsync(ct);
                    return mapper.Map<Models.CartDetail>(cart);
                }
            }
        }
    }
}