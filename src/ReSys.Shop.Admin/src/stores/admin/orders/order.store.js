// src/ReSys.Shop.Admin/src/stores/admin/orders/order.store.js

import { defineStore } from 'pinia';
import { orderService } from '@/services';

/**
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').OrderListItem} OrderListItem
 * @typedef {import('@/.js').OrderDetail} OrderDetail
 * @typedef {import('@/.js').OrderCoupon} OrderCoupon
 * @typedef {import('@/.js').OrderCreateRequest} OrderCreateRequest
 * @typedef {import('@/.js').OrderUpdateRequest} OrderUpdateRequest
 * @typedef {import('@/.js').ApplyCouponRequest} ApplyCouponRequest
 * @typedef {import('@/.js').ShipmentItem} ShipmentItem
 * @typedef {import('@/.js').ShipmentCreateRequest} ShipmentCreateRequest
 * @typedef {import('@/.js').PaymentItem} PaymentItem
 * @typedef {import('@/.js').PaymentCreateRequest} PaymentCreateRequest
 * @typedef {import('@/.js').PaymentAuthorizeRequest} PaymentAuthorizeRequest
 * @typedef {import('@/.js').PaymentCaptureRequest} PaymentCaptureRequest
 * @typedef {import('@/.js').PaymentRefundRequest} PaymentRefundRequest
 */

