using ReSys.Shop.Core.Common.Domain.Shared;
using ReSys.Shop.Core.Domain.Orders.Payments;
using ReSys.Shop.Core.Domain.Orders.Payments.Gateways;
using DomainPaymentMethod = ReSys.Shop.Core.Domain.Settings.PaymentMethods.PaymentMethod;

namespace ReSys.Shop.Infrastructure.Payments.Gateways;

public sealed class CodProcessor : IPaymentProcessor
{
    public DomainPaymentMethod.PaymentType Type => DomainPaymentMethod.PaymentType.CashOnDelivery;

    public async Task<ErrorOr<PaymentAuthorizationResult>> CreateIntentAsync(
        Payment payment, 
        Money amount,
        string idempotencyKey,
        CancellationToken ct)
    {
        await Task.CompletedTask;
        return new PaymentAuthorizationResult(
            ProviderReferenceId: $"COD-{payment.Id}",
            Status: AuthorizationStatus.Pending,
            AuthCode: "COD"
        );
    }

    public Task<ErrorOr<Success>> CaptureAsync(Payment payment, string idempotencyKey, CancellationToken ct) 
        => Task.FromResult<ErrorOr<Success>>(Result.Success);

    public Task<ErrorOr<Success>> RefundAsync(Payment payment, Money amount, string reason, string idempotencyKey, CancellationToken ct)
        => Task.FromResult<ErrorOr<Success>>(Result.Success);

    public Task<ErrorOr<Success>> VoidAsync(Payment payment, string idempotencyKey, CancellationToken ct)
        => Task.FromResult<ErrorOr<Success>>(Result.Success);

    public ErrorOr<Success> ValidateWebhook(string payload, string signature, string webhookSecret)
        => Error.Failure("COD.NoWebhooks");
}
