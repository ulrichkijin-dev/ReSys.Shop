// src/ReSys.Shop.Admin/src/models/admin/catalog/products/product.model.js

/**
 * @typedef {object} ProductParameter
 * @property {string} name
 * @property {string} presentation
 * @property {string | null} [description]
 * @property {string} slug
 * @property {string | null} [availableOn] - DateTimeOffset
 * @property {string | null} [makeActiveAt] - DateTimeOffset
 * @property {string | null} [discontinueOn] - DateTimeOffset
 * @property {boolean} isDigital
 * @property {string | null} [metaTitle]
 * @property {string | null} [metaDescription]
 * @property {string | null} [metaKeywords]
 * @property {{ [key: string]: any } | null} [publicMetadata]
 * @property {{ [key: string]: any } | null} [privateMetadata]
 */

/**
 * @typedef {object} ProductSelectItem
 * @property {string} id - Guid
 * @property {string} name
 * @property {string} presentation
 * @property {string} slug
 */

/**
 * @typedef {object} ProductListItem
 * @property {string} id - Guid
 * @property {string} name
 * @property {string} presentation
 * @property {string | null} [description]
 * @property {string} slug
 * @property {string} status // ProductStatus enum as string
 * @property {boolean} isDigital
 * @property {string | null} [availableOn] - DateTimeOffset
 * @property {number} variantCount
 * @property {number} imageCount
 * @property {boolean} available
 * @property {boolean} purchasable
 * @property {string} createdAt - DateTimeOffset
 * @property {string | null} [updatedAt] - DateTimeOffset
 */

/**
 * @typedef {ProductParameter & {
 *   id: string, // Guid
 * }} ProductDetail
 */
