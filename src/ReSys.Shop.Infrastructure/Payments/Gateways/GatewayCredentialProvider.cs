using System.Text.Json;
using ReSys.Shop.Core.Common.Services.Security.Encryptors.Interfaces;
using ReSys.Shop.Core.Domain.Orders.Payments.Gateways;
using ReSys.Shop.Core.Domain.Settings.PaymentMethods;

namespace ReSys.Shop.Infrastructure.Payments.Gateways;

public sealed class GatewayCredentialProvider(
    IApplicationDbContext dbContext,
    ICredentialEncryptor encryptor) : IGatewayCredentialProvider
{
    public async Task<ErrorOr<T>> GetSettingsAsync<T>(Guid configurationId, CancellationToken ct = default) where T : class
    {
        var config = await dbContext.Set<PaymentGatewayConfiguration>()
            .FirstOrDefaultAsync(c => c.Id == configurationId, ct);

        if (config == null)
            return Error.NotFound("GatewayConfiguration.NotFound");

        try
        {
            var json = encryptor.Decrypt(config.EncryptedSettings);
            var settings = JsonSerializer.Deserialize<T>(json);
            
            return settings ?? throw new InvalidOperationException("Failed to deserialize settings.");
        }
        catch (Exception ex)
        {
            return Error.Failure("GatewayConfiguration.DecryptionError", ex.Message);
        }
    }
}
