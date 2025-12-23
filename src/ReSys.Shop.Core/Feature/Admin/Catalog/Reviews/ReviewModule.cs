using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ReSys.Shop.Core.Common.Models.Wrappers.Queryable;
using ReSys.Shop.Core.Common.Models.Wrappers.Responses;

namespace ReSys.Shop.Core.Feature.Admin.Catalog.Reviews;

public static partial class ReviewModule
{
    public sealed class Endpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup(prefix: "api/admin/catalog/reviews")
                .UseGroupMeta(meta: Annotations.Group)
                .RequireAuthorization(); // Should ideally require specific Admin role/permission

            group.MapGet(pattern: string.Empty, handler: async (
                    [AsParameters] QueryableParams queryParams,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Get.PagedListQuery(Params: queryParams),
                        cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Reviews retrieved successfully"));
                })
                .UseEndpointMeta(meta: Annotations.GetPagedList);

            group.MapGet(pattern: "{id:guid}", handler: async (
                    [FromRoute] Guid id,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Get.ByIdQuery(Id: id),
                        cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Review details retrieved successfully"));
                })
                .UseEndpointMeta(meta: Annotations.GetById);

            group.MapPatch(pattern: "{id:guid}/approve", handler: async (
                    [FromRoute] Guid id,
                    [FromBody] Actions.Approve.Request request,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Actions.Approve.Command(Id: id, Request: request), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Review approved successfully"));
                })
                .UseEndpointMeta(meta: Annotations.Approve);

            group.MapPatch(pattern: "{id:guid}/reject", handler: async (
                    [FromRoute] Guid id,
                    [FromBody] Actions.Reject.Request request,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Actions.Reject.Command(Id: id, Request: request), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Review rejected successfully"));
                })
                .UseEndpointMeta(meta: Annotations.Reject);

            group.MapDelete(pattern: "{id:guid}", handler: async (
                    [FromRoute] Guid id,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Actions.Delete.Command(Id: id), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponseDeleted(message: "Review deleted successfully"));
                })
                .UseEndpointMeta(meta: Annotations.Delete);
        }
    }
}
