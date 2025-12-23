/**
 * @typedef {Object} CartDetail
 * @property {string} id
 * @property {string} number
 * @property {string} token
 * @property {string} state
 * @property {number} total
 * @property {number} itemTotal
 * @property {number} shipmentTotal
 * @property {number} adjustmentTotal
 * @property {string} currency
 * @property {string} email
 * @property {string} [paymentClientSecret]
 * @property {string} [paymentApprovalUrl]
 * @property {Array<CartLineItem>} lineItems
 * @property {Array<CartAdjustment>} adjustments
 * @property {CartAddress} [shippingAddress]
 * @property {CartAddress} [billingAddress]
 */

/**
 * @typedef {Object} CartLineItem
 * @property {string} id
 * @property {string} variantId
 * @property {string} name
 * @property {string} sku
 * @property {number} quantity
 * @property {number} price
 * @property {number} total
 */

/**
 * @typedef {Object} CartAdjustment
 * @property {string} description
 * @property {number} amount
 * @property {string} scope
 */

/**
 * @typedef {Object} CartAddress
 * @property {string} firstName
 * @property {string} lastName
 * @property {string} address1
 * @property {string} address2
 * @property {string} city
 * @property {string} zipCode
 * @property {string} phone
 * @property {string} countryName
 * @property {string} stateName
 */