using ReSys.Shop.Core.Domain.Identity.Permissions;

namespace ReSys.Shop.Core.Common.Services.Security.Authorization.Permissions.Constants;

public static partial class FeaturePermission
{
    public static partial class Admin
    {
        public static class Order
        {
            public static AccessPermission View = AccessPermission.Create(name: "Admin.Order.View", "View Order Details", "Order Management").Value;
            public static AccessPermission List = AccessPermission.Create(name: "Admin.Order.List", "List Orders", "Order Management").Value;
            public static AccessPermission Create = AccessPermission.Create(name: "Admin.Order.Create", "Create Orders", "Order Management").Value;
            public static AccessPermission Update = AccessPermission.Create(name: "Admin.Order.Update", "Update Orders", "Order Management").Value;
            public static AccessPermission Delete = AccessPermission.Create(name: "Admin.Order.Delete", "Delete Orders", "Order Management").Value;
            public static AccessPermission Cancel = AccessPermission.Create(name: "Admin.Order.Cancel", "Cancel Orders", "Order Management").Value;
            public static AccessPermission Ship = AccessPermission.Create(name: "Admin.Order.Ship", "Ship Orders", "Order Management").Value;
            public static AccessPermission Payment = AccessPermission.Create(name: "Admin.Order.Payment", "Manage Order Payments", "Order Management").Value;

            public static AccessPermission[] All =>
            [
                View,
                List,
                Create,
                Update,
                Delete,
                Cancel,
                Ship,
                Payment
            ];
        }
    }
}
