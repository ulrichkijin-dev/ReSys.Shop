using Microsoft.AspNetCore.Http;
using ReSys.Shop.Core.Common.Models.Wrappers.PagedLists;
using ReSys.Shop.Core.Common.Models.Wrappers.Responses;

namespace ReSys.Shop.Core.Feature.Storefront.Countries;

public static partial class CountryModule
{
    private static class Annotations
    {
        public static ApiGroupMeta Group => new()
        {
            Name = "Storefront.Country",
            Tags = ["Storefront Countries"],
            Summary = "Storefront Country API",
            Description = "Endpoints for retrieving countries and states for the storefront."
        };

        public static ApiEndpointMeta GetPagedList => new()
        {
            Name = "Storefront.Country.List",
            Summary = "List all countries",
            Description = "Retrieves a paginated list of countries.",
            ResponseType = typeof(ApiResponse<PaginationList<Models.CountryItem>>),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta GetById => new()
        {
            Name = "Storefront.Country.GetById",
            Summary = "Retrieve a country",
            Description = "Retrieves detailed information about a specific country by its ID.",
            ResponseType = typeof(ApiResponse<Models.CountryItem>),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta GetDefault => new()
        {
            Name = "Storefront.Country.GetDefault",
            Summary = "Retrieve default country",
            Description = "Retrieves the default country (typically based on store settings).",
            ResponseType = typeof(ApiResponse<Models.CountryItem>),
            StatusCode = StatusCodes.Status200OK
        };

        public static class States
        {
            public static ApiEndpointMeta GetPagedList => new()
            {
                Name = "Storefront.State.List",
                Summary = "List all states",
                Description = "Retrieves a paginated list of states.",
                ResponseType = typeof(ApiResponse<PaginationList<Models.StateItem>>),
                StatusCode = StatusCodes.Status200OK
            };

            public static ApiEndpointMeta GetById => new()
            {
                Name = "Storefront.State.GetById",
                Summary = "Retrieve a state",
                Description = "Retrieves detailed information about a specific state by its ID.",
                ResponseType = typeof(ApiResponse<Models.StateItem>),
                StatusCode = StatusCodes.Status200OK
            };
        }
    }
}
