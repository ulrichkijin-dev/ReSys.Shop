using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ReSys.Shop.Core.Common.Models.Wrappers.Queryable;
using ReSys.Shop.Core.Common.Models.Wrappers.Responses;

namespace ReSys.Shop.Core.Feature.Storefront.Reviews;

public static partial class ReviewModule
{
    public sealed class Endpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var productReviews = app.MapGroup(prefix: "api/storefront/products/{productId:guid}/reviews")
                .UseGroupMeta(meta: Annotations.Group);

            productReviews.MapGet(pattern: string.Empty, handler: async (
                    [FromRoute] Guid productId,
                    [AsParameters] QueryableParams queryParams,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Get.PagedListQuery(ProductId: productId, Params: queryParams),
                        cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Reviews retrieved successfully"));
                })
                .UseEndpointMeta(meta: Annotations.GetPagedList);

            productReviews.MapPost(pattern: string.Empty, handler: async (
                    [FromRoute] Guid productId,
                    [FromBody] Actions.Create.Request request,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Actions.Create.Command(ProductId: productId, Request: request), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponseCreated(message: "Review submitted successfully"));
                })
                .UseEndpointMeta(meta: Annotations.Create)
                .RequireAuthorization();

            var votes = app.MapGroup(prefix: "api/storefront/reviews")
                .UseGroupMeta(meta: Annotations.Group);

            votes.MapPost(pattern: "{id:guid}/vote", handler: async (
                    [FromRoute] Guid id,
                    [FromBody] Actions.Vote.Request request,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Actions.Vote.Command(ReviewId: id, Request: request), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Vote recorded successfully"));
                })
                .UseEndpointMeta(meta: Annotations.Vote);
        }
    }
}
