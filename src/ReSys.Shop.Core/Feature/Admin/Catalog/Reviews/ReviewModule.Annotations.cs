using Microsoft.AspNetCore.Http;
using ReSys.Shop.Core.Common.Models.Wrappers.PagedLists;
using ReSys.Shop.Core.Common.Models.Wrappers.Responses;

namespace ReSys.Shop.Core.Feature.Admin.Catalog.Reviews;

public static partial class ReviewModule
{
    private static class Annotations
    {
        public static ApiGroupMeta Group => new()
        {
            Name = "Admin.Review",
            Tags = ["Admin Catalog Reviews"],
            Summary = "Product Reviews Administration",
            Description = "Endpoints for administrators to moderate and manage product reviews."
        };

        public static ApiEndpointMeta GetPagedList => new()
        {
            Name = "Admin.Review.List",
            Summary = "List all reviews",
            Description = "Retrieves a paginated list of all product reviews with filtering and search.",
            ResponseType = typeof(ApiResponse<PaginationList<Models.ReviewItem>>),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta GetById => new()
        {
            Name = "Admin.Review.GetById",
            Summary = "Get review details",
            Description = "Retrieves detailed information about a specific review.",
            ResponseType = typeof(ApiResponse<Models.ReviewItem>),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta Approve => new()
        {
            Name = "Admin.Review.Approve",
            Summary = "Approve review",
            Description = "Approves a review, making it visible on the storefront.",
            ResponseType = typeof(ApiResponse<Models.ReviewItem>),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta Reject => new()
        {
            Name = "Admin.Review.Reject",
            Summary = "Reject review",
            Description = "Rejects a review with a reason.",
            ResponseType = typeof(ApiResponse<Models.ReviewItem>),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta Delete => new()
        {
            Name = "Admin.Review.Delete",
            Summary = "Delete review",
            Description = "Permanently deletes a product review.",
            ResponseType = typeof(ApiResponse),
            StatusCode = StatusCodes.Status200OK
        };
    }
}
