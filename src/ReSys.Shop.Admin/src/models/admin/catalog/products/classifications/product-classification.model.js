// src/ReSys.Shop.Admin/src/models/admin/catalog/products/classifications/product-classification.model.js

/**
 * @typedef {import('../../../../common/common.model').HasPosition} HasPosition
 */

/**
 * @typedef {object} ProductClassificationParameter
 * @property {string} taxonId - The ID of the taxon.
 * @property {number} position - The display order of the classification.
 */

/**
 * @typedef {ProductClassificationParameter & object} ProductClassificationResult
 * @property {string} id - The unique identifier of the product classification.
 * @property {string} [taxonName] - The name of the associated taxon.
 * @property {string} [taxonPrettyName] - The pretty name of the associated taxon.
 */
