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

        var allLineItems = order.LineItems.ToList();
        var fulfilledLineItemIds = new HashSet<Guid>();
        var itemsByLocation = new Dictionary<Guid, List<FulfillmentItem>>();

        foreach (var lineItem in allLineItems)
        {
            var variant = await applicationDbContext.Set<Variant>()
                .Include(v => v.StockItems)
                .ThenInclude(si => si.StockLocation)
                .FirstOrDefaultAsync(v => v.Id == lineItem.VariantId, cancellationToken);

            if (variant is null) continue;

            // Find locations that have stock OR are backorderable
            var potentialLocations = variant.StockItems
                .Where(si => si.InStock || si.Backorderable)
                .Select(si => si.StockLocation)
                .ToList();

            if (!potentialLocations.Any()) continue;

            var customerLat = order.ShipAddressLatitude;
            var customerLon = order.ShipAddressLongitude;

            var selectedLocation = strategy.SelectLocation(
                variant: variant,
                requiredQuantity: lineItem.Quantity,
                availableLocations: potentialLocations,
                customerLatitude: customerLat,
                customerLongitude: customerLon
            );

            if (selectedLocation is null) continue;

            var stockItem = variant.StockItems.First(si => si.StockLocationId == selectedLocation.Id);
            bool isBackordered = stockItem.CountAvailable < lineItem.Quantity;

            if (!itemsByLocation.ContainsKey(selectedLocation.Id))
            {
                itemsByLocation[selectedLocation.Id] = new List<FulfillmentItem>();
            }

            var fulfillmentItemResult = FulfillmentItem.Create(
                lineItemId: lineItem.Id, 
                variantId: variant.Id, 
                quantity: lineItem.Quantity,
                isBackordered: isBackordered);

            if (fulfillmentItemResult.IsError) return fulfillmentItemResult.Errors;

            itemsByLocation[selectedLocation.Id].Add(fulfillmentItemResult.Value);
            fulfilledLineItemIds.Add(lineItem.Id);
        }

        var fulfillmentShipmentPlans = new List<FulfillmentShipmentPlan>();
        foreach (var entry in itemsByLocation)
        {
            var shipmentPlanResult = FulfillmentShipmentPlan.Create(entry.Key, entry.Value);
            if (shipmentPlanResult.IsError) return shipmentPlanResult.Errors;
            fulfillmentShipmentPlans.Add(shipmentPlanResult.Value);
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
