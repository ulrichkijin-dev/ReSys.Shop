using ReSys.Shop.Core.Domain.Identity.Permissions;

namespace ReSys.Shop.Core.Common.Services.Security.Authorization.Permissions.Constants;

public static partial class FeaturePermission
{
    public static partial class Admin
    {
        public static partial class Inventory
        {
            public static AccessPermission[] All =>
            [
                ..StockItem.All,
                ..StockLocation.All,
                ..StockTransfer.All
            ];
        }
    }
}
