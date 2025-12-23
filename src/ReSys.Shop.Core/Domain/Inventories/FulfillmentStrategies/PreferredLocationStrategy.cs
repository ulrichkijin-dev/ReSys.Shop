using ReSys.Shop.Core.Domain.Catalog.Products.Variants;
using ReSys.Shop.Core.Domain.Inventories.Locations;

namespace ReSys.Shop.Core.Domain.Inventories.FulfillmentStrategies;

/// <summary>
/// Fulfillment strategy that respects admin-configured preferred locations.
/// Attempts to fulfill from the preferred location with automatic fallback to other locations.
/// </summary>
/// <remarks>
/// <para>
/// <b>Strategy Behavior:</b>
/// - Prioritizes admin-configured preferred locations
/// - Falls back to other locations if preferred lacks stock
/// - Does not split orders across multiple locations by default
/// - Can be configured per store, vendor, or product category
/// </para>
/// 
/// <para>
/// <b>Configuration:</b>
/// Preference is stored in location.PublicMetadata or StoreConfiguration:
/// - Key: "fulfillment_preference_priority" (int, 0-100)
/// - Higher values = higher priority
/// - Default priority for non-preferred locations: 0
/// </para>
/// 
/// <para>
/// <b>Use Cases:</b>
/// - Preferring first-party fulfillment over third-party
/// - Routing to specific distribution centers by region
/// - Promoting newer warehouses to clear inventory
/// - Vendor-specific fulfillment rules
/// - 3PL vs. 1PL routing decisions
/// </para>
/// 
/// <para>
/// <b>Performance:</b>
/// - O(n log n) due to sorting by preference priority
/// - No external API calls or complex calculations
/// - Fast even with many locations
/// </para>
/// </remarks>
public sealed class PreferredLocationStrategy : IFulfillmentStrategy
{
    private const int DefaultPreferencePriority = 0;

    public string Name => "Preferred Location";

    public string Description =>
        "Selects admin-configured preferred locations. " +
        "Ideal for brand control and vendor relationships.";

    public bool SupportsMultipleLocations => false;

    /// <summary>
    /// Selects the highest-priority location with sufficient stock.
    /// Falls back to non-preferred locations if preferred lacks stock.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Selection Order:</b>
    /// 1. Preferred locations (sorted by priority, descending)
    /// 2. Non-preferred locations (if preferred locations lack stock)
    /// 3. First location with stock (if all else fails)
    /// </para>
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

        var preferred = locationsWithStock
            .OrderByDescending(keySelector: loc => GetPreferencePriority(location: loc))
            .FirstOrDefault(predicate: loc => GetPreferencePriority(location: loc) > DefaultPreferencePriority);

        if (preferred != null)
            return preferred;

        return locationsWithStock.First();
    }

    /// <summary>
    /// Returns a single location or empty list (does not support splits).
    /// </summary>
    /// <remarks>
    /// The Preferred strategy prioritizes brand control and vendor relationships,
    /// which typically require single-location fulfillment. For split fulfillment scenarios,
    /// fall back to Nearest or HighestStock strategies.
    /// </remarks>
    public IList<(StockLocation Location, int Quantity)> SelectMultipleLocations(
        Variant variant,
        int requiredQuantity,
        IEnumerable<StockLocation> availableLocations,
        int maxLocations = 3,
        decimal? customerLatitude = null,
        decimal? customerLongitude = null)
    {
        var selected = SelectLocation(variant: variant, requiredQuantity: requiredQuantity, availableLocations: availableLocations, 
            customerLatitude: customerLatitude, customerLongitude: customerLongitude);

        if (selected == null)
            return new List<(StockLocation, int)>();

        return new List<(StockLocation, int)> { (selected, requiredQuantity) };
    }

    /// <summary>
    /// Gets the preference priority for a location.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Priority Sources (checked in order):</b>
    /// 1. Location.PublicMetadata["fulfillment_preference_priority"]
    /// 2. Location.PrivateMetadata["fulfillment_preference_priority"]
    /// 3. DefaultPreferencePriority (0)
    /// </para>
    /// </remarks>
    private static int GetPreferencePriority(StockLocation location)
    {
        if (location.PublicMetadata?.TryGetValue(key: "fulfillment_preference_priority", value: out var pubValue) == true)
        {
            if (int.TryParse(s: pubValue?.ToString(), result: out var pubPriority))
                return pubPriority;
        }

        if (location.PrivateMetadata?.TryGetValue(key: "fulfillment_preference_priority", value: out var privValue) == true)
        {
            if (int.TryParse(s: privValue?.ToString(), result: out var privPriority))
                return privPriority;
        }

        return DefaultPreferencePriority;
    }
}
