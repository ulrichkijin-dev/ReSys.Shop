using Microsoft.AspNetCore.Http;

using ReSys.Shop.Core.Common.Services.Security.Authentication.Contexts;

namespace ReSys.Shop.Infrastructure.Security.Authentication.Contexts;

public sealed class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public string? UserId =>
        httpContextAccessor.HttpContext?.User.GetUserId();

    public string? UserName =>
        httpContextAccessor.HttpContext?.User.GetUserName();

    public bool IsAuthenticated =>
        httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public string? AdhocCustomerId { get; private set; }

    public Guid? StoreId { get; private set; }

    public void SetAdhocCustomerId(string adhocId)
    {
        AdhocCustomerId = adhocId;
    }

    public void SetStoreId(Guid storeId)
    {
        StoreId = storeId;
    }
}
