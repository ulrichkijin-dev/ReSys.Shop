using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ReSys.Shop.Core.Common.Models.Wrappers.Responses;
using ReSys.Shop.Core.Common.Services.Security.Authorization.Attributes;
using ReSys.Shop.Core.Common.Services.Security.Authorization.Permissions.Constants;

namespace ReSys.Shop.Core.Feature.Admin.Orders;

public static partial class OrderModule
{
    public sealed class Endpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup(prefix: "api/admin/orders")
                .UseGroupMeta(meta: Annotations.Group)
                .RequireAuthorization();

            // --- Order CRUD & List ---
            group.MapGet(pattern: string.Empty, handler: async (
                    [AsParameters] Get.PagedList.Request request,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Get.PagedList.Query(Request: request), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Orders retrieved successfully"));
                })
                .UseEndpointMeta(meta: Annotations.GetPagedList)
                .RequireAccessPermission(permission: FeaturePermission.Admin.Order.List);

            group.MapPost(pattern: string.Empty, handler: async (
                    [FromBody] Create.Request request,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Create.Command(Request: request), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponseCreated(message: "Order created successfully"));
                })
                .UseEndpointMeta(meta: Annotations.Create)
                .RequireAccessPermission(permission: FeaturePermission.Admin.Order.Create);

            group.MapGet(pattern: "{id:guid}", handler: async (
                    [FromRoute] Guid id,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Get.ById.Query(Id: id), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Order details retrieved successfully"));
                })
                .UseEndpointMeta(meta: Annotations.GetById)
                .RequireAccessPermission(permission: FeaturePermission.Admin.Order.View);

            group.MapDelete(pattern: "{id:guid}", handler: async (
                    [FromRoute] Guid id,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Delete.Command(Id: id), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponseDeleted(message: "Order deleted successfully"));
                })
                .UseEndpointMeta(meta: Annotations.Delete)
                .RequireAccessPermission(permission: FeaturePermission.Admin.Order.Delete);

            group.MapPatch(pattern: "{id:guid}", handler: async (
                    [FromRoute] Guid id,
                    [FromBody] Update.Request request,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Update.Command(Id: id, Request: request), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Order updated successfully"));
                })
                .UseEndpointMeta(meta: Annotations.Update)
                .RequireAccessPermission(permission: FeaturePermission.Admin.Order.Update);

            // --- Order Actions ---
            group.MapPatch(pattern: "{id:guid}/advance", handler: async (
                    [FromRoute] Guid id,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Actions.Advance.Command(Id: id), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Order advanced successfully"));
                })
                .UseEndpointMeta(meta: Annotations.Advance)
                .RequireAccessPermission(permission: FeaturePermission.Admin.Order.Update);

            group.MapPatch(pattern: "{id:guid}/next", handler: async (
                    [FromRoute] Guid id,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Actions.Next.Command(Id: id), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Order state transitioned successfully"));
                })
                .UseEndpointMeta(meta: Annotations.Next)
                .RequireAccessPermission(permission: FeaturePermission.Admin.Order.Update);

            group.MapPatch(pattern: "{id:guid}/complete", handler: async (
                    [FromRoute] Guid id,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Actions.Complete.Command(Id: id), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Order completed successfully"));
                })
                .UseEndpointMeta(meta: Annotations.Complete)
                .RequireAccessPermission(permission: FeaturePermission.Admin.Order.Update);

            group.MapPatch(pattern: "{id:guid}/empty", handler: async (
                    [FromRoute] Guid id,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Actions.Empty.Command(Id: id), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Order emptied successfully"));
                })
                .UseEndpointMeta(meta: Annotations.Empty)
                .RequireAccessPermission(permission: FeaturePermission.Admin.Order.Update);

            group.MapPatch(pattern: "{id:guid}/approve", handler: async (
                    [FromRoute] Guid id,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Actions.Approve.Command(Id: id), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Order approved successfully"));
                })
                .UseEndpointMeta(meta: Annotations.Approve)
                .RequireAccessPermission(permission: FeaturePermission.Admin.Order.Update);

            group.MapPatch(pattern: "{id:guid}/cancel", handler: async (
                    [FromRoute] Guid id,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Actions.Cancel.Command(Id: id), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Order canceled successfully"));
                })
                .UseEndpointMeta(meta: Annotations.Cancel)
                .RequireAccessPermission(permission: FeaturePermission.Admin.Order.Cancel);

            group.MapPatch(pattern: "{id:guid}/apply_coupon", handler: async (
                    [FromRoute] Guid id,
                    [FromBody] Actions.ApplyCoupon.Request request,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Actions.ApplyCoupon.Command(Id: id, Request: request), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Coupon code applied successfully"));
                })
                .UseEndpointMeta(meta: Annotations.ApplyCoupon)
                .RequireAccessPermission(permission: FeaturePermission.Admin.Order.Update);

            group.MapDelete(pattern: "{id:guid}/apply_coupon", handler: async (
                    [FromRoute] Guid id,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Actions.RemoveCoupon.Command(Id: id), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Coupon removed successfully"));
                })
                .UseEndpointMeta(meta: Annotations.RemoveCoupon)
                .RequireAccessPermission(permission: FeaturePermission.Admin.Order.Update);

            group.MapGet(pattern: "{id:guid}/coupons", handler: async (
                    [FromRoute] Guid id,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Actions.GetCoupons.Query(Id: id), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Applied coupons retrieved successfully"));
                })
                .UseEndpointMeta(meta: Annotations.GetCoupons)
                .RequireAccessPermission(permission: FeaturePermission.Admin.Order.View);


            // --- Shipment Management ---
            group.MapGet(pattern: "{id:guid}/shipments", handler: async (
                    [FromRoute] Guid id,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Shipments.GetList.Query(OrderId: id), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Order shipments retrieved successfully"));
                })
                .UseEndpointMeta(meta: Annotations.Shipments.GetList)
                .RequireAccessPermission(permission: FeaturePermission.Admin.Order.View);

            group.MapPost(pattern: "{id:guid}/shipments", handler: async (
                    [FromRoute] Guid id,
                    [FromBody] Shipments.Create.Request request,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Shipments.Create.Command(OrderId: id, Request: request), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponseCreated(message: "Shipment created successfully"));
                })
                .UseEndpointMeta(meta: Annotations.Shipments.Create)
                .RequireAccessPermission(permission: FeaturePermission.Admin.Order.Ship);

            group.MapPost(pattern: "{id:guid}/shipments/auto_plan", handler: async (
                    [FromRoute] Guid id,
                    [FromBody] Shipments.AutoPlan.Request request,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Shipments.AutoPlan.Command(OrderId: id, Request: request), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Shipments planned and created successfully"));
                })
                .UseEndpointMeta(meta: Annotations.Shipments.AutoPlan)
                .RequireAccessPermission(permission: FeaturePermission.Admin.Order.Ship);

            group.MapGet(pattern: "{id:guid}/shipments/{shipmentId:guid}", handler: async (
                    [FromRoute] Guid id,
                    [FromRoute] Guid shipmentId,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Shipments.GetById.Query(OrderId: id, ShipmentId: shipmentId), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Shipment details retrieved successfully"));
                })
                .UseEndpointMeta(meta: Annotations.Shipments.GetById)
                .RequireAccessPermission(permission: FeaturePermission.Admin.Order.View);

            group.MapDelete(pattern: "{id:guid}/shipments/{shipmentId:guid}", handler: async (
                    [FromRoute] Guid id,
                    [FromRoute] Guid shipmentId,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Shipments.Delete.Command(OrderId: id, ShipmentId: shipmentId), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponseDeleted(message: "Shipment deleted successfully"));
                })
                .UseEndpointMeta(meta: Annotations.Shipments.Delete)
                .RequireAccessPermission(permission: FeaturePermission.Admin.Order.Ship);

            group.MapPatch(pattern: "{id:guid}/shipments/{shipmentId:guid}", handler: async (
                    [FromRoute] Guid id,
                    [FromRoute] Guid shipmentId,
                    [FromBody] Shipments.Update.Request request,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Shipments.Update.Command(OrderId: id, ShipmentId: shipmentId, Request: request), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Shipment updated successfully"));
                })
                .UseEndpointMeta(meta: Annotations.Shipments.Update)
                .RequireAccessPermission(permission: FeaturePermission.Admin.Order.Ship);

            // Shipment Actions
            group.MapPatch(pattern: "{id:guid}/shipments/{shipmentId:guid}/add_item", handler: async (
                    [FromRoute] Guid id,
                    [FromRoute] Guid shipmentId,
                    [FromBody] Shipments.AddItem.Request request,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Shipments.AddItem.Command(OrderId: id, ShipmentId: shipmentId, Request: request), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Item added to shipment"));
                })
                .UseEndpointMeta(meta: Annotations.Shipments.AddItem)
                .RequireAccessPermission(permission: FeaturePermission.Admin.Order.Ship);

            group.MapPatch(pattern: "{id:guid}/shipments/{shipmentId:guid}/remove_item", handler: async (
                    [FromRoute] Guid id,
                    [FromRoute] Guid shipmentId,
                    [FromBody] Shipments.RemoveItem.Request request,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Shipments.RemoveItem.Command(OrderId: id, ShipmentId: shipmentId, Request: request), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Item removed from shipment"));
                })
                .UseEndpointMeta(meta: Annotations.Shipments.RemoveItem)
                .RequireAccessPermission(permission: FeaturePermission.Admin.Order.Ship);

            group.MapPatch(pattern: "{id:guid}/shipments/{shipmentId:guid}/ready", handler: async (
                    [FromRoute] Guid id,
                    [FromRoute] Guid shipmentId,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Shipments.Ready.Command(OrderId: id, ShipmentId: shipmentId), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Shipment marked as ready"));
                })
                .UseEndpointMeta(meta: Annotations.Shipments.Ready)
                .RequireAccessPermission(permission: FeaturePermission.Admin.Order.Ship);

            group.MapPatch(pattern: "{id:guid}/shipments/{shipmentId:guid}/ship", handler: async (
                    [FromRoute] Guid id,
                    [FromRoute] Guid shipmentId,
                    [FromBody] Shipments.Ship.Request request,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Shipments.Ship.Command(OrderId: id, ShipmentId: shipmentId, Request: request), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Shipment marked as shipped"));
                })
                .UseEndpointMeta(meta: Annotations.Shipments.Ship)
                .RequireAccessPermission(permission: FeaturePermission.Admin.Order.Ship);

            group.MapPatch(pattern: "{id:guid}/shipments/{shipmentId:guid}/cancel", handler: async (
                    [FromRoute] Guid id,
                    [FromRoute] Guid shipmentId,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Shipments.Cancel.Command(OrderId: id, ShipmentId: shipmentId), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Shipment canceled"));
                })
                .UseEndpointMeta(meta: Annotations.Shipments.CancelAction)
                .RequireAccessPermission(permission: FeaturePermission.Admin.Order.Ship);

            group.MapPatch(pattern: "{id:guid}/shipments/{shipmentId:guid}/resume", handler: async (
                    [FromRoute] Guid id,
                    [FromRoute] Guid shipmentId,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Shipments.Resume.Command(OrderId: id, ShipmentId: shipmentId), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Shipment resumed"));
                })
                .UseEndpointMeta(meta: Annotations.Shipments.Resume)
                .RequireAccessPermission(permission: FeaturePermission.Admin.Order.Ship);

            group.MapPatch(pattern: "{id:guid}/shipments/{shipmentId:guid}/pending", handler: async (
                    [FromRoute] Guid id,
                    [FromRoute] Guid shipmentId,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Shipments.ToPending.Command(OrderId: id, ShipmentId: shipmentId), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Shipment moved to pending"));
                })
                .UseEndpointMeta(meta: Annotations.Shipments.ToPending)
                .RequireAccessPermission(permission: FeaturePermission.Admin.Order.Ship);

            group.MapPatch(pattern: "{id:guid}/shipments/{shipmentId:guid}/deliver", handler: async (
                    [FromRoute] Guid id,
                    [FromRoute] Guid shipmentId,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Shipments.Deliver.Command(OrderId: id, ShipmentId: shipmentId), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Shipment marked as delivered"));
                })
                .UseEndpointMeta(meta: Annotations.Shipments.Deliver)
                .RequireAccessPermission(permission: FeaturePermission.Admin.Order.Ship);

            group.MapPatch(pattern: "{id:guid}/shipments/{shipmentId:guid}/transfer_to_shipment", handler: async (
                    [FromRoute] Guid id,
                    [FromRoute] Guid shipmentId,
                    [FromBody] Shipments.TransferToShipment.Request request,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Shipments.TransferToShipment.Command(OrderId: id, SourceShipmentId: shipmentId, Request: request), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Items transferred to target shipment"));
                })
                .UseEndpointMeta(meta: Annotations.Shipments.TransferToShipment)
                .RequireAccessPermission(permission: FeaturePermission.Admin.Order.Ship);

            group.MapPatch(pattern: "{id:guid}/shipments/{shipmentId:guid}/transfer_to_location", handler: async (
                    [FromRoute] Guid id,
                    [FromRoute] Guid shipmentId,
                    [FromBody] Shipments.TransferToLocation.Request request,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Shipments.TransferToLocation.Command(OrderId: id, SourceShipmentId: shipmentId, Request: request), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Items transferred to target location"));
                })
                .UseEndpointMeta(meta: Annotations.Shipments.TransferToLocation)
                .RequireAccessPermission(permission: FeaturePermission.Admin.Order.Ship);


            // --- Payment Management ---
            group.MapGet(pattern: "{id:guid}/payments", handler: async (
                    [FromRoute] Guid id,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Payments.GetList.Query(OrderId: id), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Order payments retrieved successfully"));
                })
                .UseEndpointMeta(meta: Annotations.Payments.GetList)
                .RequireAccessPermission(permission: FeaturePermission.Admin.Order.View);

            group.MapPost(pattern: "{id:guid}/payments", handler: async (
                    [FromRoute] Guid id,
                    [FromBody] Payments.Create.Request request,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Payments.Create.Command(OrderId: id, Request: request), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponseCreated(message: "Payment added successfully"));
                })
                .UseEndpointMeta(meta: Annotations.Payments.Create)
                .RequireAccessPermission(permission: FeaturePermission.Admin.Order.Payment);

            group.MapPost(pattern: "{id:guid}/payments/{paymentId:guid}/authorize", handler: async (
                    [FromRoute] Guid id,
                    [FromRoute] Guid paymentId,
                    [FromBody] Payments.Authorize.Request request,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Payments.Authorize.Command(OrderId: id, PaymentId: paymentId, Request: request), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Payment authorized successfully"));
                })
                .UseEndpointMeta(meta: Annotations.Payments.Authorize)
                .RequireAccessPermission(permission: FeaturePermission.Admin.Order.Payment);

            group.MapPost(pattern: "{id:guid}/payments/{paymentId:guid}/capture", handler: async (
                    [FromRoute] Guid id,
                    [FromRoute] Guid paymentId,
                    [FromBody] Payments.Capture.Request request,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Payments.Capture.Command(OrderId: id, PaymentId: paymentId, Request: request), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Payment captured successfully"));
                })
                .UseEndpointMeta(meta: Annotations.Payments.Capture)
                .RequireAccessPermission(permission: FeaturePermission.Admin.Order.Payment);

            group.MapPost(pattern: "{id:guid}/payments/{paymentId:guid}/refund", handler: async (
                    [FromRoute] Guid id,
                    [FromRoute] Guid paymentId,
                    [FromBody] Payments.Refund.Request request,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Payments.Refund.Command(OrderId: id, PaymentId: paymentId, Request: request), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Payment refunded successfully"));
                })
                .UseEndpointMeta(meta: Annotations.Payments.Refund)
                .RequireAccessPermission(permission: FeaturePermission.Admin.Order.Payment);

            group.MapPost(pattern: "{id:guid}/payments/{paymentId:guid}/void", handler: async (
                    [FromRoute] Guid id,
                    [FromRoute] Guid paymentId,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Payments.Void.Command(OrderId: id, PaymentId: paymentId), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Payment voided successfully"));
                })
                .UseEndpointMeta(meta: Annotations.Payments.Void)
                .RequireAccessPermission(permission: FeaturePermission.Admin.Order.Payment);
        }
    }
}
