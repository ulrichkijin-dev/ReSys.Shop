using FluentAssertions;


using Microsoft.EntityFrameworkCore;


using ReSys.Shop.Core.Domain.Catalog.Products;
using ReSys.Shop.Core.Domain.Catalog.Products.Variants;
using ReSys.Shop.Core.Domain.Orders;
using ReSys.Shop.Core.Domain.Orders.Payments;
using ReSys.Shop.Core.Domain.Settings;
using ReSys.Shop.Core.Domain.Settings.Stores;
using ReSys.Shop.Core.Feature.Admin.Reports;
using ReSys.Shop.Infrastructure.Persistence.Contexts;

namespace ReSys.Shop.Tests;

public class DashboardTests
{
    private readonly ApplicationDbContext _dbContext;

    public DashboardTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
    }

    private async Task<Variant> CreateVariant(string sku, int stock)
    {
        var product = Product.Create(name: "Product " + sku, slug: "product-" + sku.ToLower()).Value;
        product.Activate();
        
        // Ensure Master variant doesn't interfere with alerts
        var master = product.GetMaster().Value;
        master.TrackInventory = false;
        
        _dbContext.Set<Product>().Add(product);
        
        var variant = Variant.Create(productId: product.Id, isMaster: false, sku: sku, trackInventory: true).Value;
        variant.Product = product;
        
        var locationId = Guid.NewGuid();
        var stockItem = ReSys.Shop.Core.Domain.Inventories.Stocks.StockItem.Create(variant.Id, locationId, sku, quantityOnHand: stock).Value;
        variant.StockItems.Add(stockItem);
        
        _dbContext.Set<Variant>().Add(variant);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return variant;
    }

    [Fact]
    public async Task Summary_ShouldCalculateRevenueFromCapturedPaymentsOnly()
    {
        // Arrange
        var order1 = Order.Create(Guid.NewGuid(), "USD").Value;
        var order2 = Order.Create(Guid.NewGuid(), "USD").Value;
        _dbContext.Set<Order>().AddRange(order1, order2);

        // Captured payment for order 1 ($100)
        var p1 = Payment.Create(order1.Id, 10000, "USD", "CC", Guid.NewGuid()).Value;
        p1.MarkAsCaptured("T1");
        p1.CapturedAt = DateTimeOffset.UtcNow;

        // Pending payment for order 2 ($50) - should NOT be counted
        var p2 = Payment.Create(order2.Id, 5000, "USD", "CC", Guid.NewGuid()).Value;

        // Refunded payment ($100 original, $30 refunded) -> $70 net
        var order3 = Order.Create(Guid.NewGuid(), "USD").Value;
        var p3 = Payment.Create(order3.Id, 10000, "USD", "CC", Guid.NewGuid()).Value;
        p3.MarkAsCaptured("T3");
        p3.CapturedAt = DateTimeOffset.UtcNow;
        p3.Refund(3000, "Partial");

        _dbContext.Set<Payment>().AddRange(p1, p2, p3);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new DashboardModule.Get.SummaryHandler(_dbContext);

        // Act
        var result = await handler.Handle(new DashboardModule.Get.SummaryQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        // Total Revenue = $100 (p1) + $70 (p3 net) = $170
        result.Value.Sales.TotalRevenue.Should().Be(170m);
    }

    [Fact]
    public async Task InventoryAlerts_ShouldUseThresholdFromSettings()
    {
        // Arrange
        await CreateVariant("LOW-1", 2); // Below default (5) and custom (3)
        await CreateVariant("MID-1", 4); // Below default (5) but above custom (3)
        await CreateVariant("HIGH-1", 10); // Above both

        // Set custom threshold to 3
        var setting = Setting.Create(SettingKey.Inventory(InventorySettingKey.LowStockThreshold), "3", "Desc", "5", ConfigurationValueType.Integer).Value;
        _dbContext.Set<Setting>().Add(setting);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new DashboardModule.Get.InventoryAlertsHandler(_dbContext);

        // Act
        var result = await handler.Handle(new DashboardModule.Get.InventoryAlertsQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        // Only LOW-1 should be returned because its stock (2) <= custom threshold (3)
        // MID-1 (4) is > 3, so it should be excluded
        result.Value.Should().HaveCount(1);
        result.Value.First().Sku.Should().Be("LOW-1");
    }
}