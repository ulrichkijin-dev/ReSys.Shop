using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using ReSys.Shop.Core.Domain.Catalog.Products;
using ReSys.Shop.Core.Domain.Catalog.Products.Variants;
using ReSys.Shop.Core.Domain.Inventories.Stocks;
using ReSys.Shop.Core.Domain.Orders;
using ReSys.Shop.Core.Domain.Orders.Shipments;
using ReSys.Shop.Core.Feature.Admin.Inventories.StockItems;
using ReSys.Shop.Core.Feature.Admin.Orders;
using ReSys.Shop.Infrastructure.Persistence.Contexts;

namespace ReSys.Shop.Tests;

public class InventoryTests
{
    private readonly ApplicationDbContext _dbContext;

    public InventoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
    }

    private async Task<StockItem> CreateStockItem(Guid variantId, Guid locationId, int quantity)
    {
        var stockItem = StockItem.Create(variantId, locationId, "SKU-" + Guid.NewGuid(), quantity).Value;
        _dbContext.Set<StockItem>().Add(stockItem);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return stockItem;
    }

    private void SetVariantPrice(Variant variant, decimal amount, string currency = "USD")
    {
        variant.SetPrice(amount, null, currency).IsError.Should().BeFalse();
    }

    [Fact]
    public async Task AddItemToShipment_ShouldReserveStock()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        
        var product = Product.Create(name: "Test Product", slug: "test-product").Value;
        _dbContext.Set<Product>().Add(product);
        
        var variant = Variant.Create(productId: product.Id, isMaster: false, sku: "SKU-1").Value;
        SetVariantPrice(variant, 100m, "USD");
        _dbContext.Set<Variant>().Add(variant);

        var stockItem = await CreateStockItem(variant.Id, locationId, 100);

        var order = Order.Create(storeId, "USD").Value;
        order.AddLineItem(variant, 5).IsError.Should().BeFalse();
        order.State = Order.OrderState.Delivery;
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var lineItem = order.LineItems.First();
        var shipmentResult = Shipment.Create(order.Id, locationId);
        shipmentResult.IsError.Should().BeFalse();
        var shipment = shipmentResult.Value;
        
        var unitResult = InventoryUnit.Create(lineItem.VariantId, lineItem.Id, shipment.Id, InventoryUnit.InventoryUnitState.OnHand);
        unitResult.IsError.Should().BeFalse();
        shipment.InventoryUnits.Add(unitResult.Value);
        lineItem.InventoryUnits.Add(unitResult.Value);
        
        order.AddShipment(shipment).IsError.Should().BeFalse();
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        var handler = new StockReservations.OnShipmentItemUpdated(_dbContext);

        // Act - Add item to shipment
        var unitResult2 = InventoryUnit.Create(lineItem.VariantId, lineItem.Id, shipment.Id, InventoryUnit.InventoryUnitState.OnHand);
        unitResult2.IsError.Should().BeFalse();
        shipment.InventoryUnits.Add(unitResult2.Value);
        lineItem.InventoryUnits.Add(unitResult2.Value);
        
        var unitResult3 = InventoryUnit.Create(lineItem.VariantId, lineItem.Id, shipment.Id, InventoryUnit.InventoryUnitState.OnHand);
        unitResult3.IsError.Should().BeFalse();
        shipment.InventoryUnits.Add(unitResult3.Value);
        lineItem.InventoryUnits.Add(unitResult3.Value);
        
        order.AddDomainEvent(new Order.Events.ShipmentItemUpdated(order.Id, shipment.Id, lineItem.VariantId));
        
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Simulate Event Dispatch
        var evt = order.DomainEvents.OfType<Order.Events.ShipmentItemUpdated>().Last();
        await handler.Handle(evt, TestContext.Current.CancellationToken);

        // Assert
        var updatedStock = await _dbContext.Set<StockItem>().FirstAsync(s => s.Id == stockItem.Id, TestContext.Current.CancellationToken);
        updatedStock.QuantityReserved.Should().Be(3);
    }
    
    [Fact]
    public async Task TransferToLocation_ShouldNotDuplicateInventory()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var sourceLocId = Guid.NewGuid();
        var targetLocId = Guid.NewGuid();
        
        var product = Product.Create(name: "Test Product 2", slug: "test-product-2").Value;
        _dbContext.Set<Product>().Add(product);
        var variant = Variant.Create(productId: product.Id, isMaster: false, sku: "SKU-2").Value;
        SetVariantPrice(variant, 100m, "USD");
        _dbContext.Set<Variant>().Add(variant);

        var stockSource = await CreateStockItem(variant.Id, sourceLocId, 100);
        var stockTarget = await CreateStockItem(variant.Id, targetLocId, 100);

        var order = Order.Create(storeId, "USD").Value;
        order.AddLineItem(variant, 5).IsError.Should().BeFalse();
        order.State = Order.OrderState.Delivery;
        _dbContext.Set<Order>().Add(order);

        var lineItem = order.LineItems.First();
        var shipmentResult = Shipment.Create(order.Id, sourceLocId);
        shipmentResult.IsError.Should().BeFalse();
        var shipment = shipmentResult.Value;
        
        for (int i = 0; i < 3; i++)
        {
            var unitResult = InventoryUnit.Create(lineItem.VariantId, lineItem.Id, shipment.Id, InventoryUnit.InventoryUnitState.OnHand);
            unitResult.IsError.Should().BeFalse();
            shipment.InventoryUnits.Add(unitResult.Value);
            lineItem.InventoryUnits.Add(unitResult.Value);
        }
        order.AddShipment(shipment).IsError.Should().BeFalse();
        
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        stockSource.Reserve(3, order.Id).IsError.Should().BeFalse();
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var transferHandler = new OrderModule.Shipments.TransferToLocation.CommandHandler(_dbContext);
        
        // Act - Transfer 2 units to target location
        var request = new OrderModule.Shipments.TransferToLocation.Request(targetLocId, variant.Id, 2);
        var command = new OrderModule.Shipments.TransferToLocation.Command(order.Id, shipment.Id, request);
        
        var handleResult = await transferHandler.Handle(command, TestContext.Current.CancellationToken);
        handleResult.IsError.Should().BeFalse("Transfer handler failed: " + (handleResult.IsError ? handleResult.FirstError.Description : ""));

        // Assert
        var sourceShipment = await _dbContext.Set<Shipment>().Include(s => s.InventoryUnits).FirstAsync(s => s.Id == shipment.Id, TestContext.Current.CancellationToken);
        sourceShipment.InventoryUnits.Count.Should().Be(1);

        var targetShipment = await _dbContext.Set<Shipment>().Include(s => s.InventoryUnits).FirstAsync(s => s.StockLocationId == targetLocId, TestContext.Current.CancellationToken);
        targetShipment.InventoryUnits.Count.Should().Be(2);

        var totalUnits = await _dbContext.Set<InventoryUnit>().CountAsync(u => u.LineItemId == lineItem.Id, TestContext.Current.CancellationToken);
        totalUnits.Should().Be(3);
    }
    
     [Fact]
    public async Task Ship_ShouldConfirmStock()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        
        var product = Product.Create(name: "Test Product 3", slug: "test-product-3").Value;
        _dbContext.Set<Product>().Add(product);
        var variant = Variant.Create(productId: product.Id, isMaster: false, sku: "SKU-3").Value;
        SetVariantPrice(variant, 100m, "USD");
        _dbContext.Set<Variant>().Add(variant);

        var stockItem = await CreateStockItem(variant.Id, locationId, 100);

        var order = Order.Create(storeId, "USD").Value;
        order.AddLineItem(variant, 5).IsError.Should().BeFalse();
        order.State = Order.OrderState.Delivery;
        _dbContext.Set<Order>().Add(order);
        
        var lineItem = order.LineItems.First();
        var shipmentResult = Shipment.Create(order.Id, locationId);
        shipmentResult.IsError.Should().BeFalse();
        var shipment = shipmentResult.Value;
        
        for (int i = 0; i < 5; i++)
        {
            var unitResult = InventoryUnit.Create(lineItem.VariantId, lineItem.Id, shipment.Id, InventoryUnit.InventoryUnitState.OnHand);
            unitResult.IsError.Should().BeFalse();
            shipment.InventoryUnits.Add(unitResult.Value);
            lineItem.InventoryUnits.Add(unitResult.Value);
        }
        order.AddShipment(shipment).IsError.Should().BeFalse();
        
        stockItem.Reserve(5, order.Id).IsError.Should().BeFalse();
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var shipHandler = new StockConfirmation.OnShipmentShipped(_dbContext);

        // Act
        var shipResult = shipment.Ship("TRACK-123");
        shipResult.IsError.Should().BeFalse();
        
        // Dispatch Event
        var evt = shipment.DomainEvents.OfType<Shipment.Events.Shipped>().First();
        await shipHandler.Handle(evt, TestContext.Current.CancellationToken);

        // Assert
        var updatedStock = await _dbContext.Set<StockItem>().FirstAsync(s => s.Id == stockItem.Id, TestContext.Current.CancellationToken);
        updatedStock.QuantityOnHand.Should().Be(95);
        updatedStock.QuantityReserved.Should().Be(0);
    }
}