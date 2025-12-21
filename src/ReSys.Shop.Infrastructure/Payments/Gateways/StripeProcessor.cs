using Stripe;
using ReSys.Shop.Core.Common.Domain.Shared;
using ReSys.Shop.Core.Domain.Orders.Payments;
using ReSys.Shop.Core.Domain.Orders.Payments.Gateways;
using DomainPaymentMethod = ReSys.Shop.Core.Domain.Settings.PaymentMethods.PaymentMethod;

namespace ReSys.Shop.Infrastructure.Payments.Gateways;

public sealed class StripeProcessor(IGatewayCredentialProvider credentialProvider) : IPaymentProcessor
{
    public DomainPaymentMethod.PaymentType Type => DomainPaymentMethod.PaymentType.Stripe;

    private async Task<ErrorOr<RequestOptions>> GetOptionsAsync(Guid? configId, CancellationToken ct)
    {
        if (!configId.HasValue) return Error.Validation("Gateway.ConfigRequired");
        
        var settingsResult = await credentialProvider.GetSettingsAsync<StripeSettings>(configId.Value, ct);
        if (settingsResult.IsError) return settingsResult.Errors;

        return new RequestOptions { ApiKey = settingsResult.Value.ApiKey };
    }

    public async Task<ErrorOr<PaymentAuthorizationResult>> CreateIntentAsync(
        Payment payment, 
        Money amount,
        string idempotencyKey,
        CancellationToken ct)
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = amount.ToMinorUnit(),
            Currency = amount.Currency.ToLowerInvariant(),
            CaptureMethod = payment.PaymentMethod?.AutoCapture == true ? "automatic" : "manual",
            Metadata = new Dictionary<string, string>
            {
                ["payment_id"] = payment.Id.ToString(),
                ["order_id"] = payment.OrderId.ToString()
            }
        };

        var requestOptionsResult = await GetOptionsAsync(payment.PaymentMethod?.GatewayConfigurationId, ct);
        if (requestOptionsResult.IsError) return requestOptionsResult.Errors;

        var requestOptions = requestOptionsResult.Value;
        requestOptions.IdempotencyKey = idempotencyKey;

        var service = new PaymentIntentService();
        try
        {
            var intent = await service.CreateAsync(options, requestOptions, ct);
            
            var status = MapStatus(intent.Status);
            var nextAction = new Dictionary<string, string>();
            
            if (status == AuthorizationStatus.RequiresAction)
            {
                nextAction["client_secret"] = intent.ClientSecret;
            }

            return new PaymentAuthorizationResult(intent.Id, status, intent.Status, nextAction);
        }
        catch (StripeException ex)
        {
            return Error.Failure("Stripe.IntentError", ex.Message);
        }
    }

    public async Task<ErrorOr<Success>> CaptureAsync(Payment payment, string idempotencyKey, CancellationToken ct)
    {
        if (payment.PaymentMethod == null) return Error.Validation("Payment.MethodMissing");
        
        var requestOptionsResult = await GetOptionsAsync(payment.PaymentMethod.GatewayConfigurationId, ct);
        if (requestOptionsResult.IsError) return requestOptionsResult.Errors;

        var requestOptions = requestOptionsResult.Value;
        requestOptions.IdempotencyKey = idempotencyKey;

        var service = new PaymentIntentService();
        try
        {
            await service.CaptureAsync(payment.ReferenceTransactionId, null, requestOptions, ct);
            return Result.Success;
        }
        catch (StripeException ex)
        {
            return Error.Failure("Stripe.CaptureError", ex.Message);
        }
    }

    public async Task<ErrorOr<Success>> RefundAsync(Payment payment, Money amount, string reason, string idempotencyKey, CancellationToken ct)
    {
        if (payment.PaymentMethod == null) return Error.Validation("Payment.MethodMissing");

        var options = new RefundCreateOptions
        {
            PaymentIntent = payment.ReferenceTransactionId,
            Amount = amount.ToMinorUnit(),
            Reason = MapRefundReason(reason)
        };

        var requestOptionsResult = await GetOptionsAsync(payment.PaymentMethod.GatewayConfigurationId, ct);
        if (requestOptionsResult.IsError) return requestOptionsResult.Errors;

        var requestOptions = requestOptionsResult.Value;
        requestOptions.IdempotencyKey = idempotencyKey;

        var service = new RefundService();
        try
        {
            await service.CreateAsync(options, requestOptions, ct);
            return Result.Success;
        }
        catch (StripeException ex)
        {
            return Error.Failure("Stripe.RefundError", ex.Message);
        }
    }

    public async Task<ErrorOr<Success>> VoidAsync(Payment payment, string idempotencyKey, CancellationToken ct)
    {
        if (payment.PaymentMethod == null) return Error.Validation("Payment.MethodMissing");

        var requestOptionsResult = await GetOptionsAsync(payment.PaymentMethod.GatewayConfigurationId, ct);
        if (requestOptionsResult.IsError) return requestOptionsResult.Errors;

        var requestOptions = requestOptionsResult.Value;
        requestOptions.IdempotencyKey = idempotencyKey;

        var service = new PaymentIntentService();
        try
        {
            await service.CancelAsync(payment.ReferenceTransactionId, null, requestOptions, ct);
            return Result.Success;
        }
        catch (StripeException ex)
        {
            return Error.Failure("Stripe.VoidError", ex.Message);
        }
    }

    public ErrorOr<Success> ValidateWebhook(string payload, string signature, string webhookSecret)
    {
        try
        {
            EventUtility.ConstructEvent(payload, signature, webhookSecret);
            return Result.Success;
        }
        catch (StripeException)
        {
            return Error.Unauthorized("Stripe.InvalidSignature");
        }
    }

    private static AuthorizationStatus MapStatus(string stripeStatus) => stripeStatus switch
    {
        "succeeded" => AuthorizationStatus.Authorized,
        "requires_action" => AuthorizationStatus.RequiresAction,
        "requires_payment_method" => AuthorizationStatus.RequiresAction,
        "processing" => AuthorizationStatus.Pending,
        "requires_confirmation" => AuthorizationStatus.Pending,
        "requires_capture" => AuthorizationStatus.Authorized,
        "canceled" => AuthorizationStatus.Failed,
        _ => AuthorizationStatus.Failed
    };

    private static string MapRefundReason(string reason) => reason.ToLowerInvariant() switch
    {
        "duplicate" => "duplicate",
        "fraudulent" => "fraudulent",
        "requested_by_customer" => "requested_by_customer",
        _ => "requested_by_customer"
    };
}