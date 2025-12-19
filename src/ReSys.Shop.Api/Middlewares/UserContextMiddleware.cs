using ReSys.Shop.Core.Common.Services.Persistence.Interfaces;
using ReSys.Shop.Core.Common.Services.Security.Authentication.Contexts;

namespace ReSys.Shop.Api.Middlewares;

public sealed class UserContextMiddleware(RequestDelegate next)
{
    private const string AdhocSessionCookieName = "ReSys.Adhoc.SessionId";

    public async Task InvokeAsync(
        HttpContext context,
        IUserContext userContext,
        IApplicationDbContext dbContext)
    {
        HandleAdhocUser(context: context, userContext: userContext);

        await next(context: context);
    }

    private static void HandleAdhocUser(HttpContext context, IUserContext userContext)
    {
        if (context.Request.Cookies.TryGetValue(key: AdhocSessionCookieName, value: out var adhocId) &&
            !string.IsNullOrWhiteSpace(value: adhocId))
        {
            userContext.SetAdhocCustomerId(adhocCustomerId: adhocId);
            return;
        }

        var newAdhocId = Guid.NewGuid().ToString(format: "N");
        userContext.SetAdhocCustomerId(adhocCustomerId: newAdhocId);

        context.Response.Cookies.Append(
            key: AdhocSessionCookieName,
            value: newAdhocId,
            options: new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(days: 60)
            });
    }
}
