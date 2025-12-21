using Carter;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using ReSys.Shop.Core.Common.Models.Wrappers.PagedLists;
using ReSys.Shop.Core.Common.Models.Wrappers.Responses;
using ReSys.Shop.Core.Common.Services.Security.Authorization.Attributes;
using ReSys.Shop.Core.Common.Services.Security.Authorization.Permissions.Constants;

namespace  ReSys.Shop.Core.Feature.Admin.Identity.Permissions;

public static partial class PermissionModule
{
    public sealed class Endpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup(prefix: "api/admin/account/permissions")
                .UseGroupMeta(meta: Annotations.Group)
                .RequireAuthorization();

            group.MapGet(pattern: "select", handler: GetSelectListHandler)
                .UseEndpointMeta(meta: Annotations.GetSelectList)
                .RequireAccessPermission(FeaturePermission.Admin.Identity.AccessControlPermission.List);

            group.MapGet(pattern: string.Empty, handler: GetPagedListHandler)
                .UseEndpointMeta(meta: Annotations.GetPagedList)
                .RequireAccessPermission(FeaturePermission.Admin.Identity.AccessControlPermission.List);

            group.MapGet(pattern: "{id:guid}", handler: GetByIdHandler)
                .UseEndpointMeta(meta: Annotations.GetById)
                .RequireAccessPermission(FeaturePermission.Admin.Identity.AccessControlPermission.View);

            group.MapGet(pattern: "{name}", handler: GetByNameHandler)
                .UseEndpointMeta(meta: Annotations.GetByName)
                .RequireAccessPermission(FeaturePermission.Admin.Identity.AccessControlPermission.View);
        }

        private static async Task<Ok<ApiResponse<Get.ByName.Result>>> GetByNameHandler([FromRoute] string name, [FromServices] ISender mediator, CancellationToken cancellationToken)
        {
            var query = new Get.ByName.Query(Name: name);
            var result = await mediator.Send(request: query, cancellationToken: cancellationToken);
            var apiResponse = result.ToApiResponse(message: "Access permission retrieved successfully");
            return TypedResults.Ok(value: apiResponse);
        }

        private static async Task<Ok<ApiResponse<Get.ById.Result>>> GetByIdHandler([FromRoute] Guid id, [FromServices] ISender mediator, CancellationToken cancellationToken)
        {
            var query = new Get.ById.Query(Id: id);
            var result = await mediator.Send(request: query, cancellationToken: cancellationToken);
            var apiResponse = result.ToApiResponse(message: "Access permission retrieved successfully");
            return TypedResults.Ok(value: apiResponse);
        }

        private static async Task<Ok<ApiResponse<PaginationList<Get.PagedList.Result>>>> GetPagedListHandler([AsParameters] Get.PagedList.Request request, [FromServices] ISender mediator, CancellationToken cancellationToken)
        {
            var query = new Get.PagedList.Query(Request: request);
            var result = await mediator.Send(request: query, cancellationToken: cancellationToken);
            var apiResponse = result.ToApiResponse(message: "Access permissions retrieved successfully");
            return TypedResults.Ok(value: apiResponse);
        }

        private static async Task<Ok<ApiResponse<PaginationList<Get.SelectList.Result>>>> GetSelectListHandler([AsParameters] Get.SelectList.Request request, [FromServices] ISender mediator, CancellationToken cancellationToken)
        {
            var query = new Get.SelectList.Query(Request: request);
            var result = await mediator.Send(request: query, cancellationToken: cancellationToken);
            var apiResponse = result.ToApiResponse(message: "Access permissions retrieved successfully");
            return TypedResults.Ok(value: apiResponse);
        }
    }
}
