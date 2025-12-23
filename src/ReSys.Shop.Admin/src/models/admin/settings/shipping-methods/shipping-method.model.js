// src/ReSys.Shop.Admin/src/models/admin/settings/shipping-methods/shipping-method.model.js

/**
 * @enum {string} ShippingMethodType
 * @property {string} FlatRate - Flat rate shipping.
 * @property {string} PerItem - Per item shipping.
 * @property {string} WeightBased - Weight-based shipping.
 * // Add other types as discovered
 */
export const ShippingMethodType = {
  FlatRate: 'FlatRate',
  PerItem: 'PerItem',
  WeightBased: 'WeightBased',
};

/**
 * @enum {string} DisplayOn
 * @property {string} Both - Display on both storefront and admin.
 * @property {string} Storefront - Display on storefront only.
 * @property {string} Admin - Display on admin panel only.
 */
export const DisplayOn = {
  Both: 'Both',
  Storefront: 'Storefront',
  Admin: 'Admin',
};

/**
 * @typedef {object} ShippingMethodParameter
 * @property {string} name - The internal name of the shipping method.
 * @property {string} presentation - The display name of the shipping method.
 * @property {string} [description] - A description of the shipping method.
 * @property {ShippingMethodType} type - The type of the shipping method.
 * @property {boolean} active - Whether the shipping method is active.
 * @property {number} position - The display order of the shipping method.
 * @property {number} baseCost - The base cost of the shipping method.
 * @property {number} [estimatedDaysMin] - Minimum estimated delivery days.
 * @property {number} [estimatedDaysMax] - Maximum estimated delivery days.
 * @property {number} [maxWeight] - Maximum weight for this shipping method.
 * @property {DisplayOn} displayOn - Where the shipping method should be displayed.
 * @property {Object.<string, any>} [publicMetadata] - Public metadata.
 * @property {Object.<string, any>} [privateMetadata] - Private metadata.
 */

/**
 * @typedef {object} ShippingMethodSelectItem
 * @property {string} id - The unique identifier of the shipping method.
 * @property {string} name - The internal name of the shipping method.
 * @property {string} presentation - The display name of the shipping method.
 * @property {ShippingMethodType} type - The type of the shipping method.
 * @property {boolean} active - Whether the shipping method is active.
 * @property {number} baseCost - The base cost of the shipping method.
 * @property {string} estimatedDelivery - A string representing the estimated delivery time.
 */

/**
 * @typedef {object} ShippingMethodListItem
 * @property {string} id - The unique identifier of the shipping method.
 * @property {string} name - The internal name of the shipping method.
 * @property {string} presentation - The display name of the shipping method.
 * @property {string} [description] - A description of the shipping method.
 * @property {ShippingMethodType} type - The type of the shipping method.
 * @property {boolean} active - Whether the shipping method is active.
 * @property {number} position - The display order of the shipping method.
 * @property {number} baseCost - The base cost of the shipping method.
 * @property {number} [estimatedDaysMin] - Minimum estimated delivery days.
 * @property {number} [estimatedDaysMax] - Maximum estimated delivery days.
 * @property {number} [maxWeight] - Maximum weight for this shipping method.
 * @property {string} createdAt - Date and time when the shipping method was created (ISO 8601).
 * @property {string} [updatedAt] - Date and time when the shipping method was last updated (ISO 8601).
 * @property {Object.<string, any>} [publicMetadata] - Public metadata.
 * @property {Object.<string, any>} [privateMetadata] - Private metadata.
 */

/**
 * @typedef {ShippingMethodListItem & object} ShippingMethodDetail
 * @property {Object.<string, any>} [publicMetadata] - Public metadata.
 * @property {Object.<string, any>} [privateMetadata] - Private metadata.
 */
