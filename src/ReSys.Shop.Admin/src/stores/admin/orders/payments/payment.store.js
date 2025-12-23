// src/ReSys.Shop.Admin/src/stores/admin/orders/payments/payment.store.js

import { defineStore } from 'pinia';
import { paymentService } from '@/services';

/**
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').PaymentCreateParameter} PaymentCreateParameter
 * @typedef {import('@/.js').PaymentAuthorizeParameter} PaymentAuthorizeParameter
 * @typedef {import('@/.js').PaymentCaptureParameter} PaymentCaptureParameter
 * @typedef {import('@/.js').PaymentRefundParameter} PaymentRefundParameter
 * @typedef {import('@/.js').PaymentListItem} PaymentListItem
 */

export const usePaymentStore = defineStore('admin-order-payment', {
  state: () => ({
    /** @type {PaymentListItem[]} */
    payments: [],
    /** @type {boolean} */
    loading: false,
    /** @type {boolean} */
    error: false,
    /** @type {string | null} */
    errorMessage: null,
    /** @type {PaginationList<PaymentListItem> | null} */
    pagedPayments: null, // Although annotations suggest List, using PaginationList for consistency
  }),
  actions: {
    /**
     * Fetches a list of payments for a specific order.
     * @param {string} orderId
     * @param {QueryableParams} [params={}]
     */
    async fetchPayments(orderId, params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await paymentService.getList(orderId, params);
        if (response.succeeded) {
          // Adjusting to fit PaginationList structure if the backend actually returns it
          // Otherwise, directly assign response.data to payments.
          this.pagedPayments = {
            items: response.data?.items || response.data || [],
            totalCount: response.data?.totalCount || (response.data ? response.data.length : 0),
            pageNumber: params.pageNumber || 0,
            pageSize: params.pageSize || 0,
          };
          this.payments = response.data?.items || response.data || [];
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch payments.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching payments:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Creates a new payment for an order.
     * @param {string} orderId
     * @param {PaymentCreateParameter} payload
     * @returns {Promise<boolean>}
     */
    async createPayment(orderId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await paymentService.create(orderId, payload);
        if (response.succeeded) {
          this.fetchPayments(orderId); // Refresh the list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to create payment.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error creating payment:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Authorizes a payment.
     * @param {string} orderId
     * @param {string} paymentId
     * @param {PaymentAuthorizeParameter} payload
     * @returns {Promise<boolean>}
     */
    async authorizePayment(orderId, paymentId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await paymentService.authorize(orderId, paymentId, payload);
        if (response.succeeded) {
          this.fetchPayments(orderId); // Refresh the list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to authorize payment.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error authorizing payment:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Captures a payment.
     * @param {string} orderId
     * @param {string} paymentId
     * @param {PaymentCaptureParameter} payload
     * @returns {Promise<boolean>}
     */
    async capturePayment(orderId, paymentId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await paymentService.capture(orderId, paymentId, payload);
        if (response.succeeded) {
          this.fetchPayments(orderId); // Refresh the list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to capture payment.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error capturing payment:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Refunds a payment.
     * @param {string} orderId
     * @param {string} paymentId
     * @param {PaymentRefundParameter} payload
     * @returns {Promise<boolean>}
     */
    async refundPayment(orderId, paymentId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await paymentService.refund(orderId, paymentId, payload);
        if (response.succeeded) {
          this.fetchPayments(orderId); // Refresh the list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to refund payment.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error refunding payment:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Voids a payment.
     * @param {string} orderId
     * @param {string} paymentId
     * @returns {Promise<boolean>}
     */
    async voidPayment(orderId, paymentId) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await paymentService.voidPayment(orderId, paymentId);
        if (response.succeeded) {
          this.fetchPayments(orderId); // Refresh the list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to void payment.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error voiding payment:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },
  },
});