export const useOrderStore = defineStore('admin-order', {
  state: () => ({
    /** @type {OrderListItem[]} */
    orders: [],
    /** @type {OrderDetail | null} */
    selectedOrder: null,
    /** @type {OrderCoupon[] | null} */
    appliedCoupons: null,
    /** @type {ShipmentItem[] | null} */
    orderShipments: null,
    /** @type {PaymentItem[] | null} */
    orderPayments: null,
    /** @type {boolean} */
    loading: false,
    /** @type {boolean} */
    error: false,
    /** @type {string | null} */
    errorMessage: null,
    /** @type {PaginationList<OrderListItem> | null} */
    pagedOrders: null,
  }),
  actions: {
    /**
     * Fetches a paginated list of orders.
     * @param {QueryableParams} [params={}]
     */
    async fetchPagedOrders(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await orderService.getPagedList(params);
        if (response.succeeded) {
          this.pagedOrders = response.data;
          this.orders = response.data?.items || [];
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch orders.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching paged orders:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Creates a new order.
     * @param {OrderCreateRequest} payload
     * @returns {Promise<boolean>}
     */
    async createOrder(payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await orderService.create(payload);
        if (response.succeeded) {
          this.fetchPagedOrders();
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to create order.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error creating order:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches details for a single order by ID.
     * @param {string} id
     */
    async fetchOrderById(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await orderService.getById(id);
        if (response.succeeded) {
          this.selectedOrder = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch order details.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching order by ID:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Deletes an order by ID.
     * @param {string} id
     * @returns {Promise<boolean>}
     */
    async deleteOrder(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await orderService.delete(id);
        if (response.succeeded) {
          this.fetchPagedOrders();
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to delete order.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error deleting order:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Updates an order by ID.
     * @param {string} id
     * @param {OrderUpdateRequest} payload
     * @returns {Promise<boolean>}
     */
    async updateOrder(id, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await orderService.update(id, payload);
        if (response.succeeded) {
          await this.fetchOrderById(id);
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to update order.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error updating order:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Advances the order state.
     * @param {string} id
     * @returns {Promise<boolean>}
     */
    async advanceOrderState(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await orderService.advanceOrder(id);
        if (response.succeeded) {
          await this.fetchOrderById(id);
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to advance order state.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error advancing order state:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Moves the order to the next state.
     * @param {string} id
     * @returns {Promise<boolean>}
     */
    async moveOrderToNextState(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await orderService.nextOrderState(id);
        if (response.succeeded) {
          await this.fetchOrderById(id);
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to move order to next state.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error moving order to next state:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Completes an order.
     * @param {string} id
     * @returns {Promise<boolean>}
     */
    async completeOrder(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await orderService.completeOrder(id);
        if (response.succeeded) {
          await this.fetchOrderById(id);
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to complete order.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error completing order:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Empties the order cart.
     * @param {string} id
     * @returns {Promise<boolean>}
     */
    async emptyOrder(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await orderService.emptyOrder(id);
        if (response.succeeded) {
          await this.fetchOrderById(id);
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to empty order.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error emptying order:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Approves an order.
     * @param {string} id
     * @returns {Promise<boolean>}
     */
    async approveOrder(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await orderService.approveOrder(id);
        if (response.succeeded) {
          await this.fetchOrderById(id);
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to approve order.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error approving order:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Cancels an order.
     * @param {string} id
     * @returns {Promise<boolean>}
     */
    async cancelOrder(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await orderService.cancelOrder(id);
        if (response.succeeded) {
          await this.fetchPagedOrders(); // Refresh the list, as canceled orders might be filtered differently
          await this.fetchOrderById(id);
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to cancel order.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error canceling order:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Applies a coupon code to the order.
     * @param {string} id
     * @param {ApplyCouponRequest} payload
     * @returns {Promise<boolean>}
     */
    async applyOrderCoupon(id, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await orderService.applyCoupon(id, payload);
        if (response.succeeded) {
          await this.fetchOrderById(id);
          await this.fetchAppliedCoupons(id);
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to apply coupon.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error applying coupon:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches applied coupons for a specific order by ID.
     * @param {string} id
     */
    async fetchAppliedCoupons(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await orderService.getCoupons(id);
        if (response.succeeded) {
          this.appliedCoupons = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch applied coupons.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching applied coupons:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Removes an applied coupon from a specific order by ID.
     * @param {string} id
     * @returns {Promise<boolean>}
     */
    async removeOrderCoupon(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await orderService.removeAppliedCoupon(id);
        if (response.succeeded) {
          await this.fetchOrderById(id);
          await this.fetchAppliedCoupons(id);
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to remove applied coupon.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error removing applied coupon:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    // --- Shipment Actions ---
    /**
     * Fetches all shipments associated with an order.
     * @param {string} orderId
     */
    async fetchOrderShipments(orderId) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await orderService.getShipments(orderId);
        if (response.succeeded) {
          this.orderShipments = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch order shipments.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching order shipments:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Creates a new shipment for the order.
     * @param {string} orderId
     * @param {ShipmentCreateRequest} payload
     * @returns {Promise<boolean>}
     */
    async createOrderShipment(orderId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await orderService.createShipment(orderId, payload);
        if (response.succeeded) {
          await this.fetchOrderShipments(orderId); // Refresh shipments list
          await this.fetchOrderById(orderId); // Refresh order details to reflect shipment changes
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to create order shipment.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error creating order shipment:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Deletes a shipment.
     * @param {string} orderId
     * @param {string} shipmentId
     * @returns {Promise<boolean>}
     */
    async deleteOrderShipment(orderId, shipmentId) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await orderService.deleteShipment(orderId, shipmentId);
        if (response.succeeded) {
          await this.fetchOrderShipments(orderId); // Refresh shipments list
          await this.fetchOrderById(orderId); // Refresh order details
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to delete order shipment.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error deleting order shipment:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    // --- Payment Actions ---
    /**
     * Fetches all payments associated with an order.
     * @param {string} orderId
     */
    async fetchOrderPayments(orderId) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await orderService.getPayments(orderId);
        if (response.succeeded) {
          this.orderPayments = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch order payments.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching order payments:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Adds a new payment record to the order.
     * @param {string} orderId
     * @param {PaymentCreateRequest} payload
     * @returns {Promise<boolean>}
     */
    async createOrderPayment(orderId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await orderService.createPayment(orderId, payload);
        if (response.succeeded) {
          await this.fetchOrderPayments(orderId); // Refresh payments list
          await this.fetchOrderById(orderId); // Refresh order details
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to create order payment.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error creating order payment:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Marks a payment as authorized.
     * @param {string} orderId
     * @param {string} paymentId
     * @param {PaymentAuthorizeRequest} payload
     * @returns {Promise<boolean>}
     */
    async authorizeOrderPayment(orderId, paymentId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await orderService.authorizePayment(orderId, paymentId, payload);
        if (response.succeeded) {
          await this.fetchOrderPayments(orderId); // Refresh payments list
          await this.fetchOrderById(orderId); // Refresh order details
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to authorize order payment.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error authorizing order payment:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Marks a payment as captured.
     * @param {string} orderId
     * @param {string} paymentId
     * @param {PaymentCaptureRequest} payload
     * @returns {Promise<boolean>}
     */
    async captureOrderPayment(orderId, paymentId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await orderService.capturePayment(orderId, paymentId, payload);
        if (response.succeeded) {
          await this.fetchOrderPayments(orderId); // Refresh payments list
          await this.fetchOrderById(orderId); // Refresh order details
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to capture order payment.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error capturing order payment:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Records a refund for a captured payment.
     * @param {string} orderId
     * @param {string} paymentId
     * @param {PaymentRefundRequest} payload
     * @returns {Promise<boolean>}
     */
    async refundOrderPayment(orderId, paymentId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await orderService.refundPayment(orderId, paymentId, payload);
        if (response.succeeded) {
          await this.fetchOrderPayments(orderId); // Refresh payments list
          await this.fetchOrderById(orderId); // Refresh order details
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to refund order payment.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error refunding order payment:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Voids an authorized but not yet captured payment.
     * @param {string} orderId
     * @param {string} paymentId
     * @returns {Promise<boolean>}
     */
    async voidOrderPayment(orderId, paymentId) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await orderService.voidPayment(orderId, paymentId);
        if (response.succeeded) {
          await this.fetchOrderPayments(orderId); // Refresh payments list
          await this.fetchOrderById(orderId); // Refresh order details
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to void order payment.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error voiding order payment:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },
  },
});
