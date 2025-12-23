using Microsoft.AspNetCore.Http;
using ReSys.Shop.Core.Common.Models.Wrappers.PagedLists;
using ReSys.Shop.Core.Common.Models.Wrappers.Responses;

namespace ReSys.Shop.Core.Feature.Storefront.Reviews;

public static partial class ReviewModule
{
    private static class Annotations
    {
        public static ApiGroupMeta Group => new()
        {
            Name = "Storefront.Review",
            Tags = ["Storefront Reviews"],
            Summary = "Product Reviews API",
            Description = "Endpoints for customers to read and post product reviews."
        };

        public static ApiEndpointMeta GetPagedList => new()
        {
            Name = "Storefront.Review.List",
            Summary = "List product reviews",
            Description = "Retrieves a paginated list of approved reviews for a specific product.",
            ResponseType = typeof(ApiResponse<PaginationList<Models.ReviewItem>>),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta Create => new()
        {
            Name = "Storefront.Review.Create",
            Summary = "Post a review",
            Description = "Submits a new review for a product. Reviews are pending moderation by default.",
            ResponseType = typeof(ApiResponse<Models.ReviewItem>),
            StatusCode = StatusCodes.Status201Created
        };

        public static ApiEndpointMeta Vote => new()
        {
            Name = "Storefront.Review.Vote",
            Summary = "Vote on review helpfulness",
            Description = "Allows a user to vote whether a review was helpful or not.",
            ResponseType = typeof(ApiResponse<Models.ReviewItem>),
            StatusCode = StatusCodes.Status200OK
        };
    }
}
