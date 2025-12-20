using ReSys.Shop.Core.Domain.Identity.Permissions;

namespace ReSys.Shop.Core.Common.Services.Security.Authorization.Permissions.Constants;

public static partial class FeaturePermission
{
    public static partial class Admin
    {
        public static partial class Setting
        {
            public static AccessPermission[] All =>
            [
                ..PaymentMethod.All,
                ..ShippingMethod.All,
                ..SettingPermissions.All,
            ];
        }
    }
}