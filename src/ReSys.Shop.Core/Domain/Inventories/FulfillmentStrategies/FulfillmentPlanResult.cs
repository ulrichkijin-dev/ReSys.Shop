namespace ReSys.Shop.Core.Domain.Inventories.FulfillmentStrategies;

/// <summary>
/// Represents the overall plan for fulfilling an order, broken down by shipments.
/// </summary>
public record FulfillmentPlanResult
{
    public List<FulfillmentShipmentPlan> Shipments { get; init; } = new();

    /// <summary>
    /// Gets a value indicating whether all line items in the original order are fully covered by this plan.
    /// </summary>
    public bool IsFullyFulfillable { get; init; }

    /// <summary>
    /// Gets a value indicating whether some, but not all, line items are fulfilled by this plan.
    /// </summary>
    public bool IsPartialFulfillment { get; init; }

    private FulfillmentPlanResult() { }

    /// <summary>
    /// Creates a new FulfillmentPlanResult with validation.
    /// </summary>
    /// <param name="shipments">The list of shipments in the plan.</param>
    /// <param name="isFullyFulfillable">Whether the plan fulfills the entire order.</param>
    /// <param name="isPartialFulfillment">Whether the plan partially fulfills the order.</param>
    /// <returns>An ErrorOr containing the fulfillment plan or validation errors.</returns>
    public static ErrorOr<FulfillmentPlanResult> Create(List<FulfillmentShipmentPlan> shipments, bool isFullyFulfillable, bool isPartialFulfillment)
    {
        if (shipments is null)
            return Error.Validation(code: "FulfillmentPlan.NullShipments", description: "Fulfillment plan shipments list cannot be null.");

        if (isFullyFulfillable && isPartialFulfillment)
            return Error.Validation(code: "FulfillmentPlan.InvalidState", description: "A plan cannot be both fully fulfillable and partial.");

        if (isPartialFulfillment && !shipments.Any())
            return Error.Validation(code: "FulfillmentPlan.InvalidState", description: "A partial fulfillment plan must contain at least one shipment.");
        
        if (!isFullyFulfillable && !isPartialFulfillment && shipments.Any())
            return Error.Validation(code: "FulfillmentPlan.InvalidState", description: "A plan that is not fulfilled should not contain any shipments.");

        var result = new FulfillmentPlanResult
        {
            Shipments = shipments,
            IsFullyFulfillable = isFullyFulfillable,
            IsPartialFulfillment = isPartialFulfillment
        };
        return result;
    }
}

public record FulfillmentShipmentPlan
{
    public Guid FulfillmentLocationId { get; init; }
    public List<FulfillmentItem> Items { get; init; } = new();

    public static ErrorOr<FulfillmentShipmentPlan> Create(Guid fulfillmentLocationId, List<FulfillmentItem>? items)
    {
        if (fulfillmentLocationId == Guid.Empty)
        {
            return Error.Validation(code: "FulfillmentShipmentPlan.LocationRequired", description: "Fulfillment location ID is required.");
        }
        if (items == null || !items.Any())
        {
            return Error.Validation(code: "FulfillmentShipmentPlan.EmptyItems", description: "Shipment plan must contain items.");
        }

        var plan = new FulfillmentShipmentPlan
        {
            FulfillmentLocationId = fulfillmentLocationId,
            Items = items
        };
        return plan;
    }
}

public record FulfillmentItem
{
    public Guid LineItemId { get; init; }
    public Guid VariantId { get; init; }
    public int Quantity { get; init; }
    public bool IsBackordered { get; init; }

    public static ErrorOr<FulfillmentItem> Create(Guid lineItemId, Guid variantId, int quantity, bool isBackordered = false)
    {
        if (lineItemId == Guid.Empty)
        {
            return Error.Validation(code: "FulfillmentItem.LineItemIdRequired", description: "Line item ID is required.");
        }
        if (variantId == Guid.Empty)
        {
            return Error.Validation(code: "FulfillmentItem.VariantIdRequired", description: "Variant ID is required.");
        }
        if (quantity <= 0)
        {
            return Error.Validation(code: "FulfillmentItem.QuantityInvalid", description: "Quantity must be greater than zero.");
        }

        var item = new FulfillmentItem
        {
            LineItemId = lineItemId,
            VariantId = variantId,
            Quantity = quantity,
            IsBackordered = isBackordered
        };
        return item;
    }
}
