// src/ReSys.Shop.Admin/src/models/admin/catalog/products/properties/product-property.model.js

/**
 * @typedef {object} ProductPropertyParameter
 * @property {string} propertyValue
 * @property {number} position
 */

/**
 * @typedef {ProductPropertyParameter & {
 *   id: string, // Guid
 *   propertyTypeId?: string | null, // Guid
 *   propertyTypeName?: string | null,
 *   propertyTypePresentation?: string | null,
 * }} ProductPropertyResult
 */

/**
 * @typedef {import('@/models/common/common.model').QueryableParams & {
 *   productId?: string[], // Guid[]
 *   propertyTypeId?: string[], // Guid[]
 * }} ProductPropertyGetListRequest
 */

/**
 * @typedef {object} ProductPropertyManageParameter
 * @property {string} propertyValue
 * @property {number} position
 * @property {string} propertyId - Guid
 */

/**
 * @typedef {object} ProductPropertyManageRequest
 * @property {ProductPropertyManageParameter[]} data
 */
