// src/ReSys.Shop.Admin/src/models/admin/orders/order.model.js

/**
 * @typedef {object} OrderCreateParameter
 * @property {string} storeId - The ID of the store the order belongs to.
 * @property {string} [currency] - The currency of the order (default "USD").
 * @property {string} [userId] - The ID of the user placing the order (optional).
 * @property {string} [email] - The customer's email address (optional).
 */

/**
 * @typedef {object} OrderUpdateParameter
 * @property {string} [email] - The customer's email address.
 * @property {string} [specialInstructions] - Special instructions for the order.
 */

/**
 * @typedef {object} OrderAdvanceStateParameter
 * @property {string} [transition] - The specific state transition to execute (optional).
 */

/**
 * @typedef {object} OrderApplyCouponParameter
 * @property {string} couponCode - The coupon code to apply.
 */

/**
 * @typedef {object} OrderListItem
 * @property {string} id - The unique identifier of the order.
 * @property {string} number - The order number.
 * @property {string} state - The current state of the order (e.g., "Cart", "Pending", "Complete").
 * @property {string} [email] - The customer's email address.
 * @property {number} total - The total amount of the order.
 * @property {string} currency - The currency of the order.
 * @property {number} itemCount - The number of line items in the order.
 * @property {string} createdAt - Date and time when the order was created (ISO 8601).
 * @property {string} [userName] - The username associated with the order.
 */

/**
 * @typedef {OrderListItem & object} OrderDetail
 * @property {number} itemTotal - Total value of all line items.
 * @property {number} shipmentTotal - Total value of all shipments.
 * @property {number} adjustmentTotal - Total value of all adjustments (discounts, taxes).
 * @property {string} [specialInstructions] - Special instructions for the order.
 * @property {string} [completedAt] - Date and time when the order was completed (ISO 8601).
 * @property {string} [canceledAt] - Date and time when the order was canceled (ISO 8601).
 * @property {OrderLineItem[]} lineItems - List of line items in the order.
 * @property {OrderAdjustmentItem[]} adjustments - List of adjustments applied to the order.
 * @property {OrderShipmentItem[]} shipments - List of shipments for the order.
 * @property {OrderPaymentItem[]} payments - List of payments for the order.
 * @property {OrderHistoryItem[]} histories - List of order history events.
 */

/**
 * @typedef {object} OrderLineItem
 * @property {string} id - The unique identifier of the line item.
 * @property {string} variantId - The ID of the product variant.
 * @property {string} capturedName - The name of the product variant at the time of order.
 * @property {string} [capturedSku] - The SKU of the product variant at the time of order.
 * @property {number} quantity - The quantity of this line item.
 * @property {number} unitPrice - The price per unit of the variant.
 * @property {number} total - The total price for this line item.
 * @property {boolean} isPromotional - Indicates if this line item is part of a promotion.
 */

/**
 * @typedef {object} OrderAdjustmentItem
 * @property {string} id - The unique identifier of the adjustment.
 * @property {string} description - Description of the adjustment.
 * @property {number} amount - The amount of the adjustment.
 * @property {string} scope - The scope of the adjustment (e.g., "Order", "LineItem").
 * @property {boolean} eligible - Whether the adjustment is still eligible.
 * @property {boolean} isPromotion - Indicates if the adjustment is a promotion.
 */

/**
 * @typedef {object} OrderShipmentItem
 * @property {string} id - The unique identifier of the shipment.
 * @property {string} number - The shipment tracking number.
 * @property {string} state - The current state of the shipment (e.g., "Pending", "Shipped").
 * @property {string} [trackingNumber] - The tracking number provided by the carrier.
 * @property {string} stockLocationName - The name of the stock location from which the items were shipped.
 * @property {string} [shippedAt] - Date and time when the shipment was marked as shipped (ISO 8601).
 */

/**
 * @typedef {object} OrderPaymentItem
 * @property {string} id - The unique identifier of the payment.
 * @property {string} state - The current state of the payment (e.g., "Pending", "Authorized", "Captured").
 * @property {number} amount - The amount of the payment.
 * @property {string} paymentMethodType - The type of payment method used (e.g., "Credit Card", "Cash").
 * @property {string} [referenceTransactionId] - The transaction ID from the payment gateway.
 * @property {string} createdAt - Date and time when the payment was created (ISO 8601).
 */

/**
 * @typedef {object} OrderHistoryItem
 * @property {string} id - The unique identifier of the history entry.
 * @property {string} description - Description of the history event.
 * @property {string} [fromState] - The state before the change.
 * @property {string} toState - The state after the change.
 * @property {string} [triggeredBy] - Who triggered the event.
 * @property {string} createdAt - Date and time when the event occurred (ISO 8601).
 */
