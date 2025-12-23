// src/ReSys.Shop.Admin/src/models/admin/catalog/option-values/option-value.model.js

/**
 * @typedef {import('../../common/common.model').HasPosition} HasPosition
 * @typedef {import('../../common/common.model').HasMetadata} HasMetadata
 */

/**
 * @typedef {object} OptionValueParameter
 * @property {string} optionTypeId - The ID of the parent option type.
 * @property {string} name - The internal name of the option value (unique within its option type).
 * @property {string} presentation - The display name of the option value.
 * @property {number} position - The display order of the option value.
 * @property {Object.<string, any>} [publicMetadata] - Public metadata.
 * @property {Object.<string, any>} [privateMetadata] - Private metadata.
 */

/**
 * @typedef {object} OptionValueSelectItem
 * @property {string} id - The unique identifier of the option value.
 * @property {string} name - The internal name of the option value.
 * @property {string} presentation - The display name of the option value.
 * @property {string} [optionTypeId] - The ID of the parent option type.
 * @property {string} [optionTypeName] - The name of the parent option type.
 * @property {string} [optionTypePresentation] - The presentation of the parent option type.
 */

/**
 * @typedef {object} OptionValueListItem
 * @property {string} id - The unique identifier of the option value.
 * @property {string} name - The internal name of the option value.
 * @property {string} presentation - The display name of the option value.
 * @property {string} optionTypeName - The name of the parent option type.
 * @property {number} position - The display order of the option value.
 * @property {string} createdAt - Date and time when the option value was created (ISO 8601).
 * @property {string} [updatedAt] - Date and time when the option value was last updated (ISO 8601).
 */

/**
 * @typedef {OptionValueParameter & object} OptionValueDetail
 * @property {string} id - The unique identifier of the option value.
 */
