using Microsoft.AspNetCore.Http;
using ReSys.Shop.Core.Common.Models.Wrappers.PagedLists;
using ReSys.Shop.Core.Common.Models.Wrappers.Responses;

namespace ReSys.Shop.Core.Feature.Admin.SystemAdmin;

public static partial class SystemModule
{
    private static class Annotations
    {
        public static ApiGroupMeta Group => new()
        {
            Name = "Admin.System",
            Tags = ["Admin System"],
            Summary = "System Administration",
            Description = "Endpoints for system administration, including audit logs and diagnostics."
        };

        public static ApiEndpointMeta GetAuditLogs => new()
        {
            Name = "Admin.System.GetAuditLogs",
            Summary = "Get audit logs",
            Description = "Retrieves a paginated list of audit logs capturing system changes.",
            ResponseType = typeof(ApiResponse<PaginationList<Models.AuditLogItem>>),
            StatusCode = StatusCodes.Status200OK
        };
    }
}
