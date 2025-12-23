using MapsterMapper;

using ReSys.Shop.Core.Domain.Orders;

using OrderPayments = ReSys.Shop.Core.Domain.Orders.Payments;

using ReSys.Shop.Core.Domain.Orders.Payments.Gateways;
using ReSys.Shop.Core.Domain.Settings.PaymentMethods;
using ReSys.Shop.Core.Common.Domain.Shared;

namespace ReSys.Shop.Core.Feature.Admin.Orders;

public static partial class OrderModule
{
    public static partial class Payments
    {
        public static class Create
        {
            public record Request
            {
                public decimal Amount { get; init; }
                public Guid PaymentMethodId { get; init; }
                public string PaymentMethodType { get; init; } = string.Empty;
            }

            public record Result : Models.PaymentItem;

            public sealed record Command(Guid OrderId, Request Request) : ICommand<Result>;

            public sealed class CommandValidator : AbstractValidator<Command>
            {
                public CommandValidator()
                {
                    RuleFor(x => x.OrderId).NotEmpty();
                    RuleFor(x => x.Request.Amount).GreaterThanOrEqualTo(0);
                    RuleFor(x => x.Request.PaymentMethodId).NotEmpty();
                    RuleFor(x => x.Request.PaymentMethodType).NotEmpty();
                }
            }

            public sealed class CommandHandler(
                IApplicationDbContext dbContext,
                IMapper mapper,
                PaymentProcessorFactory gatewayFactory)
                : ICommandHandler<Command, Result>
            {
                public async Task<ErrorOr<Result>> Handle(Command command, CancellationToken ct)
                {
                    var order = await dbContext.Set<Order>()
                        .FirstOrDefaultAsync(o => o.Id == command.OrderId, ct);

                    if (order == null)
                        return Order.Errors.NotFound(command.OrderId);

                    var paymentMethod = await dbContext.Set<PaymentMethod>()
                        .FirstOrDefaultAsync(pm => pm.Id == command.Request.PaymentMethodId, ct);

                    if (paymentMethod == null)
                        return PaymentMethod.Errors.NotFound(command.Request.PaymentMethodId);

                    if (!paymentMethod.Active)
                        return Error.Validation(code: "PaymentMethod.Inactive", description: "This payment method is not active.");

                    if (paymentMethod.MethodCode != command.Request.PaymentMethodType.ToLowerInvariant())
                        return Error.Validation(code: "PaymentMethod.TypeMismatch", description: "Payment method type mismatch.");

                    var amountCents = (long)(command.Request.Amount * 100);
                    
                    var paymentResult = OrderPayments.Payment.Create(
                        orderId: order.Id, 
                        amountCents: amountCents, 
                        currency: order.Currency,
                        paymentMethodType: command.Request.PaymentMethodType, 
                        paymentMethodId: command.Request.PaymentMethodId);

                    if (paymentResult.IsError) return paymentResult.Errors;
                    var payment = paymentResult.Value;
                    order.AddPayment(payment);

                    payment.PaymentMethod = paymentMethod;

                    // Execute external intent
                    var processorResult = gatewayFactory.GetProcessor(paymentMethod.Type);
                    if (!processorResult.IsError)
                    {
                        var money = Money.Create(command.Request.Amount, order.Currency);
                        var idempotencyKey = $"payment-create-{payment.Id}";

                        var intentResult =
                            await processorResult.Value.CreateIntentAsync(payment, money, idempotencyKey, ct);
                        if (!intentResult.IsError)
                        {
                            var intent = intentResult.Value;
                                                    if (intent.Status == AuthorizationStatus.RequiresAction)
                                                    {
                                                        var actionResult = payment.MarkAsRequiresAction(intent.ProviderReferenceId, intent.NextActionData);
                                                        if (actionResult.IsError) return actionResult.Errors;
                                                    }
                                                    else if (intent.Status == AuthorizationStatus.Captured)
                                                    {
                                                        var captureResult = payment.MarkAsCaptured(intent.ProviderReferenceId);
                                                        if (captureResult.IsError) return captureResult.Errors;
                                                    }
                                                    else
                                                    {
                                                        var authResult = payment.MarkAsAuthorized(intent.ProviderReferenceId, intent.AuthCode);
                                                        if (authResult.IsError) return authResult.Errors;
                                                    }                        }
                    }

                    await dbContext.SaveChangesAsync(ct);
                    return mapper.Map<Result>(payment);
                }
            }
        }

        public static class Authorize
        {
            public record Request(string TransactionId, string? AuthCode);

            public sealed record Command(Guid OrderId, Guid PaymentId, Request Request) : ICommand<Success>;

