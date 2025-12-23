using Microsoft.AspNetCore.Http;
using ReSys.Shop.Core.Common.Models.Wrappers.PagedLists;
using ReSys.Shop.Core.Common.Models.Wrappers.Responses;

namespace ReSys.Shop.Core.Feature.Storefront.Taxons;

public static partial class TaxonModule
{
    private static class Annotations
    {
        public static ApiGroupMeta Group => new()
        {
            Name = "Storefront.Taxon",
            Tags = ["Storefront Taxons"],
            Summary = "Storefront Taxon API",
            Description = "Endpoints for retrieving taxons (categories) for the storefront."
        };

        public static ApiEndpointMeta GetPagedList => new()
        {
            Name = "Storefront.Taxon.List",
            Summary = "List all taxons",
            Description = "Retrieves a paginated list of taxons.",
            ResponseType = typeof(ApiResponse<PaginationList<Models.TaxonItem>>),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta GetById => new()
        {
            Name = "Storefront.Taxon.GetById",
            Summary = "Retrieve a taxon",
            Description = "Retrieves detailed information about a specific taxon by its ID.",
            ResponseType = typeof(ApiResponse<Models.TaxonItem>),
            StatusCode = StatusCodes.Status200OK
        };
    }
}
