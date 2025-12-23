using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ReSys.Shop.Core.Common.Models.Wrappers.Queryable;
using ReSys.Shop.Core.Common.Models.Wrappers.Responses;

namespace ReSys.Shop.Core.Feature.Storefront.Products;

public static partial class ProductModule
{
    public sealed class Endpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup(prefix: "api/storefront/products")
                .UseGroupMeta(meta: Annotations.Group);

            group.MapGet(pattern: string.Empty, handler: async (
                    [FromQuery] Guid? taxonId,
                    [AsParameters] QueryableParams queryParams,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Get.PagedList.Query(TaxonId: taxonId, Params: queryParams),
                        cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Products retrieved successfully"));
                })
                .UseEndpointMeta(meta: Annotations.GetPagedList);

            group.MapGet(pattern: "{slug}", handler: async (
                    [FromRoute] string slug,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Get.BySlug.Query(Slug: slug),
                        cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Product retrieved successfully"));
                })
                .UseEndpointMeta(meta: Annotations.GetBySlug);
        }
    }
}
