using Stripe;
using ReSys.Shop.Core.Domain.Orders.Payments;
using ReSys.Shop.Core.Domain.Orders.Payments.Gateways;
using DomainPaymentMethod = ReSys.Shop.Core.Domain.Settings.PaymentMethods.PaymentMethod;

namespace ReSys.Shop.Infrastructure.Payments.Gateways;

public sealed class PaymentWebhookProcessor(
    IApplicationDbContext dbContext,
    PaymentProcessorFactory factory,
    IGatewayCredentialProvider credentialProvider)
{
    public async Task<ErrorOr<Success>> ProcessWebhookAsync(DomainPaymentMethod.PaymentType type, string payload, string signature, CancellationToken ct = default)
    {
        var method = await dbContext.Set<DomainPaymentMethod>().FirstOrDefaultAsync(m => m.Type == type && m.Active, ct);
        if (method == null) return Error.NotFound("Payment.MethodInactive");

        var processorResult = factory.GetProcessor(type);
        if (processorResult.IsError) return processorResult.Errors;

        // Resolve webhook secret from encrypted config
        if (method.GatewayConfigurationId.HasValue)
        {
            var secretResult = await GetWebhookSecretAsync(type, method.GatewayConfigurationId.Value, ct);
            if (secretResult.IsError) return secretResult.Errors;

            var validation = processorResult.Value.ValidateWebhook(payload, signature, secretResult.Value);
            if (validation.IsError) return validation.Errors;
        }

        if (type == DomainPaymentMethod.PaymentType.Stripe)
        {
            return await ProcessStripeWebhookAsync(payload, ct);
        }

        return Result.Success;
    }

    private async Task<ErrorOr<string>> GetWebhookSecretAsync(DomainPaymentMethod.PaymentType type, Guid configId, CancellationToken ct)
    {
        if (type == DomainPaymentMethod.PaymentType.Stripe)
        {
            var settingsResult = await credentialProvider.GetSettingsAsync<StripeSettings>(configId, ct);
            if (settingsResult.IsError) return settingsResult.Errors;
            return settingsResult.Value.WebhookSecret;
        }
        
        return string.Empty;
    }

    private async Task<ErrorOr<Success>> ProcessStripeWebhookAsync(string payload, CancellationToken ct)
    {
        var stripeEvent = EventUtility.ParseEvent(payload);

        if (stripeEvent.Type == EventTypes.PaymentIntentSucceeded)
        {
            var intent = (PaymentIntent)stripeEvent.Data.Object;
            var paymentIdStr = intent.Metadata.GetValueOrDefault("payment_id");
            
            if (Guid.TryParse(paymentIdStr, out var paymentId))
            {
                var payment = await dbContext.Set<Payment>()
                    .FirstOrDefaultAsync(p => p.Id == paymentId, ct);

                if (payment != null)
                {
                    payment.MarkAsCaptured(intent.Id);
                    await dbContext.SaveChangesAsync(ct);
                }
            }
        }
        else if (stripeEvent.Type == EventTypes.PaymentIntentPaymentFailed)
        {
            var intent = (PaymentIntent)stripeEvent.Data.Object;
            var paymentIdStr = intent.Metadata.GetValueOrDefault("payment_id");

            if (Guid.TryParse(paymentIdStr, out var paymentId))
            {
                var payment = await dbContext.Set<Payment>()
                    .FirstOrDefaultAsync(p => p.Id == paymentId, ct);

                if (payment != null)
                {
                    payment.MarkAsFailed(intent.LastPaymentError?.Message ?? "Payment failed", intent.LastPaymentError?.Code);
                    await dbContext.SaveChangesAsync(ct);
                }
            }
        }

        return Result.Success;
    }
}