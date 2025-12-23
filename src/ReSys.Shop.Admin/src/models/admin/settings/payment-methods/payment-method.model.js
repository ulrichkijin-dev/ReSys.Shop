// src/ReSys.Shop.Admin/src/models/admin/settings/payment-methods/payment-method.model.js

/**
 * @enum {string} PaymentMethodType
 * @property {string} CreditCard - Credit Card payment method.
 * @property {string} Cash - Cash payment method.
 * @property {string} BankTransfer - Bank Transfer payment method.
 * // Add other types as discovered
 */
export const PaymentMethodType = {
  CreditCard: 'CreditCard',
  Cash: 'Cash',
  BankTransfer: 'BankTransfer',
};

/**
 * @enum {string} DisplayOn
 * @property {string} Both - Display on both storefront and admin.
 * @property {string} Storefront - Display on storefront only.
 * @property {string} Admin - Display on admin panel only.
 */
export const DisplayOn = {
  Both: 'Both',
  Storefront: 'Storefront',
  Admin: 'Admin',
};

/**
 * @typedef {object} PaymentMethodParameter
 * @property {string} name - The internal name of the payment method.
 * @property {string} presentation - The display name of the payment method.
 * @property {string} [description] - A description of the payment method.
 * @property {PaymentMethodType} type - The type of the payment method.
 * @property {boolean} active - Whether the payment method is active.
 * @property {number} position - The display order of the payment method.
 * @property {boolean} autoCapture - Whether payments should be automatically captured.
 * @property {DisplayOn} displayOn - Where the payment method should be displayed.
 * @property {Object.<string, any>} [publicMetadata] - Public metadata.
 * @property {Object.<string, any>} [privateMetadata] - Private metadata.
 */

/**
 * @typedef {object} PaymentMethodSelectItem
 * @property {string} id - The unique identifier of the payment method.
 * @property {string} name - The internal name of the payment method.
 * @property {string} presentation - The display name of the payment method.
 * @property {PaymentMethodType} type - The type of the payment method.
 */

/**
 * @typedef {object} PaymentMethodListItem
 * @property {string} id - The unique identifier of the payment method.
 * @property {string} name - The internal name of the payment method.
 * @property {string} presentation - The display name of the payment method.
 * @property {string} [description] - A description of the payment method.
 * @property {PaymentMethodType} type - The type of the payment method.
 * @property {boolean} active - Whether the payment method is active.
 * @property {number} position - The display order of the payment method.
 * @property {boolean} autoCapture - Whether payments should be automatically captured.
 * @property {DisplayOn} displayOn - Where the payment method should be displayed.
 * @property {string} createdAt - Date and time when the payment method was created (ISO 8601).
 * @property {string} [updatedAt] - Date and time when the payment method was last updated (ISO 8601).
 * @property {string} [deletedAt] - Date and time when the payment method was soft-deleted (ISO 8601).
 * @property {Object.<string, any>} [publicMetadata] - Public metadata.
 * @property {Object.<string, any>} [privateMetadata] - Private metadata.
 */

/**
 * @typedef {PaymentMethodListItem & object} PaymentMethodDetail
 * @property {Object.<string, any>} [publicMetadata] - Public metadata.
 * @property {Object.<string, any>} [privateMetadata] - Private metadata.
 */
