using MapsterMapper;

using ReSys.Shop.Core.Domain.Orders;
using ReSys.Shop.Core.Domain.Settings.ShippingMethods;
using ReSys.Shop.Core.Domain.Inventories.FulfillmentStrategies;
using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Settings.PaymentMethods;
using ReSys.Shop.Core.Domain.Orders.Payments.Gateways;

using Microsoft.Extensions.Options;

using ReSys.Shop.Core.Common.Options.Systems;
using ReSys.Shop.Core.Common.Domain.Shared;

namespace ReSys.Shop.Core.Feature.Storefront.Cart;

public static partial class CartModule
{
    public static class Checkout
    {
        public static class GetSummary
        {
            public record Result : Models.CartDetail;

            public sealed record Query(string? Token = null) : IQuery<Result>;

            public sealed class QueryHandler(IApplicationDbContext dbContext, IMapper mapper, IUserContext userContext)
                : IQueryHandler<Query, Result>
            {
                public async Task<ErrorOr<Result>> Handle(Query request, CancellationToken ct)
                {
                    var cart = await GetCartAsync(dbContext, userContext, request.Token, ct);
                    if (cart == null) return Error.NotFound("Cart.NotFound");

                    var result = mapper.Map<Result>(cart);
                    return result;
                }
            }
        }

        public static class ListPaymentMethods
        {
            public record Result(Guid Id, string Name, string Type, string? Description);

            public sealed record Query(string? Token = null) : IQuery<List<Result>>;

            public sealed class QueryHandler(IApplicationDbContext dbContext, IUserContext userContext)
                : IQueryHandler<Query, List<Result>>
            {
                public async Task<ErrorOr<List<Result>>> Handle(Query request, CancellationToken ct)
                {
                    var cart = await GetCartAsync(dbContext, userContext, request.Token, ct);
                    if (cart == null) return Error.NotFound("Cart.NotFound");

                    var methods = await dbContext.Set<PaymentMethod>()
                        .Where(m => m.Active && (m.DisplayOn == DisplayOn.Both || m.DisplayOn == DisplayOn.Storefront))
                        .OrderBy(m => m.Position)
                        .Select(m => new Result(m.Id, m.Presentation, m.Type.ToString(), m.Description))
                        .ToListAsync(ct);

                    return methods;
                }
            }
        }

        public static class AddPayment
        {
            public record Request(
                Guid PaymentMethodId,
                decimal Amount,
                string? ReturnUrl = null,
                string? CancelUrl = null);

            public sealed record Command(Request Request, string? Token = null) : ICommand<Models.CartDetail>;

            public sealed class CommandHandler(
                IApplicationDbContext dbContext,
                IMapper mapper,
                IUserContext userContext,
                PaymentProcessorFactory gatewayFactory,
                IOptions<StorefrontOption> storefrontOptions)
                : ICommandHandler<Command, Models.CartDetail>
            {
                public async Task<ErrorOr<Models.CartDetail>> Handle(Command command, CancellationToken ct)
                {
                    var cart = await GetCartAsync(dbContext, userContext, command.Token, ct);
                    if (cart == null) return Error.NotFound("Cart.NotFound");

                    var method = await dbContext.Set<PaymentMethod>()
                        .FirstOrDefaultAsync(m => m.Id == command.Request.PaymentMethodId, ct);
                    if (method == null) return Error.NotFound("PaymentMethod.NotFound");

                    var amountCents = (long)(command.Request.Amount * 100);
                    var paymentResult = cart.AddPayment(amountCents, method.Id, method.MethodCode);
                    if (paymentResult.IsError) return paymentResult.Errors;

                    var payment = paymentResult.Value;
                    payment.PaymentMethod = method;

                    var baseUrl = storefrontOptions.Value.BaseUrl.TrimEnd('/');
                    var returnUrl = command.Request.ReturnUrl ??
                                    $"{baseUrl}/checkout/order-summary?number={cart.Number}";
                    var cancelUrl = command.Request.CancelUrl ?? $"{baseUrl}/checkout/payment";

                    payment.SetPrivate("return_url", returnUrl);
                    payment.SetPrivate("cancel_url", cancelUrl);

                    var gatewayResult = gatewayFactory.GetProcessor(method.Type);
                    if (!gatewayResult.IsError)
                    {
                        var money = Money.Create(command.Request.Amount, cart.Currency);
                        var idempotencyKey = $"checkout-payment-{payment.Id}";

                        var intentResult =
                            await gatewayResult.Value.CreateIntentAsync(payment, money, idempotencyKey, ct);
                        if (!intentResult.IsError)
                        {
                            var intent = intentResult.Value;
                            if (intent.Status == AuthorizationStatus.RequiresAction)
                                payment.MarkAsRequiresAction(intent.ProviderReferenceId, intent.NextActionData);
                            else
                                payment.MarkAsAuthorized(intent.ProviderReferenceId, intent.AuthCode);
                        }
                    }

                    await dbContext.SaveChangesAsync(ct);
                    return mapper.Map<Models.CartDetail>(cart);
                }
            }
        }

        public static class Update
        {
            public record Request
            {
                public string? Email { get; init; }
            }

            public sealed record Command(Request Request, string? Token = null) : ICommand<Models.CartDetail>;

