using System.Security.Claims;

namespace ReSys.Shop.Core.Common.Services.Security.Authentication.Contexts;

public static class ClaimsPrincipalExtensions
{
    public static string? GetUserId(this ClaimsPrincipal user)
    {
        return user.FindFirst(type: ClaimTypes.NameIdentifier)?.Value;
    }

    public static string? GetUserName(this ClaimsPrincipal user)
    {
        return user?.FindFirst(type: ClaimTypes.Name)?.Value;
    }

    public static bool IsAuthenticated(this ClaimsPrincipal user)
    {
        return user?.Identity?.IsAuthenticated ?? false;
    }
}
