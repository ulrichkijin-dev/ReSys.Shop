using Microsoft.AspNetCore.Http;

using ReSys.Shop.Core.Common.Models.Wrappers.Responses;

namespace  ReSys.Shop.Core.Feature.Admin.Settings.SettingModule;

public static partial class SettingModule
{
    private static class Annotations
    {
        // ---------------- Group Metadata ----------------
        public static ApiGroupMeta Group => new()
        {
            Name = "Admin.Settings.Setting",
            Tags = ["Setting"],
            Summary = "Setting Management API",
            Description = "Endpoints for managing application settings"
        };

        // ---------------- Endpoint Metadata ----------------
        public static ApiEndpointMeta Create => new()
        {
            Name = "Admin.Settings.Setting.Create",
            Summary = "Create a new setting",
            Description = "Creates a new application setting with the specified details.",
            ResponseType = typeof(ApiResponse<SettingModule.Create.Result>),
            StatusCode = StatusCodes.Status201Created
        };

        public static ApiEndpointMeta Delete => new()
        {
            Name = "Admin.Settings.Setting.Delete",
            Summary = "Delete a setting",
            Description = "Deletes an application setting by ID.",
            ResponseType = typeof(ApiResponse),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta GetById => new()
        {
            Name = "Admin.Settings.Setting.GetById",
            Summary = "Get setting details",
            Description = "Retrieves details of a specific application setting by ID.",
            ResponseType = typeof(ApiResponse<SettingModule.Get.ById.Result>),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta GetPagedList => new()
        {
            Name = "Admin.Settings.Setting.GetPagedList",
            Summary = "Get paged list of settings",
            Description = "Retrieves a paginated list of application settings.",
            ResponseType = typeof(ApiResponse<List<SettingModule.Get.PagedList.Result>>),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta GetSelectList => new()
        {
            Name = "Admin.Settings.Setting.GetSelectList",
            Summary = "Get selectable list of settings",
            Description = "Retrieves a simplified list of application settings for selection purposes.",
            ResponseType = typeof(ApiResponse<List<SettingModule.Get.SelectList.Result>>),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta Update => new()
        {
            Name = "Admin.Settings.Setting.Update",
            Summary = "Update a setting",
            Description = "Updates an existing application setting by ID.",
            ResponseType = typeof(ApiResponse<Update.Result>),
            StatusCode = StatusCodes.Status200OK
        };
    }
}
