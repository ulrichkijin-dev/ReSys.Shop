using ReSys.Shop.Core.Domain.Identity.Tokens;
using ReSys.Shop.Core.Domain.Identity.Users;

namespace ReSys.Shop.Core.Common.Services.Security.Authentication.Tokens.Models;

/// <summary>
/// Result of refresh token validation.
/// </summary>
public sealed record RefreshTokenValidationResult
{
    public RefreshToken RefreshToken { get; init; } = null!;
    public User User { get; init; } = null!;
}
