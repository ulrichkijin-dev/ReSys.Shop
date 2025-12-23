/**
 * @typedef {0|1} AddressType
 */
/** @enum {AddressType} */
export const AddressType = {
  Shipping: 0,
  Billing: 1,
};

/**
 * @typedef {object} AddressParam
 * @property {string} firstName
 * @property {string} lastName
 * @property {string | null} [company]
 * @property {string} address1
 * @property {string | null} [address2]
 * @property {string} city
 * @property {string} zipcode
 * @property {string} phone
 * @property {string | null} [stateName]
 * @property {string | null} [label]
 * @property {boolean} quickCheckout
 * @property {boolean} isDefault
 * @property {AddressType} type
 * @property {string} countryId - Guid
 * @property {string | null} [stateId] - Guid
 */

/**
 * @typedef {object} AddressListItem
 * @property {string} id - Guid
 * @property {string} firstName
 * @property {string} lastName
 * @property {string} address1
 * @property {string} city
 * @property {string} zipcode
 * @property {string} phone
 * @property {string | null} [stateName]
 * @property {string | null} [label]
 * @property {boolean} quickCheckout
 * @property {boolean} isDefault
 * @property {AddressType} type
 * @property {string} countryId - Guid
 * @property {string | null} [stateId] - Guid
 * @property {string | null} [company]
 * @property {string} createdAt - DateTimeOffset
 * @property {string | null} [updatedAt] - DateTimeOffset
 */

/**
 * @typedef {AddressListItem & {
 *   address2?: string | null,
 *   countryName?: string | null,
 *   userId?: string | null,
 * }} AddressDetail
 */

/**
 * @typedef {object} AddressSelectItem
 * @property {string} id - Guid
 * @property {string} label
 * @property {string} addressSummary
 */
