namespace ReSys.Shop.Core.Common.Services.Security.Authentication.Tokens.Models;
public record TokenResult
{
    public string Token { get; init; } = string.Empty;
    public long ExpiresAt { get; init; }
}

