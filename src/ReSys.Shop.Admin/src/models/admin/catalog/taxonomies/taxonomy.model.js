// src/ReSys.Shop.Admin/src/models/admin/catalog/taxonomies/taxonomy.model.js

/**
 * @typedef {import('../../common/common.model').HasPosition} HasPosition
 * @typedef {import('../../common/common.model').HasMetadata} HasMetadata
 */

/**
 * @typedef {object} TaxonomyParameter
 * @property {string} name - The internal name of the taxonomy (unique).
 * @property {string} presentation - The display name of the taxonomy.
 * @property {number} position - The display order of the taxonomy.
 * @property {Object.<string, any>} [publicMetadata] - Public metadata.
 * @property {Object.<string, any>} [privateMetadata] - Private metadata.
 */

/**
 * @typedef {object} TaxonomySelectItem
 * @property {string} id - The unique identifier of the taxonomy.
 * @property {string} name - The internal name of the taxonomy.
 * @property {string} presentation - The display name of the taxonomy.
 */

/**
 * @typedef {object} TaxonomyListItem
 * @property {string} id - The unique identifier of the taxonomy.
 * @property {string} name - The internal name of the taxonomy.
 * @property {string} presentation - The display name of the taxonomy.
 * @property {number} position - The display order of the taxonomy.
 * @property {number} taxonCount - Number of taxons within this taxonomy.
 * @property {string} createdAt - Date and time when the taxonomy was created (ISO 8601).
 * @property {string} [updatedAt] - Date and time when the taxonomy was last updated (ISO 8601).
 */

/**
 * @typedef {TaxonomyParameter & object} TaxonomyDetail
 * @property {string} id - The unique identifier of the taxonomy.
 */
