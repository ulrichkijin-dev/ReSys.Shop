using ReSys.Shop.Core.Common.Domain.Shared;
using ReSys.Shop.Core.Domain.Settings.PaymentMethods;

namespace ReSys.Shop.Core.Domain.Orders.Payments.Gateways;

public enum AuthorizationStatus
{
    Authorized,
    Captured,
    RequiresAction,
    Failed,
    Pending
}

public record PaymentAuthorizationResult(
    string ProviderReferenceId,
    AuthorizationStatus Status,
    string? AuthCode = null,
    Dictionary<string, string>? NextActionData = null,
    string? ErrorMessage = null,
    string? ErrorCode = null
);

public interface IPaymentProcessor
{
    PaymentMethod.PaymentType Type { get; }

    Task<ErrorOr<PaymentAuthorizationResult>> CreateIntentAsync(
        Payment payment,
        Money amount,
        string idempotencyKey,
        CancellationToken ct
    );

    Task<ErrorOr<Success>> CaptureAsync(
        Payment payment,
        string idempotencyKey,
        CancellationToken ct
    );

    Task<ErrorOr<Success>> RefundAsync(
        Payment payment,
        Money amount,
        string reason,
        string idempotencyKey,
        CancellationToken ct
    );

    Task<ErrorOr<Success>> VoidAsync(
        Payment payment,
        string idempotencyKey,
        CancellationToken ct
    );

    ErrorOr<Success> ValidateWebhook(
        string payload,
        string signature,
        string webhookSecret
    );
}