using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ReSys.Shop.Core.Common.Models.Wrappers.Responses;

namespace ReSys.Shop.Core.Feature.Admin.Reports;

public static partial class DashboardModule
{
    public sealed class Endpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup(prefix: "api/admin/dashboard")
                .UseGroupMeta(meta: Annotations.Group)
                .RequireAuthorization();

            group.MapGet(pattern: "summary", handler: async (
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Get.SummaryQuery(), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Dashboard summary retrieved successfully"));
                })
                .UseEndpointMeta(meta: Annotations.GetSummary);

            group.MapGet(pattern: "recent-activity", handler: async (
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Get.RecentActivityQuery(), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Recent activity retrieved successfully"));
                })
                .UseEndpointMeta(meta: Annotations.GetRecentActivity);

            group.MapGet(pattern: "inventory-alerts", handler: async (
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Get.InventoryAlertsQuery(), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Inventory alerts retrieved successfully"));
                })
                .UseEndpointMeta(meta: Annotations.GetInventoryAlerts);

            group.MapGet(pattern: "sales-analysis", handler: async (
                    [FromQuery] int days,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Get.SalesAnalysisQuery(days > 0 ? days : 30), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Sales analysis retrieved successfully"));
                })
                .UseEndpointMeta(meta: Annotations.GetSalesAnalysis);

            group.MapGet(pattern: "order-trends", handler: async (
                    [FromQuery] int days,
                    [FromServices] ISender mediator,
                    CancellationToken ct) =>
                {
                    var result = await mediator.Send(request: new Get.OrderTrendsQuery(days > 0 ? days : 30), cancellationToken: ct);
                    return TypedResults.Ok(value: result.ToApiResponse(message: "Order trends retrieved successfully"));
                })
                .UseEndpointMeta(meta: Annotations.GetOrderTrends);
        }
    }
}
