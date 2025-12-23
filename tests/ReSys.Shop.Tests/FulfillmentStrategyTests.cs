using FluentAssertions;


using Microsoft.EntityFrameworkCore;


using ReSys.Shop.Core.Domain.Catalog.Products;
using ReSys.Shop.Core.Domain.Catalog.Products.Variants;
using ReSys.Shop.Core.Domain.Inventories.FulfillmentStrategies;
using ReSys.Shop.Core.Domain.Inventories.Locations;
using ReSys.Shop.Core.Domain.Inventories.Stocks;
using ReSys.Shop.Infrastructure.Persistence.Contexts;

namespace ReSys.Shop.Tests;

public class FulfillmentStrategyTests
{
    private readonly ApplicationDbContext _dbContext;

    public FulfillmentStrategyTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
    }

    private async Task<(Variant, List<StockLocation>)> SetupScenario(int loc1Stock, int loc2Stock)
    {
        var product = Product.Create("Test", "test").Value;
        product.Activate();
        _dbContext.Set<Product>().Add(product);

        var variant = Variant.Create(product.Id, sku: "V1").Value;
        variant.SetPrice(100, null, "USD");
        product.AddVariant(variant);
        _dbContext.Set<Variant>().Add(variant);

        var loc1 = StockLocation.Create("Loc 1").Value;
        var si1 = StockItem.Create(variant.Id, loc1.Id, "V1-L1", loc1Stock).Value;
        loc1.StockItems.Add(si1);
        _dbContext.Set<StockLocation>().Add(loc1);

        var loc2 = StockLocation.Create("Loc 2").Value;
        var si2 = StockItem.Create(variant.Id, loc2.Id, "V1-L2", loc2Stock).Value;
        loc2.StockItems.Add(si2);
        _dbContext.Set<StockLocation>().Add(loc2);

        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return (variant, new List<StockLocation> { loc1, loc2 });
    }

    [Fact]
    public async Task NearestLocationStrategy_ShouldSelectClosest()
    {
        // Arrange
        var (variant, locations) = await SetupScenario(10, 10);
        locations[0].Latitude = 10; locations[0].Longitude = 10; // Closer to (11, 11)
        locations[1].Latitude = 50; locations[1].Longitude = 50;
        
        var strategy = new NearestLocationStrategy();

        // Act
        var result = strategy.SelectLocation(variant, 5, locations, 11, 11);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(locations[0].Id);
    }

    [Fact]
    public async Task HighestStockStrategy_ShouldSelectMostStock()
    {
        // Arrange
        var (variant, locations) = await SetupScenario(10, 50);
        var strategy = new HighestStockStrategy();

        // Act
        var result = strategy.SelectLocation(variant, 5, locations);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(locations[1].Id);
    }

    [Fact]
    public async Task CostOptimizedStrategy_ShouldSelectCheapest()
    {
        // Arrange
        var (variant, locations) = await SetupScenario(10, 10);
        
        // Loc 1: High base cost
        locations[0].PrivateMetadata = new Dictionary<string, object?> { ["fulfillment_cost_base"] = 20m };
        // Loc 2: Low base cost
        locations[1].PrivateMetadata = new Dictionary<string, object?> { ["fulfillment_cost_base"] = 2m };
        
        var strategy = new CostOptimizedStrategy();

        // Act
        var result = strategy.SelectLocation(variant, 1, locations);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(locations[1].Id);
    }

    [Fact]
    public async Task PreferredLocationStrategy_ShouldSelectPreferred()
    {
        // Arrange
        var (variant, locations) = await SetupScenario(10, 10);
        
        // Loc 2 is preferred
        locations[1].PublicMetadata = new Dictionary<string, object?> { ["fulfillment_preference_priority"] = 100 };
        
        var strategy = new PreferredLocationStrategy();

        // Act
        var result = strategy.SelectLocation(variant, 5, locations);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(locations[1].Id);
    }

    [Fact]
    public async Task HighestStockStrategy_SelectMultiple_ShouldSplitStock()
    {
        // Arrange
        var (variant, locations) = await SetupScenario(10, 10); // Need 15 total
        var strategy = new HighestStockStrategy();

        // Act
        var result = strategy.SelectMultipleLocations(variant, 15, locations);

        // Assert
        result.Count.Should().Be(2);
        result.Sum(r => r.Quantity).Should().Be(15);
    }
}