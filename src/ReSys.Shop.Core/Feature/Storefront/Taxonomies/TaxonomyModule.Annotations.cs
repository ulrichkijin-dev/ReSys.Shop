using Microsoft.AspNetCore.Http;
using ReSys.Shop.Core.Common.Models.Wrappers.PagedLists;
using ReSys.Shop.Core.Common.Models.Wrappers.Responses;

namespace ReSys.Shop.Core.Feature.Storefront.Taxonomies;

public static partial class TaxonomyModule
{
    private static class Annotations
    {
        public static ApiGroupMeta Group => new()
        {
            Name = "Storefront.Taxonomy",
            Tags = ["Storefront Taxonomies"],
            Summary = "Storefront Taxonomy API",
            Description = "Endpoints for retrieving taxonomies (hierarchical groupings of categories)."
        };

        public static ApiEndpointMeta GetPagedList => new()
        {
            Name = "Storefront.Taxonomy.List",
            Summary = "List all taxonomies",
            Description = "Retrieves a paginated list of taxonomies including their root taxons.",
            ResponseType = typeof(ApiResponse<PaginationList<Models.TaxonomyItem>>),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta GetById => new()
        {
            Name = "Storefront.Taxonomy.GetById",
            Summary = "Retrieve a taxonomy",
            Description = "Retrieves detailed information about a specific taxonomy by its ID.",
            ResponseType = typeof(ApiResponse<Models.TaxonomyItem>),
            StatusCode = StatusCodes.Status200OK
        };
    }
}