            public sealed class CommandHandler(IApplicationDbContext dbContext, PaymentProcessorFactory gatewayFactory)
                : ICommandHandler<Command, Success>
            {
                public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
                {
                    var payment = await dbContext.Set<OrderPayments.Payment>()
                        .Include(p => p.PaymentMethod)
                        .Include(p => p.Order)
                        .FirstOrDefaultAsync(p => p.Id == command.PaymentId && p.OrderId == command.OrderId, ct);

                    if (payment == null) return OrderPayments.Payment.Errors.NotFound(command.PaymentId);
                    if (payment.PaymentMethod == null) return PaymentMethod.Errors.Required;

                    var gateway = gatewayFactory.GetProcessor(payment.PaymentMethod.Type);
                    if (gateway.IsError) return gateway.Errors;

                    var money = Money.Create(payment.Amount, payment.Currency);
                    var idempotencyKey = $"payment-auth-{payment.Id}";

                    var result = await gateway.Value.CreateIntentAsync(payment, money, idempotencyKey, ct);
                    if (result.IsError) return result.Errors;

                    var authResult = payment.MarkAsAuthorized(result.Value.ProviderReferenceId, result.Value.AuthCode);
                    if (authResult.IsError) return authResult.Errors;

                    await dbContext.SaveChangesAsync(ct);
                    return Result.Success;
                }
            }
        }

        public static class Capture
        {
            public record Request(string? TransactionId);

            public sealed record Command(Guid OrderId, Guid PaymentId, Request Request) : ICommand<Success>;

            public sealed class CommandHandler(IApplicationDbContext dbContext, PaymentProcessorFactory gatewayFactory)
                : ICommandHandler<Command, Success>
            {
                public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
                {
                    var payment = await dbContext.Set<OrderPayments.Payment>()
                        .Include(p => p.PaymentMethod)
                        .FirstOrDefaultAsync(p => p.Id == command.PaymentId && p.OrderId == command.OrderId, ct);

                    if (payment == null) return OrderPayments.Payment.Errors.NotFound(command.PaymentId);
                    if (payment.PaymentMethod == null) return PaymentMethod.Errors.Required;

                    var gateway = gatewayFactory.GetProcessor(payment.PaymentMethod.Type);
                    if (gateway.IsError) return gateway.Errors;

                    var idempotencyKey = $"payment-capture-{payment.Id}";
                    var result = await gateway.Value.CaptureAsync(payment, idempotencyKey, ct);
                    if (result.IsError) return result.Errors;

                    var captureResult = payment.MarkAsCaptured(command.Request.TransactionId ?? payment.ReferenceTransactionId);
                    if (captureResult.IsError) return captureResult.Errors;

                    await dbContext.SaveChangesAsync(ct);
                    return Result.Success;
                }
            }
        }

        public static class Refund
        {
            public record Request(decimal Amount, string Reason, string? TransactionId);

            public sealed record Command(Guid OrderId, Guid PaymentId, Request Request) : ICommand<Success>;

            public sealed class CommandHandler(IApplicationDbContext dbContext, PaymentProcessorFactory gatewayFactory)
                : ICommandHandler<Command, Success>
            {
                public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
                {
                    var payment = await dbContext.Set<OrderPayments.Payment>()
                        .Include(p => p.PaymentMethod)
                        .FirstOrDefaultAsync(p => p.Id == command.PaymentId && p.OrderId == command.OrderId, ct);

                    if (payment == null) return OrderPayments.Payment.Errors.NotFound(command.PaymentId);
                    if (payment.PaymentMethod == null) return PaymentMethod.Errors.Required;

                    var gateway = gatewayFactory.GetProcessor(payment.PaymentMethod.Type);
                    if (gateway.IsError) return gateway.Errors;

                    var money = Money.Create(command.Request.Amount, payment.Currency);
                    var idempotencyKey = $"payment-refund-{payment.Id}-{Guid.NewGuid()}";
                    var result =
                        await gateway.Value.RefundAsync(payment, money, command.Request.Reason, idempotencyKey, ct);
                    if (result.IsError) return result.Errors;

                    var refundResult = payment.Refund((long)(command.Request.Amount * 100), command.Request.Reason,
                        command.Request.TransactionId ?? payment.ReferenceTransactionId);

                    if (refundResult.IsError) return refundResult.Errors;

                    await dbContext.SaveChangesAsync(ct);
                    return Result.Success;
                }
            }
        }

        public static class Void
        {
            public sealed record Command(Guid OrderId, Guid PaymentId) : ICommand<Success>;

            public sealed class CommandHandler(IApplicationDbContext dbContext, PaymentProcessorFactory gatewayFactory)
                : ICommandHandler<Command, Success>
            {
                public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
                {
                    var payment = await dbContext.Set<OrderPayments.Payment>()
                        .Include(p => p.PaymentMethod)
                        .FirstOrDefaultAsync(p => p.Id == command.PaymentId && p.OrderId == command.OrderId, ct);

                    if (payment == null) return OrderPayments.Payment.Errors.NotFound(command.PaymentId);
                    if (payment.PaymentMethod == null) return PaymentMethod.Errors.Required;

                    var gateway = gatewayFactory.GetProcessor(payment.PaymentMethod.Type);
                    if (gateway.IsError) return gateway.Errors;

                    var idempotencyKey = $"payment-void-{payment.Id}";
                    var result = await gateway.Value.VoidAsync(payment, idempotencyKey, ct);
                    if (result.IsError) return result.Errors;

                    var voidResult = payment.Void();
                    if (voidResult.IsError) return voidResult.Errors;

                    await dbContext.SaveChangesAsync(ct);
                    return Result.Success;
                }
            }
        }
    }
}