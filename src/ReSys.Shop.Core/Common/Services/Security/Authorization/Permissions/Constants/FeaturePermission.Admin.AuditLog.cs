using ReSys.Shop.Core.Domain.Identity.Permissions;

namespace ReSys.Shop.Core.Common.Services.Security.Authorization.Permissions.Constants;

public static partial class FeaturePermission
{
    public static partial class Admin
    {
        public static class AuditLog
        {
            public static AccessPermission View  => AccessPermission.Create(name: "admin.audit_log.view",
                displayName: "View AuditLog",
                description: "Allows viewing audit logs").Value;
            public static AccessPermission List  => AccessPermission.Create(name: "admin.audit_log.list",
                displayName: "List AuditLogs",
                description: "Allows listing audit logs").Value;
            public static AccessPermission Export  => AccessPermission.Create(name: "admin.audit_log.export",
                displayName: "Export AuditLogs",
                description: "Allows exporting audit logs").Value;

            public static AccessPermission[] All => [View, List, Export];
        }
    }
}
