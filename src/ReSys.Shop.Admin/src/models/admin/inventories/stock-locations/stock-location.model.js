// src/ReSys.Shop.Admin/src/models/admin/inventories/stock-locations/stock-location.model.js

/**
 * @typedef {import('../../../common/common.model').HasMetadata} HasMetadata
 */

/**
 * @typedef {object} StockLocationParameter
 * @property {string} name - The internal name of the stock location (unique).
 * @property {string} presentation - The display name of the stock location.
 * @property {boolean} active - Whether the stock location is active.
 * @property {boolean} default - Whether this is the default stock location.
 * @property {string} [countryId] - The ID of the country.
 * @property {string} [stateId] - The ID of the state/province.
 * @property {string} [address1] - First line of the address.
 * @property {string} [address2] - Second line of the address.
 * @property {string} [city] - City.
 * @property {string} [zipCode] - Zip/postal code.
 * @property {string} [phone] - Phone number.
 * @property {string} [company] - Company name.
 * @property {Object.<string, any>} [publicMetadata] - Public metadata.
 * @property {Object.<string, any>} [privateMetadata] - Private metadata.
 */

/**
 * @typedef {object} StockLocationSelectItem
 * @property {string} id - The unique identifier of the stock location.
 * @property {string} name - The internal name of the stock location.
 * @property {string} presentation - The display name of the stock location.
 * @property {boolean} active - Whether the stock location is active.
 * @property {boolean} default - Whether this is the default stock location.
 */

/**
 * @typedef {object} StockLocationListItem
 * @property {string} id - The unique identifier of the stock location.
 * @property {string} name - The internal name of the stock location.
 * @property {string} presentation - The display name of the stock location.
 * @property {boolean} active - Whether the stock location is active.
 * @property {boolean} default - Whether this is the default stock location.
 * @property {string} [city] - City.
 * @property {string} [countryName] - Name of the country.
 * @property {string} [stateName] - Name of the state/province.
 * @property {number} stockItemCount - Number of stock items in this location.
 * @property {string} createdAt - Date and time when the stock location was created (ISO 8601).
 * @property {string} [updatedAt] - Date and time when the stock location was last updated (ISO 8601).
 */

/**
 * @typedef {StockLocationListItem & object} StockLocationDetail
 * @property {string} [address1] - First line of the address.
 * @property {string} [address2] - Second line of the address.
 * @property {string} [zipcode] - Zip/postal code.
 * @property {string} [phone] - Phone number.
 * @property {string} [company] - Company name.
 * @property {Object.<string, any>} [publicMetadata] - Public metadata.
 * @property {Object.<string, any>} [privateMetadata] - Private metadata.
 */
