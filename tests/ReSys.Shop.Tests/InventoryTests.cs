using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using ReSys.Shop.Core.Common.Domain.Events;
using ReSys.Shop.Core.Domain.Catalog.Products;
using ReSys.Shop.Core.Domain.Catalog.Products.Variants;
using ReSys.Shop.Core.Domain.Inventories.FulfillmentStrategies;
using ReSys.Shop.Core.Domain.Inventories.Locations;
using ReSys.Shop.Core.Domain.Inventories.Stocks;
using ReSys.Shop.Core.Domain.Orders;
using ReSys.Shop.Core.Domain.Orders.Shipments;
using ReSys.Shop.Core.Feature.Admin.Inventories.StockItems;
using ReSys.Shop.Core.Feature.Admin.Orders;
using ReSys.Shop.Infrastructure.Persistence.Contexts;
using Xunit;

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

        var lineItemId = order.LineItems.First().Id;
        var shipmentResult = order.AddShipment(locationId, new List<FulfillmentItem> { 
            FulfillmentItem.Create(lineItemId, variant.Id, 1).Value
        });
        shipmentResult.IsError.Should().BeFalse("AddShipment failed: " + (shipmentResult.IsError ? shipmentResult.FirstError.Description : ""));
        var shipment = shipmentResult.Value;
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        var handler = new StockReservations.OnShipmentItemUpdated(_dbContext);

        // Act - Add item to shipment
        var addResult = order.AddItemToShipment(shipment.Id, variant, 2);
        addResult.IsError.Should().BeFalse("AddItemToShipment failed: " + (addResult.IsError ? addResult.FirstError.Description : ""));
        
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

        var lineItemId = order.LineItems.First().Id;
        var shipmentResult = order.AddShipment(sourceLocId, new List<FulfillmentItem> {
            FulfillmentItem.Create(lineItemId, variant.Id, 3).Value
        });
        shipmentResult.IsError.Should().BeFalse();
        var shipment = shipmentResult.Value;
        
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

        var totalUnits = await _dbContext.Set<InventoryUnit>().CountAsync(u => u.LineItemId == lineItemId, TestContext.Current.CancellationToken);
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
        
        var lineItemId = order.LineItems.First().Id;
        var shipmentResult = order.AddShipment(locationId, new List<FulfillmentItem> {
             FulfillmentItem.Create(lineItemId, variant.Id, 5).Value
        });
        shipmentResult.IsError.Should().BeFalse();
        var shipment = shipmentResult.Value;
        
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