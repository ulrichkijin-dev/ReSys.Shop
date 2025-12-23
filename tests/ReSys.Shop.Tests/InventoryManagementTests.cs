using FluentAssertions;


using Microsoft.EntityFrameworkCore;


using ReSys.Shop.Core.Domain.Catalog.Products;
using ReSys.Shop.Core.Domain.Catalog.Products.Variants;
using ReSys.Shop.Core.Domain.Inventories.Stocks;
using ReSys.Shop.Infrastructure.Persistence.Contexts;

namespace ReSys.Shop.Tests;

public class InventoryManagementTests
{
    private readonly ApplicationDbContext _dbContext;

    public InventoryManagementTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
    }

    private async Task<StockItem> CreateStockItem(int onHand, bool backorderable = true, int? maxBackorder = null)
    {
        var product = Product.Create(name: "Test", slug: "test").Value;
        _dbContext.Set<Product>().Add(product);
        var variant = Variant.Create(productId: product.Id, isMaster: false, sku: "SKU-" + Guid.NewGuid()).Value;
        _dbContext.Set<Variant>().Add(variant);
        
        var locationId = Guid.NewGuid();
        var stockItem = StockItem.Create(variant.Id, locationId, variant.Sku ?? "UNKNOWN", onHand, backorderable: backorderable, maxBackorderQuantity: maxBackorder).Value;
        _dbContext.Set<StockItem>().Add(stockItem);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return stockItem;
    }

    [Fact]
    public async Task Reserve_WhenNoBackorder_ShouldFailIfInsufficient()
    {
        // Arrange
        var stockItem = await CreateStockItem(10, backorderable: false);

        // Act
        var result = stockItem.Reserve(15, Guid.NewGuid());

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("StockItem.InsufficientStock");
    }

    [Fact]
    public async Task Reserve_WhenBackorderable_ShouldSucceedEvenIfInsufficient()
    {
        // Arrange
        var stockItem = await CreateStockItem(10, backorderable: true);

        // Act
        var result = stockItem.Reserve(15, Guid.NewGuid());

        // Assert
        result.IsError.Should().BeFalse();
        stockItem.QuantityReserved.Should().Be(15);
        stockItem.CountAvailable.Should().Be(0);
        stockItem.CurrentBackorderQuantity.Should().Be(5);
    }

    [Fact]
    public async Task Reserve_ShouldRespectBackorderLimit()
    {
        // Arrange
        var stockItem = await CreateStockItem(0, backorderable: true, maxBackorder: 10);

        // Act
        var result = stockItem.Reserve(11, Guid.NewGuid());

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("StockItem.BackorderLimitExceeded");
    }

    [Fact]
    public async Task Adjust_ShouldImpactAvailableQuantity()
    {
        // Arrange
        var stockItem = await CreateStockItem(10);
        stockItem.Reserve(5, Guid.NewGuid()); // Available = 5

        // Act
        stockItem.Adjust(10, ReSys.Shop.Core.Domain.Inventories.Movements.StockMovement.MovementOriginator.Adjustment);

        // Assert
        stockItem.QuantityOnHand.Should().Be(20);
        stockItem.CountAvailable.Should().Be(15);
    }

    [Fact]
    public async Task Release_ShouldIncreaseAvailableQuantity()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var stockItem = await CreateStockItem(10);
        stockItem.Reserve(5, orderId); // Available = 5, Reserved = 5

        // Act
        var result = stockItem.Release(3, orderId);

        // Assert
        result.IsError.Should().BeFalse();
        stockItem.QuantityReserved.Should().Be(2);
        stockItem.CountAvailable.Should().Be(8);
    }

    [Fact]
    public async Task ConfirmShipment_ShouldDecreaseOnHandAndReserved()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var shipmentId = Guid.NewGuid();
        var stockItem = await CreateStockItem(10);
        stockItem.Reserve(5, orderId); // OnHand = 10, Reserved = 5, Available = 5

        // Act
        var result = stockItem.ConfirmShipment(5, shipmentId, orderId);

        // Assert
        result.IsError.Should().BeFalse();
        stockItem.QuantityOnHand.Should().Be(5);
        stockItem.QuantityReserved.Should().Be(0);
        stockItem.CountAvailable.Should().Be(5);
    }
}