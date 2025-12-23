// src/ReSys.Shop.Admin/src/models/admin/catalog/products/property-types/product-property-type.model.js

/**
 * @typedef {import('../../../../common/common.model').HasPosition} HasPosition
 */

/**
 * @typedef {object} ProductPropertyParameter
 * @property {string} propertyId - The ID of the property type.
 * @property {string} propertyValue - The value of the property.
 * @property {number} position - The display order of the property.
 */

/**
 * @typedef {ProductPropertyParameter & object} ProductPropertyResult
 * @property {string} id - The unique identifier of the product property.
 * @property {string} [propertyTypeId] - The ID of the associated property type.
 * @property {string} [propertyTypeName] - The name of the associated property type.
 * @property {string} [propertyTypePresentation] - The presentation of the associated property type.
 */
