using ReSys.Shop.Core.Domain.Identity.Permissions;

namespace ReSys.Shop.Core.Common.Services.Security.Authorization.Permissions.Constants;

public static partial class FeaturePermission
{
    public static AccessPermission[] AllPermissions =>
    [
        .. Testing.All,
        .. Admin.All,
        .. Storefront.All
    ];
}
