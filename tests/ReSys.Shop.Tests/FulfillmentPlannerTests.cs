using FluentAssertions;


using Microsoft.EntityFrameworkCore;


using ReSys.Shop.Core.Domain.Catalog.Products;
using ReSys.Shop.Core.Domain.Catalog.Products.Variants;
using ReSys.Shop.Core.Domain.Inventories.FulfillmentStrategies;
using ReSys.Shop.Core.Domain.Inventories.Locations;
using ReSys.Shop.Core.Domain.Inventories.Stocks;
using ReSys.Shop.Core.Domain.Orders;
using ReSys.Shop.Infrastructure.Persistence.Contexts;

namespace ReSys.Shop.Tests;

public class FulfillmentPlannerTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly FulfillmentStrategyFactory _factory;

    public FulfillmentPlannerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _factory = new FulfillmentStrategyFactory();
    }

    [Fact]
    public async Task PlanFulfillment_ShouldCreateValidPlan()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        
        var product = Product.Create("Test", "test").Value;
        product.Activate();
        _dbContext.Set<Product>().Add(product);

        var variant = Variant.Create(product.Id, sku: "V1").Value;
        variant.SetPrice(100, null, "USD");
        product.AddVariant(variant);
        _dbContext.Set<Variant>().Add(variant);

        var loc = StockLocation.Create("Warehouse").Value;
        loc.Id = locationId;
        var si = StockItem.Create(variant.Id, loc.Id, "V1-W", 100).Value;
        loc.StockItems.Add(si);
        _dbContext.Set<StockLocation>().Add(loc);
        _dbContext.Set<StockItem>().Add(si);

        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var order = Order.Create(storeId, "USD").Value;
        order.AddLineItem(variant, 5);
        _dbContext.Set<Order>().Add(order);

        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var planner = new FulfillmentPlanner(_factory, _dbContext);

        // Act
        var result = await planner.PlanFulfillment(order, "HighestStock", TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : "");
        result.Value.IsFullyFulfillable.Should().BeTrue();
        result.Value.Shipments.Should().HaveCount(1);
        result.Value.Shipments[0].FulfillmentLocationId.Should().Be(locationId);
        result.Value.Shipments[0].Items.Should().HaveCount(1);
        result.Value.Shipments[0].Items[0].Quantity.Should().Be(5);
        result.Value.Shipments[0].Items[0].IsBackordered.Should().BeFalse();
    }

    [Fact]
    public async Task PlanFulfillment_WithInsufficientStock_ShouldMarkAsBackordered()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        
        var product = Product.Create("Test", "test").Value;
        product.Activate();
        _dbContext.Set<Product>().Add(product);

        var variant = Variant.Create(product.Id, sku: "V1").Value;
        variant.SetPrice(100, null, "USD");
        product.AddVariant(variant);
        _dbContext.Set<Variant>().Add(variant);

        var loc = StockLocation.Create("Warehouse").Value;
        loc.Id = locationId;
        // Only 2 in stock, but order needs 5. Backorderable = true
        var si = StockItem.Create(variant.Id, loc.Id, "V1-W", 2, backorderable: true).Value;
        loc.StockItems.Add(si);
        _dbContext.Set<StockLocation>().Add(loc);
        _dbContext.Set<StockItem>().Add(si);

        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var order = Order.Create(storeId, "USD").Value;
        order.AddLineItem(variant, 5);
        _dbContext.Set<Order>().Add(order);

        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var planner = new FulfillmentPlanner(_factory, _dbContext);

        // Act
        var result = await planner.PlanFulfillment(order, "HighestStock", TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : "");
        result.Value.Shipments.Should().NotBeEmpty();
        result.Value.Shipments[0].Items[0].IsBackordered.Should().BeTrue();
    }
}