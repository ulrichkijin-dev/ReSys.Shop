using Carter;


using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;


using ReSys.Shop.Core.Common.Models.Pagination;
using ReSys.Shop.Core.Common.Models.Wrappers.Responses;

namespace ReSys.Shop.Core.Feature.Admin.SystemAdmin;

public static partial class SystemModule
{
    public sealed class Endpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup(prefix: "api/admin/system")
                .UseGroupMeta(meta: Annotations.Group)
                .RequireAuthorization();

            group.MapGet(pattern: "audit-logs", handler: async (
                    [AsParameters] PagingParams pagingParams,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Get.AuditLogsQuery(pagingParams), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Audit logs retrieved successfully"));
                })
                .UseEndpointMeta(meta: Annotations.GetAuditLogs);
        }
    }
}
