// src/ReSys.Shop.Admin/src/models/admin/catalog/variants/variant.model.js

/**
 * @typedef {object} VariantParameter
 * @property {string} productId - Guid
 * @property {string | null} [sku]
 * @property {string | null} [barcode]
 * @property {number | null} [weight]
 * @property {number | null} [height]
 * @property {number | null} [width]
 * @property {number | null} [depth]
 * @property {string | null} [dimensionsUnit]
 * @property {string | null} [weightUnit]
 * @property {number | null} [costPrice]
 * @property {string | null} [costCurrency]
 * @property {boolean} trackInventory
 * @property {number} position
 * @property {{ [key: string]: any } | null} [publicMetadata]
 * @property {{ [key: string]: any } | null} [privateMetadata]
 */

/**
 * @typedef {object} VariantSelectItem
 * @property {string} id - Guid
 * @property {string} productName
 * @property {string | null} [sku]
 * @property {string} optionsText
 * @property {boolean} isMaster
 */

/**
 * @typedef {object} VariantListItem
 * @property {string} id - Guid
 * @property {string} productId - Guid
 * @property {string} productName
 * @property {boolean} isMaster
 * @property {string | null} [sku]
 * @property {string | null} [barcode]
 * @property {string} optionsText
 * @property {number | null} [weight]
 * @property {string | null} [weightUnit]
 * @property {boolean} trackInventory
 * @property {number | null} [costPrice]
 * @property {string | null} [costCurrency]
 * @property {number} position
 * @property {boolean} inStock
 * @property {boolean} purchasable
 * @property {boolean} available
 * @property {number} totalOnHand
 * @property {string} createdAt - DateTimeOffset
 * @property {string | null} [updatedAt] - DateTimeOffset
 */

/**
 * @typedef {VariantListItem & {
 *   height?: number | null,
 *   width?: number | null,
 *   depth?: number | null,
 *   dimensionsUnit?: string | null,
 *   discontinueOn?: string | null, // DateTimeOffset
 *   publicMetadata?: { [key: string]: any } | null,
 *   privateMetadata?: { [key: string]: any } | null,
 *   optionValueNames: string[],
 * }} VariantDetail
 */

/**
 * @typedef {VariantParameter & {
 *   optionValueIds: string[], // Guid[]
 * }} VariantCreateUpdateRequest
 */
