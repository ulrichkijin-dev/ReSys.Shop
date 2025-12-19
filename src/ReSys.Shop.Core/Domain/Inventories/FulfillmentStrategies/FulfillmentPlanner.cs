using ReSys.Shop.Core.Domain.Catalog.Products.Variants;
using ReSys.Shop.Core.Domain.Orders;

namespace ReSys.Shop.Core.Domain.Inventories.FulfillmentStrategies;

public class FulfillmentPlanner(FulfillmentStrategyFactory strategyFactory, IApplicationDbContext applicationDbContext)
    : IFulfillmentPlanner
{
    public async Task<ErrorOr<FulfillmentPlanResult>> PlanFulfillment(Order order, string strategyType,
        CancellationToken cancellationToken = default)
    {
        if (order is null) return Error.NotFound(code: "Order.NotFound", description: "Order cannot be null.");
        if (!order.LineItems.Any()) return Error.Validation(code: "Order.Empty", description: "Order has no line items to fulfill.");

        if (!Enum.TryParse(value: strategyType, ignoreCase: true, result: out FulfillmentStrategyType parsedStrategyType))
        {
            parsedStrategyType = FulfillmentStrategyType.HighestStock;
        }

        var strategy = strategyFactory.GetStrategy(strategyType: parsedStrategyType);

        var fulfillmentShipmentPlans = new List<FulfillmentShipmentPlan>();
        var allLineItems = order.LineItems.ToList();
        var fulfilledLineItemIds = new HashSet<Guid>();

        foreach (var lineItem in allLineItems)
        {
            var variant =await applicationDbContext.Set<Variant>()
                .Include(v => v.StockItems)
                .ThenInclude(si => si.StockLocation)
                .FirstOrDefaultAsync(v => v.Id == lineItem.VariantId, cancellationToken);

            if (variant is null)
            {
                continue;
            }

            var availableLocations = variant.StockItems
                .Where(si => si.CountAvailable >= lineItem.Quantity)
                .Select(si => si.StockLocation)
                .ToList();

            if (!availableLocations.Any())
            {
                continue;
            }

            var customerLat = order.ShipAddressLatitude;
            var customerLon = order.ShipAddressLongitude;

            var selectedLocation = strategy.SelectLocation(
                variant: variant,
                requiredQuantity: lineItem.Quantity,
                availableLocations: availableLocations,
                customerLatitude: customerLat,
                customerLongitude: customerLon
            );

            if (selectedLocation is null)
            {
                continue;
            }

            var shipmentPlan =
                fulfillmentShipmentPlans.FirstOrDefault(predicate: p => p.FulfillmentLocationId == selectedLocation.Id);
            if (shipmentPlan is null)
            {
                var shipmentPlanResult = FulfillmentShipmentPlan.Create(fulfillmentLocationId: selectedLocation.Id, items: new List<FulfillmentItem>());
                if (shipmentPlanResult.IsError) return shipmentPlanResult.Errors;

                shipmentPlan = shipmentPlanResult.Value;
                fulfillmentShipmentPlans.Add(item: shipmentPlan);
            }

            var fulfillmentItemResult = FulfillmentItem.Create(lineItemId: lineItem.Id, variantId: variant.Id, quantity: lineItem.Quantity);
            if (fulfillmentItemResult.IsError) return fulfillmentItemResult.Errors;

            shipmentPlan.Items.Add(item: fulfillmentItemResult.Value);
            fulfilledLineItemIds.Add(item: lineItem.Id);
        }

        var totalItems = allLineItems.Count;
        var fulfilledItems = fulfilledLineItemIds.Count;

        var isFullyFulfillable = totalItems > 0 && totalItems == fulfilledItems;
        var isPartialFulfillment = fulfilledItems > 0 && !isFullyFulfillable;

        var planResult = FulfillmentPlanResult.Create(
            shipments: fulfillmentShipmentPlans,
            isFullyFulfillable: isFullyFulfillable,
            isPartialFulfillment: isPartialFulfillment);

        return planResult;
    }
}
