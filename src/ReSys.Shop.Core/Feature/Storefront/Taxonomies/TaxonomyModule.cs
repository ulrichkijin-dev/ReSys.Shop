using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ReSys.Shop.Core.Common.Models.Wrappers.Queryable;
using ReSys.Shop.Core.Common.Models.Wrappers.Responses;

namespace ReSys.Shop.Core.Feature.Storefront.Taxonomies;

public static partial class TaxonomyModule
{
    public sealed class Endpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup(prefix: "api/storefront/taxonomies")
                .UseGroupMeta(meta: Annotations.Group);

            group.MapGet(pattern: string.Empty, handler: async (
                    [AsParameters] QueryableParams queryParams,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Get.PagedList.Query(Params: queryParams),
                        cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Taxonomies retrieved successfully"));
                })
                .UseEndpointMeta(meta: Annotations.GetPagedList);

            group.MapGet(pattern: "{id:guid}", handler: async (
                    [FromRoute] Guid id,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Get.ById.Query(Id: id),
                        cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Taxonomy retrieved successfully"));
                })
                .UseEndpointMeta(meta: Annotations.GetById);
        }
    }
}
