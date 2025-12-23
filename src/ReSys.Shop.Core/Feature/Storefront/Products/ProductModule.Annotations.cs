using Microsoft.AspNetCore.Http;
using ReSys.Shop.Core.Common.Models.Wrappers.PagedLists;
using ReSys.Shop.Core.Common.Models.Wrappers.Responses;

namespace ReSys.Shop.Core.Feature.Storefront.Products;

public static partial class ProductModule
{
    private static class Annotations
    {
        public static ApiGroupMeta Group => new()
        {
            Name = "Storefront.Product",
            Tags = ["Storefront Products"],
            Summary = "Storefront Product API",
            Description = "Endpoints for searching and retrieving products for the storefront."
        };

        public static ApiEndpointMeta GetPagedList => new()
        {
            Name = "Storefront.Product.List",
            Summary = "List all products",
            Description = "Retrieves a paginated list of products.",
            ResponseType = typeof(ApiResponse<PaginationList<Models.ProductItem>>),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta GetBySlug => new()
        {
            Name = "Storefront.Product.GetBySlug",
            Summary = "Retrieve a product",
            Description = "Retrieves detailed information about a specific product by its slug.",
            ResponseType = typeof(ApiResponse<Models.ProductDetail>),
            StatusCode = StatusCodes.Status200OK
        };
    }
}