            public sealed class CommandHandler(
                IApplicationDbContext dbContext,
                IMapper mapper,
                IUserContext userContext)
                : ICommandHandler<Command, Models.CartDetail>
            {
                public async Task<ErrorOr<Models.CartDetail>> Handle(Command command, CancellationToken ct)
                {
                    var cart = await GetCartAsync(dbContext, userContext, command.Token, ct);
                    if (cart == null) return Error.NotFound("Cart.NotFound");
                    if (command.Request.Email != null) cart.Email = command.Request.Email;
                    await dbContext.SaveChangesAsync(ct);
                    return mapper.Map<Models.CartDetail>(cart);
                }
            }
        }

        public static class Next
        {
            public sealed record Command(string? Token = null) : ICommand<Models.CartDetail>;

            public sealed class CommandHandler(
                IApplicationDbContext dbContext,
                IMapper mapper,
                IUserContext userContext,
                IFulfillmentPlanner fulfillmentPlanner)
                : ICommandHandler<Command, Models.CartDetail>
            {
                public async Task<ErrorOr<Models.CartDetail>> Handle(Command command, CancellationToken ct)
                {
                    var cart = await GetCartAsync(dbContext, userContext, command.Token, ct);
                    if (cart == null) return Error.NotFound("Cart.NotFound");

                    if (cart.State == Order.OrderState.Address)
                    {
                        var planResult = await fulfillmentPlanner.PlanFulfillment(cart, "HighestStock", ct);
                        if (planResult.IsError) return planResult.Errors;
                        cart.Shipments.Clear();
                        foreach (var shipmentPlan in planResult.Value.Shipments)
                        {
                            var shipmentResult =
                                cart.AddShipment(shipmentPlan.FulfillmentLocationId, shipmentPlan.Items);
                            if (shipmentResult.IsError) return shipmentResult.Errors;
                        }
                    }

                    var result = cart.Next();
                    if (result.IsError) return result.Errors;
                    await dbContext.SaveChangesAsync(ct);
                    return mapper.Map<Models.CartDetail>(cart);
                }
            }
        }

        public static class Advance
        {
            public sealed record Command(string? Token = null) : ICommand<Models.CartDetail>;

            public sealed class CommandHandler(
                IApplicationDbContext dbContext,
                IMapper mapper,
                IUserContext userContext,
                IFulfillmentPlanner fulfillmentPlanner)
                : ICommandHandler<Command, Models.CartDetail>
            {
                public async Task<ErrorOr<Models.CartDetail>> Handle(Command command, CancellationToken ct)
                {
                    var cart = await GetCartAsync(dbContext, userContext, command.Token, ct);
                    if (cart == null) return Error.NotFound("Cart.NotFound");

                    bool advanced = true;
                    while (advanced && cart.State < Order.OrderState.Payment)
                    {
                        var currentState = cart.State;
                        if (cart.State == Order.OrderState.Address)
                        {
                            var planResult = await fulfillmentPlanner.PlanFulfillment(cart, "HighestStock", ct);
                            if (planResult.IsError) break;
                            cart.Shipments.Clear();
                            foreach (var shipmentPlan in planResult.Value.Shipments)
                            {
                                var shipmentResult = cart.AddShipment(shipmentPlan.FulfillmentLocationId,
                                    shipmentPlan.Items);
                                if (shipmentResult.IsError) break;
                            }
                        }

                        var result = cart.Next();
                        if (result.IsError) advanced = false;
                        if (cart.State == currentState) break;
                    }

                    await dbContext.SaveChangesAsync(ct);
                    return mapper.Map<Models.CartDetail>(cart);
                }
            }
        }

        public static class SelectShippingMethod
        {
            public record Request(Guid ShippingMethodId);

            public sealed record Command(Request Request, string? Token = null) : ICommand<Models.CartDetail>;

            public sealed class CommandHandler(
                IApplicationDbContext dbContext,
                IMapper mapper,
                IUserContext userContext)
                : ICommandHandler<Command, Models.CartDetail>
            {
                public async Task<ErrorOr<Models.CartDetail>> Handle(Command command, CancellationToken ct)
                {
                    var cart = await GetCartAsync(dbContext, userContext, command.Token, ct);
                    if (cart == null) return Error.NotFound("Cart.NotFound");
                    var method = await dbContext.Set<ShippingMethod>()
                        .FirstOrDefaultAsync(m => m.Id == command.Request.ShippingMethodId, ct);
                    if (method == null) return Error.NotFound("ShippingMethod.NotFound");
                    var result = cart.SetShippingMethod(method);
                    if (result.IsError) return result.Errors;
                    await dbContext.SaveChangesAsync(ct);
                    return mapper.Map<Models.CartDetail>(cart);
                }
            }
        }

        public static class Complete
        {
            public sealed record Command(string? Token = null) : ICommand<Models.CartDetail>;

            public sealed class CommandHandler(
                IApplicationDbContext dbContext,
                IMapper mapper,
                IUserContext userContext)
                : ICommandHandler<Command, Models.CartDetail>
            {
                public async Task<ErrorOr<Models.CartDetail>> Handle(Command command, CancellationToken ct)
                {
                    var cart = await GetCartAsync(dbContext, userContext, command.Token, ct);
                    if (cart == null) return Error.NotFound("Cart.NotFound");
                    while (cart.State < Order.OrderState.Complete)
                    {
                        var result = cart.Next();
                        if (result.IsError) return result.Errors;
                    }

                    await dbContext.SaveChangesAsync(ct);
                    return mapper.Map<Models.CartDetail>(cart);
                }
            }
        }
    }
}