/**
 * @typedef {Object} ProductItem
 * @property {string} id
 * @property {string} name
 * @property {string} presentation
 * @property {string} slug
 * @property {string} description
 * @property {string} metaTitle
 * @property {string} metaDescription
 * @property {boolean} isDigital
 * @property {string} currency
 * @property {number} price
 * @property {number} displayPrice
 * @property {string} imageUrl
 */

/**
 * @typedef {Object} ProductDetail
 * @property {string} id
 * @property {string} name
 * @property {string} presentation
 * @property {string} slug
 * @property {string} description
 * @property {Array<VariantItem>} variants
 * @property {Array<PropertyItem>} properties
 */

/**
 * @typedef {Object} VariantItem
 * @property {string} id
 * @property {string} sku
 * @property {number} price
 * @property {boolean} isMaster
 * @property {boolean} inStock
 */

/**
 * @typedef {Object} PropertyItem
 * @property {string} name
 * @property {string} presentation
 * @property {string} value
 */