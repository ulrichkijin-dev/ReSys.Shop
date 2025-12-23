using ReSys.Shop.Core.Common.Domain.Shared;
using ReSys.Shop.Core.Domain.Orders.Payments;
using ReSys.Shop.Core.Domain.Orders.Payments.Gateways;
using ReSys.Shop.Core.Common.Services.Security.Encryptors.Interfaces;
using DomainPaymentMethod = ReSys.Shop.Core.Domain.Settings.PaymentMethods.PaymentMethod;

namespace ReSys.Shop.Infrastructure.Payments.Gateways;

public sealed class PayPalProcessor(ICredentialEncryptor encryptor) : IPaymentProcessor
{
    private readonly ICredentialEncryptor _encryptor = encryptor;

    public DomainPaymentMethod.PaymentType Type => DomainPaymentMethod.PaymentType.PayPal;

    public async Task<ErrorOr<PaymentAuthorizationResult>> CreateIntentAsync(
        Payment payment, 
        Money amount,
        string idempotencyKey,
        CancellationToken ct)
    {
        // Concrete logic: Build PayPal V2 Order via HttpClient
        await Task.CompletedTask;

        var returnUrl = payment.PublicMetadata?.TryGetValue("return_url", out var rUrl) == true ? rUrl?.ToString() : null;
        var cancelUrl = payment.PublicMetadata?.TryGetValue("cancel_url", out var cUrl) == true ? cUrl?.ToString() : null;

        if (string.IsNullOrEmpty(returnUrl) || string.IsNullOrEmpty(cancelUrl))
        {
            return Error.Validation("PayPal.UrlsRequired", "Return and Cancel URLs must be provided.");
        }

        var approvalUrl = $"https://www.paypal.com/checkoutnow?token=MOCK_{Guid.NewGuid()}&return={Uri.EscapeDataString(returnUrl)}&cancel={Uri.EscapeDataString(cancelUrl)}";
        
        var nextAction = new Dictionary<string, string> 
        { 
            ["approval_url"] = approvalUrl 
        };

        return new PaymentAuthorizationResult(
            ProviderReferenceId: $"PAYID-{Guid.NewGuid()}",
            Status: AuthorizationStatus.RequiresAction,
            AuthCode: "PENDING",
            NextActionData: nextAction
        );
    }

    public Task<ErrorOr<Success>> CaptureAsync(Payment payment, string idempotencyKey, CancellationToken ct)
    {
        return Task.FromResult<ErrorOr<Success>>(Result.Success);
    }

    public Task<ErrorOr<Success>> RefundAsync(Payment payment, Money amount, string reason, string idempotencyKey, CancellationToken ct)
    {
        return Task.FromResult<ErrorOr<Success>>(Result.Success);
    }

    public Task<ErrorOr<Success>> VoidAsync(Payment payment, string idempotencyKey, CancellationToken ct)
    {
        return Task.FromResult<ErrorOr<Success>>(Result.Success);
    }

    public ErrorOr<Success> ValidateWebhook(string payload, string signature, string webhookSecret)
    {
        return Result.Success;
    }
}
