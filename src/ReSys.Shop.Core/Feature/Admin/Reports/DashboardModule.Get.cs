using ReSys.Shop.Core.Domain.Catalog.Products;
using ReSys.Shop.Core.Domain.Catalog.Products.Reviews;
using ReSys.Shop.Core.Domain.Catalog.Products.Variants;
using ReSys.Shop.Core.Domain.Identity.Users;
using ReSys.Shop.Core.Domain.Orders;
using ReSys.Shop.Core.Domain.Orders.Payments;
using ReSys.Shop.Core.Domain.Settings;
using ReSys.Shop.Core.Domain.Settings.Stores;

namespace ReSys.Shop.Core.Feature.Admin.Reports;

public static partial class DashboardModule
{
    public static class Get
    {
        public sealed record SummaryQuery : IQuery<Models.DashboardSummary>;

        public sealed class SummaryHandler(IApplicationDbContext dbContext)
            : IQueryHandler<SummaryQuery, Models.DashboardSummary>
        {
            public async Task<ErrorOr<Models.DashboardSummary>> Handle(SummaryQuery request, CancellationToken ct)
            {
                var now = DateTimeOffset.UtcNow;
                var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
                var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);

                var completedOrders = await dbContext.Set<Order>()
                    .Where(o => o.State == Order.OrderState.Complete)
                    .ToListAsync(ct);

                var allOrders = await dbContext.Set<Order>().AsNoTracking().ToListAsync(ct);
                var allUsersCount = await dbContext.Set<User>().CountAsync(ct);
                var newUsersThisMonth = await dbContext.Set<User>()
                    .CountAsync(u => u.CreatedAt >= monthStart, ct);

                var products = dbContext.Set<Product>().AsNoTracking();
                var activeProductsCount = await products.CountAsync(p => p.Status == Product.ProductStatus.Active, ct);
                var totalProductsCount = await products.CountAsync(ct);

                var pendingReviewsCount = await dbContext.Set<Review>()
                    .CountAsync(r => r.Status == Review.ReviewStatus.Pending, ct);

                // Revenue calculation based on captured payments minus refunds
                var allCapturedPayments = await dbContext.Set<Payment>()
                    .Where(p => p.State == Payment.PaymentState.Completed || p.State == Payment.PaymentState.Refunded)
                    .ToListAsync(ct);

                var totalRevenue = allCapturedPayments.Sum(p => (p.AmountCents - p.RefundedAmountCents) / 100m);
                var monthlyRevenue = allCapturedPayments
                    .Where(p => p.CapturedAt >= monthStart)
                    .Sum(p => (p.AmountCents - p.RefundedAmountCents) / 100m);
                var dailyRevenue = allCapturedPayments
                    .Where(p => p.CapturedAt >= todayStart)
                    .Sum(p => (p.AmountCents - p.RefundedAmountCents) / 100m);

                return new Models.DashboardSummary
                {
                    Sales = new Models.SalesMetrics
                    {
                        TotalRevenue = totalRevenue,
                        MonthlyRevenue = monthlyRevenue,
                        DailyRevenue = dailyRevenue
                    },
                    Orders = new Models.OrderMetrics
                    {
                        TotalOrders = allOrders.Count,
                        PendingOrders = allOrders.Count(o => o.State == Order.OrderState.Address || o.State == Order.OrderState.Delivery),
                        ProcessingOrders = allOrders.Count(o => o.State == Order.OrderState.Payment || o.State == Order.OrderState.Confirm),
                        CompletedOrders = completedOrders.Count,
                        TodayOrders = allOrders.Count(o => o.CreatedAt >= todayStart)
                    },
                    Catalog = new Models.CatalogMetrics
                    {
                        TotalProducts = totalProductsCount,
                        ActiveProducts = activeProductsCount,
                        PendingReviews = pendingReviewsCount
                    },
                    Customers = new Models.CustomerMetrics
                    {
                        TotalCustomers = allUsersCount,
                        NewCustomersThisMonth = newUsersThisMonth
                    }
                };
            }
        }

        public sealed record RecentActivityQuery : IQuery<Models.RecentActivity>;

        public sealed class RecentActivityHandler(IApplicationDbContext dbContext)
            : IQueryHandler<RecentActivityQuery, Models.RecentActivity>
        {
            public async Task<ErrorOr<Models.RecentActivity>> Handle(RecentActivityQuery request, CancellationToken ct)
            {
                var latestOrders = await dbContext.Set<Order>()
                    .Include(o => o.User)
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(10)
                    .Select(o => new Models.RecentOrder(
                        o.Id,
                        o.Number,
                        o.User != null ? (o.User.FirstName + " " + o.User.LastName) : "Guest",
                        o.Total,
                        o.State.ToString(),
                        o.CreatedAt))
                    .ToListAsync(ct);

                var latestReviews = await dbContext.Set<Review>()
                    .Include(r => r.Product)
                    .Include(r => r.User)
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(10)
                    .Select(r => new Models.RecentReview(
                        r.Id,
                        r.Product != null ? r.Product.Name : "Unknown",
                        r.User != null ? (r.User.FirstName + " " + r.User.LastName) : "Anonymous",
                        r.Rating,
                        r.CreatedAt))
                    .ToListAsync(ct);

                return new Models.RecentActivity
                {
                    LatestOrders = latestOrders,
                    LatestReviews = latestReviews
                };
            }
        }

