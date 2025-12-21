using Carter;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ReSys.Shop.Core.Common.Models.Wrappers.Queryable;
using ReSys.Shop.Core.Common.Models.Wrappers.Responses;

namespace ReSys.Shop.Core.Feature.Storefront.Orders;

public static partial class OrderModule
{
    public sealed class Endpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup(prefix: "api/storefront/account/orders")
                .UseGroupMeta(meta: Annotations.Group)
                .RequireAuthorization();

            group.MapGet(pattern: string.Empty, handler: async (
                    [AsParameters] QueryableParams queryParams,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Get.PagedList.Query(Params: queryParams),
                        cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Orders retrieved successfully"));
                })
                .UseEndpointMeta(meta: Annotations.GetPagedList);

            group.MapGet(pattern: "{number}", handler: async (
                    [FromRoute] string number,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Get.ByNumber.Query(Number: number),
                        cancellationToken: ct);
                    return TypedResults.Ok(
                        value: result.ToApiResponse(message: "Order details retrieved successfully"));
                })
                .UseEndpointMeta(meta: Annotations.GetByNumber);

            app.MapGet(pattern: "api/storefront/orders/{token}", handler: async (
                    [FromRoute] string token,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Get.ByToken.Query(Token: token), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Order details retrieved successfully"));
                })
                .UseEndpointMeta(meta: Annotations.GetByToken);

            group.MapGet(pattern: "{number}/status", handler: async (
                    [FromRoute] string number,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Get.Status.Query(Number: number),
                        cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Order status retrieved successfully"));
                })
                .UseEndpointMeta(meta: Annotations.GetStatus);
        }
    }
}
