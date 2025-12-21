using Carter;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ReSys.Shop.Core.Common.Models.Wrappers.Responses;

namespace ReSys.Shop.Core.Feature.Storefront.Cart;

public static partial class CartModule
{
    public sealed class Endpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup(prefix: "api/storefront/cart")
                .UseGroupMeta(meta: Annotations.Group);

            group.MapGet(pattern: string.Empty, handler: async (
                    [FromHeader(Name = "X-Cart-Token")] string? token,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Get.Query(Token: token), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Cart retrieved successfully"));
                })
                .UseEndpointMeta(meta: Annotations.Get);

            group.MapPost(pattern: string.Empty, handler: async (
                    [FromBody] Create.Request request,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Create.Command(Request: request), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponseCreated(message: "Cart created successfully"));
                })
                .UseEndpointMeta(meta: Annotations.Create);

            group.MapDelete(pattern: string.Empty, handler: async (
                    [FromHeader(Name = "X-Cart-Token")] string? token,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Delete.Command(Token: token), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponseDeleted(message: "Cart deleted successfully"));
                })
                .UseEndpointMeta(meta: Annotations.Delete);

            group.MapPost(pattern: "add_item", handler: async (
                    [FromHeader(Name = "X-Cart-Token")] string? token,
                    [FromBody] Items.Add.Request request,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Items.Add.Command(Token: token, Request: request), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Item added to cart"));
                })
                .UseEndpointMeta(meta: Annotations.AddItem);

            group.MapPatch(pattern: "set_quantity", handler: async (
                    [FromHeader(Name = "X-Cart-Token")] string? token,
                    [FromBody] Items.SetQuantity.Request request,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Items.SetQuantity.Command(Token: token, Request: request), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Item quantity updated"));
                })
                .UseEndpointMeta(meta: Annotations.SetQuantity);

            group.MapDelete(pattern: "remove_line_item/{id:guid}", handler: async (
                    [FromHeader(Name = "X-Cart-Token")] string? token,
                    [FromRoute] Guid id,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Items.Remove.Command(Token: token, LineItemId: id), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Item removed from cart"));
                })
                .UseEndpointMeta(meta: Annotations.RemoveItem);

            group.MapPatch(pattern: "empty", handler: async (
                    [FromHeader(Name = "X-Cart-Token")] string? token,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Actions.Empty.Command(Token: token), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Cart emptied"));
                })
                .UseEndpointMeta(meta: Annotations.Empty);

            group.MapPatch(pattern: "apply_coupon", handler: async (
                    [FromHeader(Name = "X-Cart-Token")] string? token,
                    [FromBody] Actions.ApplyCoupon.Request request,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Actions.ApplyCoupon.Command(Token: token, Request: request), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Coupon applied"));
                })
                .UseEndpointMeta(meta: Annotations.ApplyCoupon);

            group.MapPatch(pattern: "associate", handler: async (
                    [FromHeader(Name = "X-Cart-Token")] string? token,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Actions.Associate.Command(Token: token), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Cart associated with user"));
                })
                .UseEndpointMeta(meta: Annotations.Associate);

            // --- Checkout ---
            var checkout = app.MapGroup(prefix: "api/storefront/checkout")
                .UseGroupMeta(meta: Annotations.Group);

            checkout.MapGet(pattern: "summary", handler: async (
                    [FromHeader(Name = "X-Cart-Token")] string? token,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Checkout.GetSummary.Query(Token: token), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Checkout summary retrieved"));
                })
                .UseEndpointMeta(meta: Annotations.Checkout.Get);

            checkout.MapPatch(pattern: string.Empty, handler: async (
                    [FromHeader(Name = "X-Cart-Token")] string? token,
                    [FromBody] Checkout.Update.Request request,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Checkout.Update.Command(Token: token, Request: request), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Checkout updated"));
                })
                .UseEndpointMeta(meta: Annotations.Checkout.Update);

            checkout.MapPatch(pattern: "next", handler: async (
                    [FromHeader(Name = "X-Cart-Token")] string? token,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Checkout.Next.Command(Token: token), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Checkout moved to next step"));
                })
                .UseEndpointMeta(meta: Annotations.Checkout.Next);

            checkout.MapPatch(pattern: "advance", handler: async (
                    [FromHeader(Name = "X-Cart-Token")] string? token,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Checkout.Advance.Command(Token: token), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Checkout advanced"));
                })
                .UseEndpointMeta(meta: Annotations.Checkout.Advance);

            checkout.MapPatch(pattern: "complete", handler: async (
                    [FromHeader(Name = "X-Cart-Token")] string? token,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Checkout.Complete.Command(Token: token), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Order completed successfully"));
                })
                .UseEndpointMeta(meta: Annotations.Checkout.Complete);

            checkout.MapPatch(pattern: "select_shipping_method", handler: async (
                    [FromHeader(Name = "X-Cart-Token")] string? token,
                    [FromBody] Checkout.SelectShippingMethod.Request request,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Checkout.SelectShippingMethod.Command(Token: token, Request: request), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Shipping method selected"));
                })
                .UseEndpointMeta(meta: Annotations.Checkout.SelectShippingMethod);

            checkout.MapGet(pattern: "payment_methods", handler: async (
                    [FromHeader(Name = "X-Cart-Token")] string? token,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Checkout.ListPaymentMethods.Query(Token: token), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Payment methods retrieved"));
                })
                .UseEndpointMeta(meta: Annotations.Checkout.CreatePayment); // Reusing for now

            checkout.MapPost(pattern: "payments", handler: async (
                    [FromHeader(Name = "X-Cart-Token")] string? token,
                    [FromBody] Checkout.AddPayment.Request request,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Checkout.AddPayment.Command(Token: token, Request: request), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Payment added"));
                })
                .UseEndpointMeta(meta: Annotations.Checkout.CreatePayment);
        }
    }
}