        public sealed record InventoryAlertsQuery : IQuery<List<Models.InventoryAlert>>;

        public sealed class InventoryAlertsHandler(IApplicationDbContext dbContext)
            : IQueryHandler<InventoryAlertsQuery, List<Models.InventoryAlert>>
        {
            public async Task<ErrorOr<List<Models.InventoryAlert>>> Handle(InventoryAlertsQuery request, CancellationToken ct)
            {
                var thresholdSetting = await dbContext.Set<Setting>()
                    .FirstOrDefaultAsync(s => s.Key == SettingKey.Inventory(InventorySettingKey.LowStockThreshold), ct);

                int threshold = 5;
                if (thresholdSetting != null && int.TryParse(thresholdSetting.Value, out var val))
                {
                    threshold = val;
                }

                var alerts = await dbContext.Set<Variant>()
                    .Include(v => v.Product)
                    .Include(v => v.StockItems)
                    .Where(v => v.TrackInventory)
                    .ToListAsync(ct);

                var result = alerts
                    .Select(v => new
                    {
                        Variant = v,
                        Stock = v.StockItems.Sum(si => si.QuantityOnHand)
                    })
                    .Where(x => x.Stock <= threshold)
                    .OrderBy(x => x.Stock)
                    .Take(20)
                    .Select(x => new Models.InventoryAlert(
                        x.Variant.Id,
                        x.Variant.Sku ?? "N/A",
                        x.Variant.Product.Name,
                        x.Stock,
                        (int)x.Variant.StockItems.Sum(si => si.MaxBackorderQuantity)))
                    .ToList();

                return result;
            }
        }

        public sealed record SalesAnalysisQuery(int Days = 30) : IQuery<Models.SalesAnalysis>;

        public sealed class SalesAnalysisHandler(IApplicationDbContext dbContext)
            : IQueryHandler<SalesAnalysisQuery, Models.SalesAnalysis>
        {
            public async Task<ErrorOr<Models.SalesAnalysis>> Handle(SalesAnalysisQuery request, CancellationToken ct)
            {
                var startDate = DateTimeOffset.UtcNow.AddDays(-request.Days);

                var completedOrders = await dbContext.Set<Order>()
                    .Include(o => o.LineItems)
                        .ThenInclude(li => li.Variant)
                            .ThenInclude(v => v.Product)
                                .ThenInclude(p => p.Classifications)
                                    .ThenInclude(c => c.Taxon)
                    .Where(o => o.State == Order.OrderState.Complete && o.CompletedAt >= startDate)
                    .ToListAsync(ct);

                // Revenue Trends
                var revenueTrends = completedOrders
                    .GroupBy(o => o.CompletedAt!.Value.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new Models.TimeSeriesData(g.Key.ToShortDateString(), g.Sum(o => o.Total)))
                    .ToList();

                // Sales By Category
                var categorySales = completedOrders
                    .SelectMany(o => o.LineItems)
                    .SelectMany(li => li.Variant.Product.Taxons.Select(t => new { t.Name, li.TotalCents, li.Quantity }))
                    .GroupBy(x => x.Name)
                    .Select(g => new Models.CategorySales(g.Key, g.Sum(x => x.TotalCents) / 100m, g.Sum(x => x.Quantity)))
                    .OrderByDescending(x => x.Revenue)
                    .ToList();

                // Top Selling Products
                var topProducts = completedOrders
                    .SelectMany(o => o.LineItems)
                    .GroupBy(li => new { li.Variant.Product.Id, li.Variant.Product.Name })
                    .Select(g => new Models.TopProduct(g.Key.Id, g.Key.Name, g.Sum(li => li.Quantity), g.Sum(li => li.TotalCents) / 100m))
                    .OrderByDescending(p => p.QuantitySold)
                    .Take(10)
                    .ToList();

                return new Models.SalesAnalysis
                {
                    RevenueTrends = revenueTrends,
                    SalesByCategory = categorySales,
                    TopSellingProducts = topProducts
                };
            }
        }

        public sealed record OrderTrendsQuery(int Days = 30) : IQuery<Models.OrderTrend>;

        public sealed class OrderTrendsHandler(IApplicationDbContext dbContext)
            : IQueryHandler<OrderTrendsQuery, Models.OrderTrend>
        {
            public async Task<ErrorOr<Models.OrderTrend>> Handle(OrderTrendsQuery request, CancellationToken ct)
            {
                var startDate = DateTimeOffset.UtcNow.AddDays(-request.Days);

                var orders = await dbContext.Set<Order>()
                    .Where(o => o.CreatedAt >= startDate)
                    .ToListAsync(ct);

                var orderVolume = orders
                    .GroupBy(o => o.CreatedAt.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new Models.TimeSeriesData(g.Key.ToShortDateString(), g.Count()))
                    .ToList();

                var ordersByStatus = orders
                    .GroupBy(o => o.State)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count());

                return new Models.OrderTrend
                {
                    OrderVolume = orderVolume,
                    OrdersByStatus = ordersByStatus
                };
            }
        }
    }
}
