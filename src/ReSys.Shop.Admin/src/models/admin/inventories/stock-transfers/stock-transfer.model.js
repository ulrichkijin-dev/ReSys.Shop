// src/ReSys.Shop.Admin/src/models/admin/inventories/stock-transfers/stock-transfer.model.js

/**
 * @typedef {object} StockTransferParameter
 * @property {string} [sourceLocationId] - The ID of the source stock location (optional).
 * @property {string} destinationLocationId - The ID of the destination stock location.
 * @property {string} [reference] - An optional reference number or description for the transfer.
 */

/**
 * @typedef {object} StockTransferVariantQuantity
 * @property {string} variantId - The ID of the product variant.
 * @property {number} quantity - The quantity of the variant to transfer.
 */

/**
 * @typedef {object} StockTransferListItem
 * @property {string} id - The unique identifier of the stock transfer.
 * @property {string} number - The unique transfer number.
 * @property {string} [sourceLocationId] - The ID of the source stock location.
 * @property {string} [sourceLocationName] - The name of the source stock location.
 * @property {string} destinationLocationId - The ID of the destination stock location.
 * @property {string} destinationLocationName - The name of the destination stock location.
 * @property {string} [reference] - An optional reference number or description.
 * @property {number} movementCount - The number of individual stock movements within this transfer.
 * @property {string} createdAt - Date and time when the stock transfer was created (ISO 8601).
 * @property {string} [updatedAt] - Date and time when the stock transfer was last updated (ISO 8601).
 */

/**
 * @typedef {StockTransferListItem & object} StockTransferDetail
 * @property {StockTransferMovementItem[]} movements - A list of individual stock movements within this transfer.
 */

/**
 * @typedef {object} StockTransferMovementItem
 * @property {string} id - The unique identifier of the movement.
 * @property {string} variantId - The ID of the product variant.
 * @property {string} variantSku - SKU of the product variant.
 * @property {string} productName - Name of the product.
 * @property {number} quantity - The quantity moved.
 * @property {string} action - The action performed (e.g., "Transfer", "Receive").
 * @property {string} originator - The source of the movement.
 */

/**
 * @typedef {object} ExecuteStockTransferParameter
 * @property {StockTransferVariantQuantity[]} items - Array of variant quantities to transfer.
 */

/**
 * @typedef {object} ReceiveStockParameter
 * @property {StockTransferVariantQuantity[]} items - Array of variant quantities to receive.
 */
