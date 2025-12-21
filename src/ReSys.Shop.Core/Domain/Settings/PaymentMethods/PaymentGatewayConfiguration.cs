using ReSys.Shop.Core.Common.Domain.Concerns;

namespace ReSys.Shop.Core.Domain.Settings.PaymentMethods;

/// <summary>
/// Aggregate representing the encrypted configuration for a payment gateway.
/// This isolates sensitive credentials (API keys, secrets) from the user-visible PaymentMethod.
/// </summary>
public sealed class PaymentGatewayConfiguration : Aggregate, IHasMetadata
{
    public string GatewayCode { get; private set; } = string.Empty;
    
    /// <summary>
    /// Encrypted JSON string containing the provider-specific settings.
    /// </summary>
    public string EncryptedSettings { get; private set; } = string.Empty;

    public IDictionary<string, object?>? PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?>? PrivateMetadata { get; set; } = new Dictionary<string, object?>();

    private PaymentGatewayConfiguration() { }

    public static PaymentGatewayConfiguration Create(string gatewayCode, string encryptedSettings)
    {
        return new PaymentGatewayConfiguration
        {
            Id = Guid.NewGuid(),
            GatewayCode = gatewayCode,
            EncryptedSettings = encryptedSettings,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void UpdateSettings(string encryptedSettings)
    {
        EncryptedSettings = encryptedSettings;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
