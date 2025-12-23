using Microsoft.AspNetCore.Http;
using ReSys.Shop.Core.Common.Models.Wrappers.Responses;

namespace ReSys.Shop.Core.Feature.Storefront.Account;

public static partial class AccountModule
{
    private static class Annotations
    {
        public static ApiGroupMeta Group => new()
        {
            Name = "Storefront.Account",
            Tags = ["Storefront Account"],
            Summary = "Storefront Account API",
            Description = "Endpoints for managing customer account, profile, and registration."
        };

        public static ApiEndpointMeta Register => new()
        {
            Name = "Storefront.Account.Register",
            Summary = "Register new account",
            Description = "Creates a new customer account.",
            ResponseType = typeof(ApiResponse<Models.AccountDetail>),
            StatusCode = StatusCodes.Status201Created
        };

        public static ApiEndpointMeta GetProfile => new()
        {
            Name = "Storefront.Account.GetProfile",
            Summary = "Get account profile",
            Description = "Retrieves the profile information for the current authenticated user.",
            ResponseType = typeof(ApiResponse<Models.AccountDetail>),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta UpdateProfile => new()
        {
            Name = "Storefront.Account.UpdateProfile",
            Summary = "Update account profile",
            Description = "Updates the profile information for the current authenticated user.",
            ResponseType = typeof(ApiResponse<Models.AccountDetail>),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta DeleteAccount => new()
        {
            Name = "Storefront.Account.Delete",
            Summary = "Delete account",
            Description = "Closes and deletes the current user account.",
            ResponseType = typeof(ApiResponse),
            StatusCode = StatusCodes.Status200OK
        };
    }
}
