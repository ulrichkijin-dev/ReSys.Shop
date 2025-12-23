using MapsterMapper;

using ReSys.Shop.Core.Domain.Orders;
using ReSys.Shop.Core.Domain.Orders.Adjustments;
using ReSys.Shop.Core.Domain.Orders.Payments;
using ReSys.Shop.Core.Domain.Orders.Shipments;
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

        public static class UpdateAddress
        {
            public record Request(ReSys.Shop.Core.Feature.Accounts.Addresses.AddressModule.Model.Param ShipAddress, ReSys.Shop.Core.Feature.Accounts.Addresses.AddressModule.Model.Param? BillAddress = null);
            public sealed record Command(Request Request, string? Token = null) : ICommand<Models.CartDetail>;

            public sealed class CommandHandler(IApplicationDbContext dbContext, IMapper mapper, IUserContext userContext)
                : ICommandHandler<Command, Models.CartDetail>
            {
                public async Task<ErrorOr<Models.CartDetail>> Handle(Command command, CancellationToken ct)
                {
                    var cart = await GetCartAsync(dbContext, userContext, command.Token, ct);
                    if (cart == null) return Error.NotFound("Cart.NotFound");

                    // Map ship address
                    var shipAddr = command.Request.ShipAddress;
                    var shipResult = ReSys.Shop.Core.Domain.Identity.UserAddresses.UserAddress.Create(
                        firstName: shipAddr.FirstName,
                        lastName: shipAddr.LastName,
                        userId: userContext.UserId ?? "Guest",
                        countryId: shipAddr.CountryId,
                        address1: shipAddr.Address1,
                        city: shipAddr.City,
                        zipcode: shipAddr.Zipcode,
                        stateId: shipAddr.StateId,
                        address2: shipAddr.Address2,
                        phone: shipAddr.Phone,
                        company: shipAddr.Company);

                    if (shipResult.IsError) return shipResult.Errors;
                    cart.ShipAddress = shipResult.Value;

                    // Map bill address or use ship address
                    var billAddr = command.Request.BillAddress ?? shipAddr;
                    var billResult = ReSys.Shop.Core.Domain.Identity.UserAddresses.UserAddress.Create(
                        firstName: billAddr.FirstName,
                        lastName: billAddr.LastName,
                        userId: userContext.UserId ?? "Guest",
                        countryId: billAddr.CountryId,
                        address1: billAddr.Address1,
                        city: billAddr.City,
                        zipcode: billAddr.Zipcode,
                        stateId: billAddr.StateId,
                        address2: billAddr.Address2,
                        phone: billAddr.Phone,
                        company: billAddr.Company);

                    if (billResult.IsError) return billResult.Errors;
                    cart.BillAddress = billResult.Value;

                    await dbContext.SaveChangesAsync(ct);
                    return mapper.Map<Models.CartDetail>(cart);
                }
            }
        }

        public static class ListShippingMethods
        {
            public record Result(Guid Id, string Name, string? Presentation, decimal Price, string? Description);

            public sealed record Query(string? Token = null) : IQuery<List<Result>>;

            public sealed class QueryHandler(IApplicationDbContext dbContext, IUserContext userContext)
                : IQueryHandler<Query, List<Result>>
            {
                public async Task<ErrorOr<List<Result>>> Handle(Query request, CancellationToken ct)
                {
                    var cart = await GetCartAsync(dbContext, userContext, request.Token, ct);
                    if (cart == null) return Error.NotFound("Cart.NotFound");

                    // In a real system, you would filter by zone based on cart address
                    var methods = await dbContext.Set<ShippingMethod>()
                        .Where(m => m.Active && (m.DisplayOn == DisplayOn.Both || m.DisplayOn == DisplayOn.Storefront))
                        .OrderBy(m => m.Position)
                        .Select(m => new Result(m.Id, m.Name, m.Presentation, 0, m.Description)) // Price is 0 here as it's calculated later or from rules
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
                    
                    var paymentResult = Payment.Create(
                        orderId: cart.Id, 
                        amountCents: amountCents, 
                        currency: cart.Currency,
                        paymentMethodType: method.MethodCode, 
                        paymentMethodId: method.Id);

                    if (paymentResult.IsError) return paymentResult.Errors;
                    
                    var payment = paymentResult.Value;
                    cart.AddPayment(payment);
                    
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
                            var shipmentResult = Shipment.Create(cart.Id, shipmentPlan.FulfillmentLocationId);
                            if (shipmentResult.IsError) return shipmentResult.Errors;
                            var shipment = shipmentResult.Value;

                            foreach (var fulfillmentItem in shipmentPlan.Items)
                            {
                                var lineItem = cart.LineItems.FirstOrDefault(li => li.Id == fulfillmentItem.LineItemId);
                                if (lineItem == null) continue;

                                var initialState = fulfillmentItem.IsBackordered ? InventoryUnit.InventoryUnitState.Backordered : InventoryUnit.InventoryUnitState.OnHand;
                                for (int i = 0; i < fulfillmentItem.Quantity; i++)
                                {
                                    var unitResult = InventoryUnit.Create(lineItem.VariantId, lineItem.Id, shipment.Id, initialState);
                                    if (unitResult.IsError) return unitResult.Errors;
                                    shipment.InventoryUnits.Add(unitResult.Value);
                                    lineItem.InventoryUnits.Add(unitResult.Value);
                                }
                                cart.AddDomainEvent(new Order.Events.ShipmentItemUpdated(cart.Id, shipment.Id, lineItem.VariantId));
                            }

                            cart.AddShipment(shipment);
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
                                var shipmentResult = Shipment.Create(cart.Id, shipmentPlan.FulfillmentLocationId);
                                if (shipmentResult.IsError) break;
                                var shipment = shipmentResult.Value;

                                foreach (var fulfillmentItem in shipmentPlan.Items)
                                {
                                    var lineItem = cart.LineItems.FirstOrDefault(li => li.Id == fulfillmentItem.LineItemId);
                                    if (lineItem == null) continue;
                                    
                                    var initialState = fulfillmentItem.IsBackordered ? InventoryUnit.InventoryUnitState.Backordered : InventoryUnit.InventoryUnitState.OnHand;
                                    for (int i = 0; i < fulfillmentItem.Quantity; i++)
                                    {
                                        var unitResult = InventoryUnit.Create(lineItem.VariantId, lineItem.Id, shipment.Id, initialState);
                                        if (unitResult.IsError) continue;
                                        shipment.InventoryUnits.Add(unitResult.Value);
                                        lineItem.InventoryUnits.Add(unitResult.Value);
                                    }
                                    cart.AddDomainEvent(new Order.Events.ShipmentItemUpdated(cart.Id, shipment.Id, lineItem.VariantId));
                                }

                                cart.AddShipment(shipment);
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

                    decimal shippingCost = method.CalculateCost(cart.TotalWeight, cart.ItemTotal);
                    cart.SetShipmentTotal(shippingCost * 100);

                    var shippingDescription = method.Presentation ?? method.Name ?? "Shipping";
                    var existingShippingAdj = cart.OrderAdjustments.FirstOrDefault(a => a.Scope == OrderAdjustment.AdjustmentScope.Shipping);
                    
                    if (existingShippingAdj != null)
                    {
                        existingShippingAdj.AmountCents = (long)cart.ShipmentTotalCents;
                        existingShippingAdj.Description = shippingDescription;
                    }
                    else
                    {
                        var shippingAdjResult = OrderAdjustment.Create(
                            orderId: cart.Id,
                            amountCents: (long)cart.ShipmentTotalCents,
                            description: shippingDescription,
                            scope: OrderAdjustment.AdjustmentScope.Shipping,
                            eligible: true,
                            mandatory: true);

                        if (!shippingAdjResult.IsError)
                        {
                            cart.OrderAdjustments.Add(shippingAdjResult.Value);
                        }
                    }

                    cart.RecalculateTotals();

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