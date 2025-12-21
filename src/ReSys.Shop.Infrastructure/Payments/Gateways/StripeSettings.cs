namespace ReSys.Shop.Infrastructure.Payments.Gateways;

public record StripeSettings(
    string ApiKey,
    string WebhookSecret
);
