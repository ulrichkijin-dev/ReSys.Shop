using Microsoft.AspNetCore.Http;
using ReSys.Shop.Core.Common.Models.Wrappers.PagedLists;
using ReSys.Shop.Core.Common.Models.Wrappers.Responses;

namespace ReSys.Shop.Core.Feature.Admin.Orders;

public static partial class OrderModule
{
    private static class Annotations
    {
        public static ApiGroupMeta Group => new()
        {
            Name = "Admin.Order",
            Tags = ["Order Management"],
            Summary = "Order Management API",
            Description = "Endpoints for managing customer orders, shipments, and payments"
        };

        public static ApiEndpointMeta GetPagedList => new()
        {
            Name = "Admin.Order.Get.Paged",
            Summary = "Get paged list of orders",
            Description = "Retrieves a paginated list of orders with filtering and sorting.",
            ResponseType = typeof(ApiResponse<PaginationList<Get.PagedList.Result>>),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta Create => new()
        {
            Name = "Admin.Order.Create",
            Summary = "Create an order",
            Description = "Manually creates a new order.",
            ResponseType = typeof(ApiResponse<Create.Result>),
            StatusCode = StatusCodes.Status201Created
        };

        public static ApiEndpointMeta GetById => new()
        {
            Name = "Admin.Order.GetById",
            Summary = "Get order details",
            Description = "Retrieves detailed information about a specific order by ID.",
            ResponseType = typeof(ApiResponse<Get.ById.Result>),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta Delete => new()
        {
            Name = "Admin.Order.Delete",
            Summary = "Delete an order",
            Description = "Deletes an order by ID (if in Cart or Canceled state).",
            ResponseType = typeof(ApiResponse),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta Update => new()
        {
            Name = "Admin.Order.Update",
            Summary = "Update an order",
            Description = "Updates order details like email or special instructions.",
            ResponseType = typeof(ApiResponse<Update.Result>),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta Advance => new()
        {
            Name = "Admin.Order.Advance",
            Summary = "Advance order state",
            Description = "Progresses the order to the next state.",
            ResponseType = typeof(ApiResponse),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta Next => new()
        {
            Name = "Admin.Order.Next",
            Summary = "Move order to next state",
            Description = "Equivalent to advance.",
            ResponseType = typeof(ApiResponse),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta Complete => new()
        {
            Name = "Admin.Order.Complete",
            Summary = "Complete an order",
            Description = "Finalizes the order by transitioning through remaining states to Complete.",
            ResponseType = typeof(ApiResponse),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta Empty => new()
        {
            Name = "Admin.Order.Empty",
            Summary = "Empty the order cart",
            Description = "Removes all line items from the order.",
            ResponseType = typeof(ApiResponse),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta Approve => new()
        {
            Name = "Admin.Order.Approve",
            Summary = "Approve an order",
            Description = "Marks an order as approved.",
            ResponseType = typeof(ApiResponse),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta Cancel => new()
        {
            Name = "Admin.Order.Cancel",
            Summary = "Cancel an order",
            Description = "Cancels an order and releases reserved inventory.",
            ResponseType = typeof(ApiResponse),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta ApplyCoupon => new()
        {
            Name = "Admin.Order.ApplyCoupon",
            Summary = "Apply coupon code",
            Description = "Applies a promotion to the order using a coupon code.",
            ResponseType = typeof(ApiResponse),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta RemoveCoupon => new()
        {
            Name = "Admin.Order.RemoveCoupon",
            Summary = "Remove applied coupon",
            Description = "Removes the applied promotion from the order.",
            ResponseType = typeof(ApiResponse),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta GetCoupons => new()
        {
            Name = "Admin.Order.GetCoupons",
            Summary = "Get applied coupons",
            Description = "Retrieves information about the applied promotion/coupons for the order.",
            ResponseType = typeof(ApiResponse<List<Actions.GetCoupons.Result>>),
            StatusCode = StatusCodes.Status200OK
        };

        public static class Shipments
        {
            public static ApiEndpointMeta GetList => new()
            {
                Name = "Admin.Order.Shipments.Get",
                Summary = "Get order shipments",
                Description = "Retrieves all shipments associated with an order.",
                ResponseType = typeof(ApiResponse<List<OrderModule.Shipments.GetList.Result>>),
                StatusCode = StatusCodes.Status200OK
            };

            public static ApiEndpointMeta GetById => new()
            {
                Name = "Admin.Order.Shipments.GetById",
                Summary = "Get shipment details",
                Description = "Retrieves details of a specific shipment.",
                ResponseType = typeof(ApiResponse<OrderModule.Shipments.GetById.Result>),
                StatusCode = StatusCodes.Status200OK
            };

            public static ApiEndpointMeta Create => new()
            {
                Name = "Admin.Order.Shipments.Create",
                Summary = "Create shipment",
                Description = "Creates a new shipment for the order.",
                ResponseType = typeof(ApiResponse<OrderModule.Shipments.Create.Result>),
                StatusCode = StatusCodes.Status201Created
            };

            public static ApiEndpointMeta AutoPlan => new()
            {
                Name = "Admin.Order.Shipments.AutoPlan",
                Summary = "Auto-plan shipments",
                Description = "Automatically determines and creates shipments using a fulfillment strategy.",
                ResponseType = typeof(ApiResponse<List<OrderModule.Shipments.AutoPlan.Result>>),
                StatusCode = StatusCodes.Status200OK
            };

            public static ApiEndpointMeta Update => new()
            {
                Name = "Admin.Order.Shipments.Update",
                Summary = "Update shipment",
                Description = "Updates shipment details like tracking number.",
                ResponseType = typeof(ApiResponse),
                StatusCode = StatusCodes.Status200OK
            };

            public static ApiEndpointMeta Delete => new()
            {
                Name = "Admin.Order.Shipments.Delete",
                Summary = "Delete shipment",
                Description = "Cancels and deletes a shipment.",
                ResponseType = typeof(ApiResponse),
                StatusCode = StatusCodes.Status200OK
            };

            public static ApiEndpointMeta AddItem => new()
            {
                Name = "Admin.Order.Shipments.AddItem",
                Summary = "Add item to shipment",
                Description = "Adds a product variant to an existing shipment.",
                ResponseType = typeof(ApiResponse),
                StatusCode = StatusCodes.Status200OK
            };

            public static ApiEndpointMeta RemoveItem => new()
            {
                Name = "Admin.Order.Shipments.RemoveItem",
                Summary = "Remove item from shipment",
                Description = "Removes a product variant from a shipment.",
                ResponseType = typeof(ApiResponse),
                StatusCode = StatusCodes.Status200OK
            };

            public static ApiEndpointMeta Ready => new()
            {
                Name = "Admin.Order.Shipments.Ready",
                Summary = "Mark shipment as ready",
                Description = "Transitions shipment state to Ready.",
                ResponseType = typeof(ApiResponse),
                StatusCode = StatusCodes.Status200OK
            };

            public static ApiEndpointMeta Ship => new()
            {
                Name = "Admin.Order.Shipments.Ship",
                Summary = "Ship shipment",
                Description = "Marks a shipment as shipped and records tracking information.",
                ResponseType = typeof(ApiResponse),
                StatusCode = StatusCodes.Status200OK
            };

            public static ApiEndpointMeta CancelAction => new()
            {
                Name = "Admin.Order.Shipments.CancelAction",
                Summary = "Cancel shipment",
                Description = "Cancels a shipment.",
                ResponseType = typeof(ApiResponse),
                StatusCode = StatusCodes.Status200OK
            };

            public static ApiEndpointMeta Deliver => new()
            {
                Name = "Admin.Order.Shipments.Deliver",
                Summary = "Deliver shipment",
                Description = "Marks a shipment as delivered to the customer.",
                ResponseType = typeof(ApiResponse),
                StatusCode = StatusCodes.Status200OK
            };

            public static ApiEndpointMeta Resume => new()
            {
                Name = "Admin.Order.Shipments.Resume",
                Summary = "Resume shipment",
                Description = "Resumes a canceled shipment.",
                ResponseType = typeof(ApiResponse),
                StatusCode = StatusCodes.Status200OK
            };

            public static ApiEndpointMeta ToPending => new()
            {
                Name = "Admin.Order.Shipments.ToPending",
                Summary = "Move to pending",
                Description = "Moves shipment back to pending state.",
                ResponseType = typeof(ApiResponse),
                StatusCode = StatusCodes.Status200OK
            };

            public static ApiEndpointMeta TransferToShipment => new()
            {
                Name = "Admin.Order.Shipments.TransferToShipment",
                Summary = "Transfer to shipment",
                Description = "Transfers inventory units from one shipment to another.",
                ResponseType = typeof(ApiResponse),
                StatusCode = StatusCodes.Status200OK
            };

            public static ApiEndpointMeta TransferToLocation => new()
            {
                Name = "Admin.Order.Shipments.TransferToLocation",
                Summary = "Transfer to location",
                Description = "Transfers inventory units from one shipment to a new shipment at a different location.",
                ResponseType = typeof(ApiResponse),
                StatusCode = StatusCodes.Status200OK
            };
        }

        public static class Payments
        {
            public static ApiEndpointMeta GetList => new()
            {
                Name = "Admin.Order.Payments.Get",
                Summary = "Get order payments",
                Description = "Retrieves all payments associated with an order.",
                ResponseType = typeof(ApiResponse<List<OrderModule.Payments.GetList.Result>>),
                StatusCode = StatusCodes.Status200OK
            };

            public static ApiEndpointMeta Create => new()
            {
                Name = "Admin.Order.Payments.Create",
                Summary = "Add payment",
                Description = "Adds a new payment record to the order.",
                ResponseType = typeof(ApiResponse<OrderModule.Payments.Create.Result>),
                StatusCode = StatusCodes.Status201Created
            };

            public static ApiEndpointMeta Authorize => new()
            {
                Name = "Admin.Order.Payments.Authorize",
                Summary = "Authorize payment",
                Description = "Marks a payment as authorized with a transaction ID.",
                ResponseType = typeof(ApiResponse),
                StatusCode = StatusCodes.Status200OK
            };

            public static ApiEndpointMeta Capture => new()
            {
                Name = "Admin.Order.Payments.Capture",
                Summary = "Capture payment",
                Description = "Marks a payment as captured/completed.",
                ResponseType = typeof(ApiResponse),
                StatusCode = StatusCodes.Status200OK
            };

            public static ApiEndpointMeta Refund => new()
            {
                Name = "Admin.Order.Payments.Refund",
                Summary = "Refund payment",
                Description = "Records a refund for a captured payment.",
                ResponseType = typeof(ApiResponse),
                StatusCode = StatusCodes.Status200OK
            };

            public static ApiEndpointMeta Void => new()
            {
                Name = "Admin.Order.Payments.Void",
                Summary = "Void payment",
                Description = "Voids an authorized but not yet captured payment.",
                ResponseType = typeof(ApiResponse),
                StatusCode = StatusCodes.Status200OK
            };
        }
    }
}