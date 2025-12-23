// src/ReSys.Shop.Admin/src/models/admin/catalog/variants/prices/variant-price.model.js

/**
 * @typedef {object} VariantPriceSetRequest
 * @property {number | null} [amount]
 * @property {number | null} [compareAtAmount]
 * @property {string} currency
 */

/**
 * @typedef {object} VariantPriceItem
 * @property {number | null} [amount]
 * @property {string} id - Guid
 * @property {number | null} [compareAtAmount]
 * @property {string} currency
 * @property {boolean} discounted
 */

/**
 * @typedef {import('@/models/common/common.model').QueryableParams & {}} VariantPriceListRequest
 */
