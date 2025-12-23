// src/ReSys.Shop.Admin/src/models/admin/orders/shipments/shipment.model.js

/**
 * @typedef {object} ShipmentParameter
 * @property {string} stockLocationId - The ID of the stock location for the shipment.
 * @property {ShipmentFulfillmentItemRequest[]} items - List of line items to include in the shipment.
 */

/**
 * @typedef {object} ShipmentUpdateParameter
 * @property {string} [trackingNumber] - The tracking number for the shipment.
 */

/**
 * @typedef {object} ShipmentFulfillmentItemRequest
 * @property {string} lineItemId - The ID of the order line item.
 * @property {string} variantId - The ID of the product variant.
 * @property {number} quantity - The quantity of the variant in this fulfillment.
 */

/**
 * @typedef {object} ShipmentAddItemParameter
 * @property {string} variantId - The ID of the product variant to add.
 * @property {number} quantity - The quantity of the variant to add.
 */

/**
 * @typedef {object} ShipmentRemoveItemParameter
 * @property {string} variantId - The ID of the product variant to remove.
 * @property {number} quantity - The quantity of the variant to remove.
 */

/**
 * @typedef {object} ShipmentAutoPlanParameter
 * @property {string} [strategy] - The fulfillment strategy to use (default "HighestStock").
 */

/**
 * @typedef {object} ShipmentTransferToShipmentParameter
 * @property {string} targetShipmentId - The ID of the target shipment.
 * @property {string} variantId - The ID of the variant to transfer.
 * @property {number} quantity - The quantity to transfer.
 */

/**
 * @typedef {object} ShipmentTransferToLocationParameter
 * @property {string} targetStockLocationId - The ID of the target stock location.
 * @property {string} variantId - The ID of the variant to transfer.
 * @property {number} quantity - The quantity to transfer.
 */


/**
 * @typedef {object} ShipmentListItem
 * @property {string} id - The unique identifier of the shipment.
 * @property {string} orderId - The ID of the parent order.
 * @property {string} number - The shipment number.
 * @property {string} state - The current state of the shipment (e.g., "Pending", "Shipped").
 * @property {string} [trackingNumber] - The tracking number.
 * @property {string} stockLocationName - The name of the stock location.
 * @property {string} [shippedAt] - Date and time when the shipment was marked as shipped (ISO 8601).
 * @property {string} createdAt - Date and time when the shipment was created (ISO 8601).
 * @property {string} [updatedAt] - Date and time when the shipment was last updated (ISO 8601).
 * @property {number} itemCount - Total count of items in the shipment.
 */

/**
 * @typedef {ShipmentListItem & object} ShipmentDetail
 * @property {ShipmentDetailLineItem[]} lineItems - Detailed line items within this shipment.
 */

/**
 * @typedef {object} ShipmentDetailLineItem
 * @property {string} id - The unique ID of the line item in shipment.
 * @property {string} variantId - ID of the product variant.
 * @property {string} capturedName - Name of the product variant.
 * @property {number} quantity - Quantity of the variant.
 * @property {number} unitPrice - Unit price of the variant.
 */
