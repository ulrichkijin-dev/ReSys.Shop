using FluentAssertions;


using Microsoft.EntityFrameworkCore;


using ReSys.Shop.Core.Domain.Catalog.Products;
using ReSys.Shop.Core.Domain.Catalog.Products.Variants;
using ReSys.Shop.Core.Domain.Identity.UserAddresses;
using ReSys.Shop.Core.Domain.Orders;
using ReSys.Shop.Core.Domain.Orders.Payments;
using ReSys.Shop.Core.Domain.Orders.Shipments;
using ReSys.Shop.Core.Domain.Settings.ShippingMethods;
using ReSys.Shop.Infrastructure.Persistence.Contexts;

namespace ReSys.Shop.Tests;

public class OrderTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Guid _countryId = Guid.NewGuid();

    public OrderTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        
        // Setup initial data
        var country = ReSys.Shop.Core.Domain.Location.Countries.Country.Create("United States", "US", "USA").Value;
        country.Id = _countryId;
        _dbContext.Set<ReSys.Shop.Core.Domain.Location.Countries.Country>().Add(country);
        _dbContext.SaveChanges();
    }

    private async Task<Variant> CreateVariant(string sku, decimal price = 100m, bool trackInventory = false, bool isDigital = false)
    {
        var product = Product.Create(name: "Product " + sku, slug: "product-" + sku.ToLower(), isDigital: isDigital).Value;
        product.Activate();
        _dbContext.Set<Product>().Add(product);
        
        var variant = Variant.Create(productId: product.Id, isMaster: false, sku: sku, trackInventory: trackInventory).Value;
        variant.Product = product; // Explicitly set for tests
        variant.SetPrice(price, null, "USD").IsError.Should().BeFalse();
        product.AddVariant(variant);
        
        _dbContext.Set<Variant>().Add(variant);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return variant;
    }

    private UserAddress CreateAddress()
    {
        return UserAddress.Create(
            firstName: "John",
            lastName: "Doe",
            userId: Guid.NewGuid().ToString(),
            countryId: _countryId,
            address1: "123 Main St",
            city: "New York",
            zipcode: "10001").Value;
    }

    [Fact]
    public async Task Order_ModificationAfterTerminalState_ShouldFail()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var v1 = await CreateVariant("V1", 100m, isDigital: false);
        var order = Order.Create(storeId, "USD").Value;
        order.AddLineItem(v1, 1).IsError.Should().BeFalse();
        
        foreach(var li in order.LineItems) li.Variant = v1;

        // Progress to Complete state
        order.SetShippingAddress(CreateAddress());
        order.SetBillingAddress(CreateAddress());
        order.Next(); // To Address
        
        var shippingMethod = ShippingMethod.Create("Standard", "std", ShippingMethod.ShippingType.Standard, 0).Value;
        order.SetShippingMethod(shippingMethod);
        order.Next(); // To Delivery
        
        // Move to Payment state
        order.Next(); 
        
        var payment = Payment.Create(order.Id, 10000, "USD", "CreditCard", Guid.NewGuid()).Value;
        payment.MarkAsCaptured("T1");
        order.AddPayment(payment);
        
        order.Next(); // To Confirm
        
        // Add Shipment for fulfillment validation
        var shipment = Shipment.Create(order.Id, Guid.NewGuid()).Value;
        shipment.Order = order;
        var unit = InventoryUnit.Create(v1.Id, order.LineItems.First().Id, shipment.Id).Value;
        unit.Shipment = shipment;
        order.LineItems.First().InventoryUnits.Add(unit);
        shipment.InventoryUnits.Add(unit);
        order.AddShipment(shipment);
        shipment.Ready();

        order.Next(); // To Complete
        order.State.Should().Be(Order.OrderState.Complete);

        // Act & Assert
        order.UpdateLineItemQuantity(order.LineItems.First().Id, 5).IsError.Should().BeTrue();
        order.UpdateLineItemQuantity(order.LineItems.First().Id, 5).FirstError.Code.Should().Be("Order.CannotModifyInTerminalState");

        order.RemoveLineItem(order.LineItems.First().Id).IsError.Should().BeTrue();
        order.RemoveLineItem(order.LineItems.First().Id).FirstError.Code.Should().Be("Order.CannotModifyInTerminalState");
    }

    [Fact]
    public async Task Order_CancelWithShippedItems_ShouldFail()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var variant = await CreateVariant("PHYS-1", 100m);
        var order = Order.Create(storeId, "USD").Value;
        order.AddLineItem(variant, 1).IsError.Should().BeFalse();
        
        var shippingMethod = ShippingMethod.Create("Standard", "std", ShippingMethod.ShippingType.Standard, 1000).Value;
        
        // Progress to Delivery state
        order.SetShippingAddress(CreateAddress());
        order.SetBillingAddress(CreateAddress());
        order.Next(); // To Address
        order.SetShippingMethod(shippingMethod);
        order.Next(); // To Delivery
        
        var shipment = Shipment.Create(order.Id, Guid.NewGuid()).Value;
        shipment.Order = order; // Ensure navigation property is set
        var firstLineItem = order.LineItems.First();
        var unit = InventoryUnit.Create(variant.Id, firstLineItem.Id, shipment.Id).Value;
        unit.Shipment = shipment;
        firstLineItem.InventoryUnits.Add(unit); // Add to line item for allocation check
        shipment.InventoryUnits.Add(unit);
        order.AddShipment(shipment).IsError.Should().BeFalse();

        // Mark as shipped
        shipment.Ship("TRACK-123").IsError.Should().BeFalse();

        // Act
        var result = order.Cancel();

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Order.CannotCancelWithShippedItems");
    }

    [Fact]
    public async Task Order_UpdateQuantity_ShouldReconcileInventoryUnits()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var variant = await CreateVariant("PHYS-1", 100m);
        var order = Order.Create(storeId, "USD").Value;
        order.AddLineItem(variant, 5).IsError.Should().BeFalse();
        
        // Move to Delivery state to allow shipments
        order.SetShippingAddress(CreateAddress());
        order.SetBillingAddress(CreateAddress());
        order.Next(); // To Address
        order.Next(); // To Delivery
        
        var lineItem = order.LineItems.First();
        var shipment = Shipment.Create(order.Id, Guid.NewGuid()).Value;
        shipment.Order = order;
        
        for (int i = 0; i < 5; i++)
        {
            var unit = InventoryUnit.Create(variant.Id, lineItem.Id, shipment.Id).Value;
            unit.Shipment = shipment; // Set for reconciliation logic
            lineItem.InventoryUnits.Add(unit);
            shipment.InventoryUnits.Add(unit);
        }
        order.AddShipment(shipment).IsError.Should().BeFalse();

        // Act
        // Decrease quantity from 5 to 3
        order.UpdateLineItemQuantity(lineItem.Id, 3).IsError.Should().BeFalse();

        // Assert
        lineItem.Quantity.Should().Be(3);
        lineItem.InventoryUnits.Should().HaveCount(3);
        shipment.InventoryUnits.Should().HaveCount(3);
    }

    [Fact]
    public void Payment_RefundExceedsBalance_ShouldFail()
    {
        // Arrange
        var payment = Payment.Create(Guid.NewGuid(), 10000, "USD", "CreditCard", Guid.NewGuid()).Value; // 100 USD
        payment.MarkAsCaptured("TRANS-1").IsError.Should().BeFalse();

        // Act
        // Refund 60 USD
        payment.Refund(6000, "Partial refund").IsError.Should().BeFalse();
        payment.RefundedAmountCents.Should().Be(6000);
        payment.State.Should().Be(Payment.PaymentState.Completed);

        // Try to refund another 50 USD (Total 110 > 100)
        var result = payment.Refund(5000, "Too much");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Payment.RefundExceedsBalance");
    }

    [Fact]
    public async Task Order_AddressModificationAfterFinalized_ShouldFail()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var variant = await CreateVariant("V1");
        var order = Order.Create(storeId, "USD").Value;
        order.AddLineItem(variant, 1);
        
        order.SetShippingAddress(CreateAddress());
        order.SetBillingAddress(CreateAddress());
        
        // Progress past Address state
        order.Next(); // To Address
        order.Next(); // To Delivery
        order.State.Should().Be(Order.OrderState.Delivery);

        // Act & Assert
        order.SetShippingAddress(CreateAddress()).IsError.Should().BeTrue();
        order.SetShippingAddress(CreateAddress()).FirstError.Code.Should().Be("Order.CannotModifyAddress");
    }

    [Fact]
    public async Task Order_ToConfirm_ShouldOnlyCountValidPayments()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var variant = await CreateVariant("V1", 100m);
        var order = Order.Create(storeId, "USD").Value;
        order.AddLineItem(variant, 1).IsError.Should().BeFalse(); // 100 USD
        
        foreach(var li in order.LineItems) li.Variant = variant;

        order.SetShippingAddress(CreateAddress());
        order.SetBillingAddress(CreateAddress());
        order.Next(); // To Address
        
        var shippingMethod = ShippingMethod.Create("Free", "free", ShippingMethod.ShippingType.Standard, 0).Value;
        order.SetShippingMethod(shippingMethod);
        order.Next(); // To Delivery
        order.Next(); // To Payment

        // Add a Failed payment
        var p1 = Payment.Create(order.Id, 10000, "USD", "CreditCard", Guid.NewGuid()).Value;
        p1.MarkAsFailed("Declined");
        order.AddPayment(p1);

        // Act
        var result = order.Next(); // To Confirm

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Order.InsufficientPayment");
    }

    [Fact]
    public void Order_AssignToUser_AlreadyAssigned_ShouldFail()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var userId1 = "user-1";
        var userId2 = "user-2";
        var order = Order.Create(storeId, "USD", userId: userId1).Value;

        // Act
        var result = order.AssignToUser(userId2);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Order.AlreadyAssigned");
    }

    [Fact]
    public async Task Order_Complete_IncompleteFulfillment_ShouldFail()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var variant = await CreateVariant("V1", 100m);
        var order = Order.Create(storeId, "USD").Value;
        order.AddLineItem(variant, 2).IsError.Should().BeFalse(); // Qty 2 -> 200 USD (20000 cents)
        
        var lineItem = order.LineItems.First();
        lineItem.Variant = variant; // Ensure linked for IsFullyDigital
        
        order.SetShippingAddress(CreateAddress());
        order.SetBillingAddress(CreateAddress());
        order.Next(); // To Address
        
        var shippingMethod = ShippingMethod.Create("Standard", "std", ShippingMethod.ShippingType.Standard, 0).Value;
        order.SetShippingMethod(shippingMethod);
        order.Next(); // To Delivery
        
        // Add Shipment with only 1 unit (Incomplete)
        var shipment = Shipment.Create(order.Id, Guid.NewGuid()).Value;
        shipment.Order = order; // Ensure navigation property is set
        var unit = InventoryUnit.Create(variant.Id, lineItem.Id, shipment.Id).Value;
        unit.Shipment = shipment;
        shipment.InventoryUnits.Add(unit);
        order.AddShipment(shipment).IsError.Should().BeFalse();
        
        // Mark shipment as ready
        shipment.Ready();

        // Add Full Payment (200 USD)
        var payment = Payment.Create(order.Id, 20000, "USD", "CreditCard", Guid.NewGuid()).Value;
        payment.MarkAsCaptured("T1");
        order.AddPayment(payment);
        
        order.Next(); // Delivery -> Payment
        order.Next(); // Payment -> Confirm

        // Act
        var result = order.Next(); // Confirm -> Complete

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Order.IncompleteInventoryAllocation");
    }

    [Fact]
    public async Task Order_ShippingMethodModificationAfterDelivery_ShouldFail()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var variant = await CreateVariant("V1");
        var order = Order.Create(storeId, "USD").Value;
        order.AddLineItem(variant, 1);
        
        order.SetShippingAddress(CreateAddress());
        order.SetBillingAddress(CreateAddress());
        order.Next(); // To Address
        
        var shippingMethod = ShippingMethod.Create("Standard", "std", ShippingMethod.ShippingType.Standard, 0).Value;
        order.SetShippingMethod(shippingMethod);
        order.Next(); // To Delivery
        
        // Move to Payment state
        order.Next(); 
        order.State.Should().Be(Order.OrderState.Payment);

        // Act & Assert
        order.SetShippingMethod(shippingMethod).IsError.Should().BeTrue();
        order.SetShippingMethod(shippingMethod).FirstError.Code.Should().Be("Order.CannotModifyShipping");
    }

    [Fact]
    public async Task Order_ApplyPromotionAfterCart_ShouldFail()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var variant = await CreateVariant("V1");
        var order = Order.Create(storeId, "USD").Value;
        order.AddLineItem(variant, 1);
        
        // Progress past Cart
        order.Next(); 
        order.State.Should().Be(Order.OrderState.Address);

        // Act
        var promotion = ReSys.Shop.Core.Domain.Promotions.Promotions.Promotion.Create("Test", null!).Value;
        var result = order.ApplyPromotion(promotion);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Order.CannotModifyAfterCart");
    }

    [Fact]
    public async Task Shipment_OperationsOnCanceledOrder_ShouldFail()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var variant = await CreateVariant("V1");
        var order = Order.Create(storeId, "USD").Value;
        order.AddLineItem(variant, 1);
        
        var shipment = Shipment.Create(order.Id, Guid.NewGuid()).Value;
        shipment.Order = order; // Ensure navigation property is set for the guard
        order.AddShipment(shipment);
        
        // Cancel Order
        order.Cancel();

        // Act & Assert
        shipment.AllocateInventory().IsError.Should().BeTrue();
        shipment.Ready().IsError.Should().BeTrue();
        shipment.Ship().IsError.Should().BeTrue();
        
        shipment.Ship().FirstError.Code.Should().Be("Shipment.OrderCanceled");
    }

    [Fact]
    public async Task Order_Complete_GranularIncompleteFulfillment_ShouldFail()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var v1 = await CreateVariant("V1", 50m);
        var v2 = await CreateVariant("V2", 50m);
        var order = Order.Create(storeId, "USD").Value;
        order.AddLineItem(v1, 1).IsError.Should().BeFalse();
        order.AddLineItem(v2, 1).IsError.Should().BeFalse();
        
        foreach(var li in order.LineItems)
        {
            if (li.VariantId == v1.Id) li.Variant = v1;
            if (li.VariantId == v2.Id) li.Variant = v2;
        }
        
        order.SetShippingAddress(CreateAddress());
        order.SetBillingAddress(CreateAddress());
        order.Next().IsError.Should().BeFalse(); // To Address
        
        var shippingMethod = ShippingMethod.Create("Standard", "std", ShippingMethod.ShippingType.Standard, 0).Value;
        order.SetShippingMethod(shippingMethod);
        order.Next().IsError.Should().BeFalse(); // To Delivery
        
        // Add Shipment for V1 ONLY
        var shipment = Shipment.Create(order.Id, Guid.NewGuid()).Value;
        shipment.Order = order; // Ensure navigation property is set
        var li1 = order.LineItems.First(li => li.VariantId == v1.Id);
        var unit1 = InventoryUnit.Create(v1.Id, li1.Id, shipment.Id).Value;
        unit1.Shipment = shipment;
        li1.InventoryUnits.Add(unit1);
        shipment.InventoryUnits.Add(unit1);

        // Add Shipment for V2 BUT DON'T ADD UNIT (Incomplete)
        var shipment2 = Shipment.Create(order.Id, Guid.NewGuid()).Value;
        shipment2.Order = order;
        // li2 has NO units allocated
        
        order.AddShipment(shipment).IsError.Should().BeFalse();
        order.AddShipment(shipment2).IsError.Should().BeFalse();
        
        shipment.Ready();
        shipment2.Ready();

        // Add Full Payment (100 USD)
        var payment = Payment.Create(order.Id, 10000, "USD", "CreditCard", Guid.NewGuid()).Value;
        payment.MarkAsCaptured("T1");
        order.AddPayment(payment);
        
        order.Next().IsError.Should().BeFalse(); // To Payment
        order.Next().IsError.Should().BeFalse(); // To Confirm

        // Act
        var result = order.Next(); // Confirm -> Complete

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Order.IncompleteInventoryAllocation");
    }

    [Fact]
    public async Task Order_RemoveLineItem_ShouldClearInventoryUnits()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var variant = await CreateVariant("PHYS-1", 100m);
        var order = Order.Create(storeId, "USD").Value;
        order.AddLineItem(variant, 2).IsError.Should().BeFalse();
        
        var lineItem = order.LineItems.First();
        var shipment = Shipment.Create(order.Id, Guid.NewGuid()).Value;
        shipment.Order = order;
        
        var unit = InventoryUnit.Create(variant.Id, lineItem.Id, shipment.Id).Value;
        unit.Shipment = shipment; // Set for clearing logic
        lineItem.InventoryUnits.Add(unit);
        shipment.InventoryUnits.Add(unit);
        order.AddShipment(shipment);

        // Act
        order.RemoveLineItem(lineItem.Id).IsError.Should().BeFalse();

        // Assert
        order.LineItems.Should().BeEmpty();
        shipment.InventoryUnits.Should().BeEmpty();
    }

    [Fact]
    public void Order_AddPayment_CurrencyMismatch_ShouldFail()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), "USD").Value;
        var payment = Payment.Create(order.Id, 1000, "EUR", "CreditCard", Guid.NewGuid()).Value; // EUR instead of USD

        // Act
        var result = order.AddPayment(payment);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Order.CurrencyMismatch");
    }

    [Fact]
    public async Task Shipment_ShipWithBackorders_ShouldFail()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var variant = await CreateVariant("PHYS-1", 100m);
        var order = Order.Create(storeId, "USD").Value;
        order.AddLineItem(variant, 1);
        
        var shipment = Shipment.Create(order.Id, Guid.NewGuid()).Value;
        // Create unit in BACKORDERED state
        var unit = InventoryUnit.Create(variant.Id, order.LineItems.First().Id, shipment.Id, InventoryUnit.InventoryUnitState.Backordered).Value;
        shipment.InventoryUnits.Add(unit);
        order.AddShipment(shipment);

        // Act
        var result = shipment.Ship("TRACK-1");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Shipment.CannotShipWithBackorders");
    }
}