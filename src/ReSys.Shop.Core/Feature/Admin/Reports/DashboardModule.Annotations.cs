using Microsoft.AspNetCore.Http;
using ReSys.Shop.Core.Common.Models.Wrappers.Responses;

namespace ReSys.Shop.Core.Feature.Admin.Reports;

public static partial class DashboardModule
{
    private static class Annotations
    {
        public static ApiGroupMeta Group => new()
        {
            Name = "Admin.Dashboard",
            Tags = ["Admin Dashboard"],
            Summary = "Store Administration Dashboard",
            Description = "Endpoints providing aggregated data and KPIs for the admin dashboard."
        };

        public static ApiEndpointMeta GetSummary => new()
        {
            Name = "Admin.Dashboard.GetSummary",
            Summary = "Get dashboard summary",
            Description = "Retrieves high-level KPIs including sales, orders, and catalog statistics.",
            ResponseType = typeof(ApiResponse<Models.DashboardSummary>),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta GetRecentActivity => new()
        {
            Name = "Admin.Dashboard.GetRecentActivity",
            Summary = "Get recent activity",
            Description = "Retrieves the most recent orders and reviews.",
            ResponseType = typeof(ApiResponse<Models.RecentActivity>),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta GetInventoryAlerts => new()
        {
            Name = "Admin.Dashboard.GetInventoryAlerts",
            Summary = "Get inventory alerts",
            Description = "Retrieves a list of variants with low or zero stock.",
            ResponseType = typeof(ApiResponse<List<Models.InventoryAlert>>),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta GetSalesAnalysis => new()
        {
            Name = "Admin.Dashboard.GetSalesAnalysis",
            Summary = "Get sales analysis",
            Description = "Retrieves detailed sales analysis including revenue trends and top products.",
            ResponseType = typeof(ApiResponse<Models.SalesAnalysis>),
            StatusCode = StatusCodes.Status200OK
        };

        public static ApiEndpointMeta GetOrderTrends => new()
        {
            Name = "Admin.Dashboard.GetOrderTrends",
            Summary = "Get order trends",
            Description = "Retrieves order volume trends and status distribution.",
            ResponseType = typeof(ApiResponse<Models.OrderTrend>),
            StatusCode = StatusCodes.Status200OK
        };
    }
}
