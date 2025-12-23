using ReSys.Shop.Core.Domain.Catalog.Products.Variants;
using ReSys.Shop.Core.Domain.Inventories.Locations;

namespace ReSys.Shop.Core.Domain.Inventories.FulfillmentStrategies;

/// <summary>
/// Fulfillment strategy that selects locations based on geographic proximity to the customer.
/// Uses Haversine distance calculation to find the closest location with available stock.
/// </summary>
/// <remarks>
/// <para>
/// <b>Strategy Behavior:</b>
/// - Prioritizes closest location to customer
/// - Falls back to any location with stock if customer coordinates unavailable
/// - Can split across multiple locations for large orders
/// - Minimizes shipping distance for improved delivery times
/// </para>
/// 
/// <para>
/// <b>Use Cases:</b>
/// - Same-day or next-day delivery fulfillment
/// - Reducing shipping time for time-sensitive orders
/// - Minimizing shipping distance when cost is secondary
/// </para>
/// 
/// <para>
/// <b>Performance:</b>
/// - O(n) complexity where n = number of available locations
/// - Each location requires one distance calculation if coordinates present
/// - Suitable for up to 1000+ locations per query
/// </para>
/// </remarks>
public sealed class NearestLocationStrategy : IFulfillmentStrategy
{
    public string Name => "Nearest Location";

    public string Description => 
        "Selects the location closest to the customer based on geographic distance. " +
        "Ideal for minimizing delivery time.";

    public bool SupportsMultipleLocations => true;

    /// <summary>
    /// Selects the single closest location to the customer with sufficient stock.
    /// </summary>
    /// <remarks>
    /// If customer coordinates are not provided, selects the first location with stock.
    /// Prioritizes locations with their own coordinates defined.
    /// </remarks>
    public StockLocation? SelectLocation(
        Variant variant,
        int requiredQuantity,
        IEnumerable<StockLocation> availableLocations,
        decimal? customerLatitude = null,
        decimal? customerLongitude = null)
    {
        var locationsWithStock = availableLocations
            .Where(predicate: loc => loc.StockItems.Any(predicate: si => 
                si.VariantId == variant.Id && 
                (si.CountAvailable >= requiredQuantity || si.Backorderable)))
            .ToList();

        if (!locationsWithStock.Any())
            return null;

        if (!customerLatitude.HasValue || !customerLongitude.HasValue)
            return locationsWithStock.First();

        return locationsWithStock
                   .Where(predicate: loc => loc.HasLocation)
                   .OrderBy(keySelector: loc => loc.CalculateDistanceTo(otherLatitude: customerLatitude.Value, otherLongitude: customerLongitude.Value))
                   .FirstOrDefault() 
               ?? locationsWithStock.First();
    }

    /// <summary>
    /// Selects multiple locations ordered by proximity to customer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Selection Algorithm:</b>
    /// 1. Filter locations with sufficient stock for the variant
    /// 2. Sort by distance from customer (closest first)
    /// 3. Allocate quantity from closest location until satisfied or locations exhausted
    /// 4. Return up to maxLocations ordered by distance
    /// </para>
    /// 
    /// <para>
    /// This approach minimizes the number of shipments while keeping delivery distance minimal.
    /// </para>
    /// </remarks>
    public IList<(StockLocation Location, int Quantity)> SelectMultipleLocations(
        Variant variant,
        int requiredQuantity,
        IEnumerable<StockLocation> availableLocations,
        int maxLocations = 3,
        decimal? customerLatitude = null,
        decimal? customerLongitude = null)
    {
        var result = new List<(StockLocation, int)>();
        var remaining = requiredQuantity;

        var locationsWithStock = availableLocations
            .Where(predicate: loc => loc.StockItems.Any(predicate: si => 
                si.VariantId == variant.Id && 
                si.CountAvailable > 0))
            .ToList();

        if (!locationsWithStock.Any())
            return result;

        IEnumerable<StockLocation> orderedLocations = locationsWithStock;
        if (customerLatitude.HasValue && customerLongitude.HasValue)
        {
            orderedLocations = locationsWithStock
                .OrderBy(keySelector: loc => loc.HasLocation 
                    ? loc.CalculateDistanceTo(otherLatitude: customerLatitude.Value, otherLongitude: customerLongitude.Value) 
                    : decimal.MaxValue);
        }

        foreach (var location in orderedLocations.Take(count: maxLocations))
        {
            if (remaining <= 0)
                break;

            var stockItem = location.StockItems
                .FirstOrDefault(predicate: si => si.VariantId == variant.Id);

            if (stockItem == null)
                continue;

            var availableQty = stockItem.CountAvailable;
            var qtyToAllocate = Math.Min(val1: remaining, val2: availableQty);

            if (qtyToAllocate > 0)
            {
                result.Add(item: (location, qtyToAllocate));
                remaining -= qtyToAllocate;
            }
        }

        return remaining > 0 ? new List<(StockLocation, int)>() : result;
    }
}
