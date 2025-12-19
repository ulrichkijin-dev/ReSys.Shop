namespace ReSys.Shop.Core.Common.Services.Security.Authorization.Claims;

public interface IAuthorizeClaimDataProvider
{
    Task<AuthorizeClaimData?> GetUserAuthorizationAsync(string userId);
    Task InvalidateUserAuthorizationAsync(string userId);
}