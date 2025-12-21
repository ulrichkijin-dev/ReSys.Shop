using Microsoft.AspNetCore.Http;
using ReSys.Shop.Core.Common.Models.Wrappers.PagedLists;
using ReSys.Shop.Core.Common.Models.Wrappers.Responses;

namespace ReSys.Shop.Core.Feature.Storefront.Orders;

public static partial class OrderModule
{
    private static class Annotations
    {
        public static ApiGroupMeta Group => new()
        {
            Name = "Storefront.Order",
            Tags = ["Storefront Account Orders"],
            Summary = "Customer Order History API",
            Description = "Endpoints for customers to view their own order history and status"
        };

        public static ApiEndpointMeta GetPagedList => new()
        {
            Name = "Storefront.Order.List",
            Summary = "List customer orders",
            Description = "Retrieves a paginated list of orders placed by the current user.",
            ResponseType = typeof(ApiResponse<PaginationList<Models.OrderItem>>),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta GetByNumber => new()
        {
            Name = "Storefront.Order.GetByNumber",
            Summary = "Get order details",
            Description = "Retrieves detailed information about a specific order by its number.",
            ResponseType = typeof(ApiResponse<Models.OrderDetail>),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta GetByToken => new()
        {
            Name = "Storefront.Order.GetByToken",
            Summary = "Get order by guest token",
            Description = "Retrieves detailed information about a guest order using its unique token.",
            ResponseType = typeof(ApiResponse<Models.OrderDetail>),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta GetStatus => new()
        {
            Name = "Storefront.Order.GetStatus",
            Summary = "Get order status",
            Description = "Retrieves the current status of an order.",
            ResponseType = typeof(ApiResponse<Models.OrderStatus>),
            StatusCode = StatusCodes.Status200OK
        };
    }
}
