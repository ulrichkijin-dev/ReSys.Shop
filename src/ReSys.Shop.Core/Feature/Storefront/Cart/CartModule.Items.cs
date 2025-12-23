using MapsterMapper;

using ReSys.Shop.Core.Domain.Catalog.Products.Variants;
using ReSys.Shop.Core.Domain.Orders.LineItems;

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

                    // Refactored: "Transaction Script" style - Create sub-models in Handler
                    var existingLineItem = cart.LineItems.FirstOrDefault(li => li.VariantId == variant.Id);
                    if (existingLineItem != null)
                    {
                        var updateResult = existingLineItem.UpdateQuantity(existingLineItem.Quantity + command.Request.Quantity);
                        if (updateResult.IsError) return updateResult.Errors;
                    }
                    else
                    {
                        // Handler orchestrates creation of the dependent entity
                        var lineItemResult = LineItem.Create(
                            orderId: cart.Id,
                            variant: variant,
                            quantity: command.Request.Quantity,
                            currency: cart.Currency);

                        if (lineItemResult.IsError) return lineItemResult.Errors;
                        
                        // Handler adds it to the Aggregate
                        cart.LineItems.Add(lineItemResult.Value);
                    }

                    // Handler ensures consistency
                    var recalcResult = cart.RecalculateTotals();
                    if (recalcResult.IsError) return recalcResult.Errors;

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