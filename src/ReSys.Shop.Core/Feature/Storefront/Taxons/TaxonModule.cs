using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ReSys.Shop.Core.Common.Models.Wrappers.Queryable;
using ReSys.Shop.Core.Common.Models.Wrappers.Responses;

namespace ReSys.Shop.Core.Feature.Storefront.Taxons;

public static partial class TaxonModule
{
    public sealed class Endpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup(prefix: "api/storefront/taxons")
                .UseGroupMeta(meta: Annotations.Group);

            group.MapGet(pattern: string.Empty, handler: async (
                    [AsParameters] QueryableParams queryParams,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Get.PagedList.Query(Params: queryParams),
                        cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Taxons retrieved successfully"));
                })
                .UseEndpointMeta(meta: Annotations.GetPagedList);

            group.MapGet(pattern: "{id:guid}", handler: async (
                    [FromRoute] Guid id,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Get.ById.Query(Id: id),
                        cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Taxon retrieved successfully"));
                })
                .UseEndpointMeta(meta: Annotations.GetById);
        }
    }
}
