using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ReSys.Shop.Core.Common.Models.Wrappers.Queryable;
using ReSys.Shop.Core.Common.Models.Wrappers.Responses;

namespace ReSys.Shop.Core.Feature.Storefront.Countries;

public static partial class CountryModule
{
    public sealed class Endpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup(prefix: "api/storefront/countries")
                .UseGroupMeta(meta: Annotations.Group);

            group.MapGet(pattern: string.Empty, handler: async (
                    [AsParameters] QueryableParams queryParams,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Get.PagedList.Query(Params: queryParams),
                        cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Countries retrieved successfully"));
                })
                .UseEndpointMeta(meta: Annotations.GetPagedList);

            group.MapGet(pattern: "default", handler: async (
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Get.Default.Query(), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Default country retrieved successfully"));
                })
                .UseEndpointMeta(meta: Annotations.GetDefault);

            group.MapGet(pattern: "{id:guid}", handler: async (
                    [FromRoute] Guid id,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Get.ById.Query(Id: id),
                        cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Country retrieved successfully"));
                })
                .UseEndpointMeta(meta: Annotations.GetById);

            var states = app.MapGroup(prefix: "api/storefront/states")
                .UseGroupMeta(meta: Annotations.Group);

            states.MapGet(pattern: string.Empty, handler: async (
                    [FromQuery] Guid? countryId,
                    [AsParameters] QueryableParams queryParams,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new States.PagedList.Query(CountryId: countryId, Params: queryParams),
                        cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "States retrieved successfully"));
                })
                .UseEndpointMeta(meta: Annotations.States.GetPagedList);

            states.MapGet(pattern: "{id:guid}", handler: async (
                    [FromRoute] Guid id,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new States.ById.Query(Id: id),
                        cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "State retrieved successfully"));
                })
                .UseEndpointMeta(meta: Annotations.States.GetById);
        }
    }
}
