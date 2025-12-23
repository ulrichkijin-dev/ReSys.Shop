// src/ReSys.Shop.Admin/src/models/admin/catalog/option-types/option-type.model.js

/**
 * @typedef {import('../../common/common.model').HasPosition} HasPosition
 * @typedef {import('../../common/common.model').HasMetadata} HasMetadata
 */

/**
 * @typedef {object} OptionTypeParameter
 * @property {string} name - The internal name of the option type (unique).
 * @property {string} presentation - The display name of the option type.
 * @property {boolean} filterable - Whether the option type can be used for filtering products.
 * @property {number} position - The display order of the option type.
 * @property {Object.<string, any>} [publicMetadata] - Public metadata.
 * @property {Object.<string, any>} [privateMetadata] - Private metadata.
 */

/**
 * @typedef {object} OptionTypeSelectItem
 * @property {string} id - The unique identifier of the option type.
 * @property {string} name - The internal name of the option type.
 * @property {string} presentation - The display name of the option type.
 */

/**
 * @typedef {object} OptionTypeListItem
 * @property {string} id - The unique identifier of the option type.
 * @property {string} name - The internal name of the option type.
 * @property {string} presentation - The display name of the option type.
 * @property {number} position - The display order of the option type.
 * @property {boolean} filterable - Whether the option type can be used for filtering.
 * @property {number} optionValueCount - Number of option values associated with this option type.
 * @property {number} productCount - Number of products using this option type.
 * @property {string} createdAt - Date and time when the option type was created (ISO 8601).
 * @property {string} [updatedAt] - Date and time when the option type was last updated (ISO 8601).
 */

/**
 * @typedef {OptionTypeParameter & {
 *   id: string, // Guid
 * }} OptionTypeDetail
 */
