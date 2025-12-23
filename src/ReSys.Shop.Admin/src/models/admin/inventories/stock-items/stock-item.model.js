// src/ReSys.Shop.Admin/src/models/admin/inventories/stock-items/stock-item.model.js

/**
 * @typedef {import('../../../common/common.model').HasMetadata} HasMetadata
 */

/**
 * @typedef {object} StockItemParameter
 * @property {string} variantId - The ID of the product variant.
 * @property {string} stockLocationId - The ID of the stock location.
 * @property {string} sku - Stock Keeping Unit for the item.
 * @property {number} quantityOnHand - Current physical quantity on hand.
 * @property {number} quantityReserved - Quantity reserved for orders.
 * @property {boolean} backorderable - Whether the item can be backordered.
 * @property {Object.<string, any>} [publicMetadata] - Public metadata.
 * @property {Object.<string, any>} [privateMetadata] - Private metadata.
 */

/**
 * @typedef {object} StockItemListItem
 * @property {string} id - The unique identifier of the stock item.
 * @property {string} variantId - The ID of the product variant.
 * @property {string} variantSku - SKU of the product variant.
 * @property {string} productName - Name of the product.
 * @property {string} stockLocationId - The ID of the stock location.
 * @property {string} locationName - Name of the stock location.
 * @property {string} sku - Stock Keeping Unit for the item.
 * @property {number} quantityOnHand - Current physical quantity on hand.
 * @property {number} quantityReserved - Quantity reserved for orders.
 * @property {number} countAvailable - Quantity available for sale.
 * @property {boolean} backorderable - Whether the item can be backordered.
 * @property {boolean} inStock - Whether the item is currently in stock.
 * @property {string} createdAt - Date and time when the stock item was created (ISO 8601).
 * @property {string} [updatedAt] - Date and time when the stock item was last updated (ISO 8601).
 */

/**
 * @typedef {StockItemListItem & object} StockItemDetail
 * @property {Object.<string, any>} [publicMetadata] - Public metadata.
 * @property {Object.<string, any>} [privateMetadata] - Private metadata.
 */

/**
 * @typedef {object} StockMovementItem
 * @property {string} id - The unique identifier of the movement.
 * @property {number} quantity - The quantity moved.
 * @property {string} originator - The source of the movement (e.g., "Order", "Manual").
 * @property {string} action - The action performed (e.g., "Adjust", "Reserve", "Release").
 * @property {string} [reason] - Reason for the movement.
 * @property {boolean} isIncrease - True if quantity increased.
 * @property {boolean} isDecrease - True if quantity decreased.
 * @property {string} createdAt - Date and time of the movement (ISO 8601).
 */

/**
 * @typedef {object} AdjustStockParameter
 * @property {number} quantity - The quantity to adjust by.
 * @property {string} [reason] - Reason for the adjustment.
 */

/**
 * @typedef {object} ReserveStockParameter
 * @property {number} quantity - The quantity to reserve.
 * @property {string} [orderId] - Optional: The ID of the order reserving the stock.
 * @property {string} [reason] - Reason for the reservation.
 */

/**
 * @typedef {object} ReleaseStockParameter
 * @property {number} quantity - The quantity to release.
 * @property {string} [orderId] - Optional: The ID of the order releasing the stock.
 * @property {string} [reason] - Reason for the release.
 */
