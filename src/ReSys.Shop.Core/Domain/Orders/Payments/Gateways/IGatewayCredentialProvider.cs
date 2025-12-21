namespace ReSys.Shop.Core.Domain.Orders.Payments.Gateways;

public interface IGatewayCredentialProvider
{
    Task<ErrorOr<T>> GetSettingsAsync<T>(Guid configurationId, CancellationToken ct = default) where T : class;
}
