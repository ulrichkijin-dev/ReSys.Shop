// src/ReSys.Shop.Admin/src/models/admin/catalog/property-types/property-type.model.js

/**
 * @typedef {import('../../common/common.model').HasPosition} HasPosition
 * @typedef {import('../../common/common.model').HasMetadata} HasMetadata
 */

/**
 * @enum {string} PropertyTypeKind
 * @property {string} Text - For text-based properties.
 * @property {string} Number - For numerical properties.
 * @property {string} Boolean - For boolean properties.
 * @property {string} Date - For date properties.
 * @property {string} Select - For properties with predefined selectable values.
 * // Add other kinds as discovered from backend
 */
export const PropertyTypeKind = {
  Text: 'Text',
  Number: 'Number',
  Boolean: 'Boolean',
  Date: 'Date',
  Select: 'Select',
};

/**
 * @enum {string} DisplayOn
 * @property {string} Storefront - Display on the customer-facing storefront.
 * @property {string} Admin - Display on the administration panel.
 */
export const DisplayOn = {
  Storefront: 'Storefront',
  Admin: 'Admin',
};

/**
 * @typedef {object} PropertyTypeParameter
 * @property {string} name - The internal name of the property type (unique).
 * @property {string} presentation - The display name of the property type.
 * @property {PropertyTypeKind} kind - The kind/data type of the property (e.g., "Text", "Number").
 * @property {boolean} filterable - Whether the property type can be used for filtering products.
 * @property {string} [filterParam] - Optional parameter for filtering (e.g., slug for URL).
 * @property {DisplayOn} displayOn - Where the property should be displayed (e.g., "Storefront", "Admin").
 * @property {number} position - The display order of the property type.
 * @property {Object.<string, any>} [publicMetadata] - Public metadata.
 * @property {Object.<string, any>} [privateMetadata] - Private metadata.
 */

/**
 * @typedef {object} PropertyTypeSelectItem
 * @property {string} id - The unique identifier of the property type.
 * @property {string} name - The internal name of the property type.
 * @property {string} presentation - The display name of the property type.
 */

/**
 * @typedef {object} PropertyTypeListItem
 * @property {string} id - The unique identifier of the property type.
 * @property {string} name - The internal name of the property type.
 * @property {string} presentation - The display name of the property type.
 * @property {PropertyTypeKind} kind - The kind/data type of the property.
 * @property {string} [filterParam] - Optional parameter for filtering.
 * @property {DisplayOn} displayOn - Where the property is displayed.
 * @property {number} position - The display order of the property type.
 * @property {boolean} filterable - Whether the property is filterable.
 * @property {number} productPropertyCount - Number of products using this property type.
 * @property {string} createdAt - Date and time when the property type was created (ISO 8601).
 * @property {string} [updatedAt] - Date and time when the property type was last updated (ISO 8601).
 */

/**
 * @typedef {PropertyTypeListItem & object} PropertyTypeDetail
 * @property {Object.<string, any>} [publicMetadata] - Public metadata.
 * @property {Object.<string, any>} [privateMetadata] - Private metadata.
 */

/**
 * @typedef {object} UpdateDisplayOnParameter
 * @property {DisplayOn} displayOn - Where the property should be displayed (e.g., "Storefront", "Admin").
 */
