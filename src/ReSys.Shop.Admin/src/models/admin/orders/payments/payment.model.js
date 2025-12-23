// src/ReSys.Shop.Admin/src/models/admin/orders/payments/payment.model.js

/**
 * @typedef {object} PaymentCreateParameter
 * @property {number} amount - The amount of the payment.
 * @property {string} paymentMethodId - The ID of the payment method used.
 * @property {string} paymentMethodType - The type of the payment method (e.g., "Credit Card", "Cash").
 */

/**
 * @typedef {object} PaymentAuthorizeParameter
 * @property {string} transactionId - The transaction ID from the payment gateway.
 * @property {string} [authCode] - The authorization code (optional).
 */

/**
 * @typedef {object} PaymentCaptureParameter
 * @property {string} [transactionId] - The transaction ID from the payment gateway (optional).
 */

/**
 * @typedef {object} PaymentRefundParameter
 * @property {number} amount - The amount to refund.
 * @property {string} reason - The reason for the refund.
 * @property {string} [transactionId] - The transaction ID for the refund (optional).
 */

/**
 * @typedef {object} PaymentVoidParameter
 * // No specific parameters, action taken on paymentId
 */

/**
 * @typedef {import('../order.model').OrderPaymentItem} PaymentListItem
 */
