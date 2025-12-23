// src/ReSys.Shop.Admin/src/models/admin/catalog/products/option-types/product-option-type.model.js

/**
 * @typedef {import('../../../../common/common.model').HasPosition} HasPosition
 */

/**
 * @typedef {object} ProductOptionTypeParameter
 * @property {string} optionTypeId - The ID of the option type.
 * @property {number} position - The display order of the product option type.
 */

/**
 * @typedef {ProductOptionTypeParameter & object} ProductOptionTypeResult
 * @property {string} id - The unique identifier of the product option type.
 * @property {string} [optionTypeName] - The name of the associated option type.
 * @property {string} [optionTypePresentation] - The presentation of the associated option type.
 */
