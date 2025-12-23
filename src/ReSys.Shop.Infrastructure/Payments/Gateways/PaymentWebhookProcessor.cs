using Stripe;

using ReSys.Shop.Core.Domain.Orders.Payments;
using ReSys.Shop.Core.Domain.Orders.Payments.Gateways;
using ReSys.Shop.Core.Common.Services.Security.Encryptors.Interfaces;
using ReSys.Shop.Core.Common.Domain.Concerns;

using DomainPaymentMethod = ReSys.Shop.Core.Domain.Settings.PaymentMethods.PaymentMethod;

namespace ReSys.Shop.Infrastructure.Payments.Gateways;

public sealed class PaymentWebhookProcessor(
    IApplicationDbContext dbContext,
    PaymentProcessorFactory factory,
    ICredentialEncryptor encryptor)
{
    public async Task<ErrorOr<Success>> ProcessWebhookAsync(DomainPaymentMethod.PaymentType type, string payload, string signature, CancellationToken ct = default)
    {
        var method = await dbContext.Set<DomainPaymentMethod>().FirstOrDefaultAsync(m => m.Type == type && m.Active, ct);
        if (method == null) return Error.NotFound("Payment.MethodInactive");

        var processorResult = factory.GetProcessor(type);
        if (processorResult.IsError) return processorResult.Errors;

        // Resolve webhook secret from private metadata
        var secretResult = GetWebhookSecret(method);
        if (secretResult.IsError) return secretResult.Errors;

        var validation = processorResult.Value.ValidateWebhook(payload, signature, secretResult.Value);
        if (validation.IsError) return validation.Errors;

        if (type == DomainPaymentMethod.PaymentType.Stripe)
        {
            return await ProcessStripeWebhookAsync(payload, ct);
        }

        return Result.Success;
    }

    private ErrorOr<string> GetWebhookSecret(DomainPaymentMethod method)
    {
        var encryptedSecret = method.GetPrivate("WebhookSecret")?.ToString();
        if (string.IsNullOrEmpty(encryptedSecret)) return string.Empty;

        try
        {
            return encryptor.Decrypt(encryptedSecret);
        }
        catch (Exception ex)
        {
            return Error.Failure("Payment.WebhookSecretDecryptionError", ex.Message);
        }
    }

    private async Task<ErrorOr<Success>> ProcessStripeWebhookAsync(string payload, CancellationToken ct)
    {
        var stripeEvent = EventUtility.ParseEvent(payload);

        if (stripeEvent.Type == EventTypes.PaymentIntentSucceeded)
        {
            var intent = (PaymentIntent)stripeEvent.Data.Object;
            await UpdatePaymentStatusAsync(intent, p => p.MarkAsCaptured(intent.Id), ct);
        }
        else if (stripeEvent.Type == EventTypes.PaymentIntentAmountCapturableUpdated)
        {
            var intent = (PaymentIntent)stripeEvent.Data.Object;
            await UpdatePaymentStatusAsync(intent, p => p.MarkAsAuthorized(intent.Id), ct);
        }
        else if (stripeEvent.Type == EventTypes.PaymentIntentPaymentFailed)
        {
            var intent = (PaymentIntent)stripeEvent.Data.Object;
            await UpdatePaymentStatusAsync(intent, p => p.MarkAsFailed(intent.LastPaymentError?.Message ?? "Payment failed", intent.LastPaymentError?.Code), ct);
        }
        else if (stripeEvent.Type == EventTypes.PaymentIntentCanceled)
        {
            var intent = (PaymentIntent)stripeEvent.Data.Object;
            await UpdatePaymentStatusAsync(intent, p => p.Void(), ct);
        }

        return Result.Success;
    }

    private async Task UpdatePaymentStatusAsync(PaymentIntent intent, Func<Payment, ErrorOr<object>> updateAction, CancellationToken ct)
    {
        var paymentIdStr = intent.Metadata.GetValueOrDefault("payment_id");
        if (Guid.TryParse(paymentIdStr, out var paymentId))
        {
            var payment = await dbContext.Set<Payment>()
                .FirstOrDefaultAsync(p => p.Id == paymentId, ct);

            if (payment != null)
            {
                var result = updateAction(payment);
                if (!result.IsError)
                {
                    await dbContext.SaveChangesAsync(ct);
                }
            }
        }
    }
}