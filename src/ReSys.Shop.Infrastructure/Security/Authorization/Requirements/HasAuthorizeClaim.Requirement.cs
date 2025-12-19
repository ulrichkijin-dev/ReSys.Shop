using Microsoft.AspNetCore.Authorization;

namespace ReSys.Shop.Infrastructure.Security.Authorization.Requirements;

public sealed class HasAuthorizeClaimRequirement(
    string[]? permissions = null,
    string[]? policies = null,
    string[]? roles = null) : IAuthorizationRequirement
{
    public string[] Permissions { get; } = permissions ?? [];
    public string[] Policies { get; } = policies ?? [];
    public string[] Roles { get; } = roles ?? [];
}
