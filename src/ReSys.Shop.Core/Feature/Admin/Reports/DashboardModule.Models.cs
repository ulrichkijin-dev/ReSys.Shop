namespace ReSys.Shop.Core.Feature.Admin.Reports;

public static partial class DashboardModule
{
    public static class Models
    {
        public record DashboardSummary
        {
            public SalesMetrics Sales { get; init; } = new();
            public OrderMetrics Orders { get; init; } = new();
            public CatalogMetrics Catalog { get; init; } = new();
            public CustomerMetrics Customers { get; init; } = new();
        }

        public record SalesMetrics
        {
            public decimal TotalRevenue { get; init; }
            public decimal MonthlyRevenue { get; init; }
            public decimal DailyRevenue { get; init; }
            public string Currency { get; init; } = "USD";
        }

        public record OrderMetrics
        {
            public int TotalOrders { get; init; }
            public int PendingOrders { get; init; }
            public int ProcessingOrders { get; init; }
            public int CompletedOrders { get; init; }
            public int TodayOrders { get; init; }
        }

        public record CatalogMetrics
        {
            public int TotalProducts { get; init; }
            public int ActiveProducts { get; init; }
            public int OutOfStockVariants { get; init; }
            public int PendingReviews { get; init; }
        }

        public record CustomerMetrics
        {
            public int TotalCustomers { get; init; }
            public int NewCustomersThisMonth { get; init; }
        }

        public record RecentActivity
        {
            public List<RecentOrder> LatestOrders { get; init; } = [];
            public List<RecentReview> LatestReviews { get; init; } = [];
        }

        public record RecentOrder(Guid Id, string Number, string CustomerName, decimal Total, string State, DateTimeOffset CreatedAt);
        public record RecentReview(Guid Id, string ProductName, string CustomerName, int Rating, DateTimeOffset CreatedAt);

        public record InventoryAlert(Guid VariantId, string Sku, string ProductName, double QuantityOnHand, int BackorderLimit);

        public record SalesAnalysis
        {
            public List<TimeSeriesData> RevenueTrends { get; init; } = [];
            public List<CategorySales> SalesByCategory { get; init; } = [];
            public List<TopProduct> TopSellingProducts { get; init; } = [];
        }

        public record OrderTrend
        {
            public List<TimeSeriesData> OrderVolume { get; init; } = [];
            public Dictionary<string, int> OrdersByStatus { get; init; } = [];
        }

        public record TimeSeriesData(string Label, decimal Value);
        public record CategorySales(string CategoryName, decimal Revenue, int OrderCount);
        public record TopProduct(Guid ProductId, string ProductName, int QuantitySold, decimal Revenue);
    }
}
