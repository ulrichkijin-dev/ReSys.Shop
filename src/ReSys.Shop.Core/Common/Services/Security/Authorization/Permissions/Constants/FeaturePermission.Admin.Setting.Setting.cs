using ReSys.Shop.Core.Domain.Identity.Permissions;

namespace ReSys.Shop.Core.Common.Services.Security.Authorization.Permissions.Constants;

public static partial class FeaturePermission
{
    public static partial class Admin
    {
        public static partial class Setting
        {
            public static class SettingPermissions
            {
                public static AccessPermission Create => AccessPermission.Create(
                    name: "admin.setting.setting.create",
                    displayName: "Create Setting",
                    description: "Allows creation of application settings").Value;

                public static AccessPermission Update => AccessPermission.Create(
                    name: "admin.setting.setting.update",
                    displayName: "Update Setting",
                    description: "Allows updating of application settings").Value;

                public static AccessPermission Delete => AccessPermission.Create(
                    name: "admin.setting.setting.delete",
                    displayName: "Delete Setting",
                    description: "Allows deletion of application settings").Value;

                public static AccessPermission View => AccessPermission.Create(
                    name: "admin.setting.setting.delete",
                    displayName: "View Setting",
                    description: "Allows viewing of a single application setting").Value;

                public static AccessPermission List => AccessPermission.Create(
                    name: "admin.setting.setting.list",
                    displayName: "List Settings",
                    description: "Allows listing of application settings").Value;

                public static AccessPermission[] All =>
                [
                    Create, Update, Delete, View, List
                ];
            }
        }
    }
}