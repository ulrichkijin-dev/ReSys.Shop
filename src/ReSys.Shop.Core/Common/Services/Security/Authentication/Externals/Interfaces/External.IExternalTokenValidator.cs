using ReSys.Shop.Core.Common.Services.Security.Authentication.Externals.Models;

namespace ReSys.Shop.Core.Common.Services.Security.Authentication.Externals.Interfaces;
public interface IExternalTokenValidator
{
    Task<ErrorOr<ExternalUserTransfer>> ValidateTokenAsync(
        string provider,
        string? accessToken,
        string? idToken,
        string? authorizationCode,
        string? redirectUri,
        CancellationToken cancellationToken = default
    );
}