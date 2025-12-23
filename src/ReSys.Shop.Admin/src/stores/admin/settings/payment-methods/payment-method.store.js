// src/ReSys.Shop.Admin/src/stores/admin/settings/payment-methods/payment-method.store.js

import { defineStore } from 'pinia';
import { paymentMethodService } from '@/services';

/**
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').PaymentMethodParameter} PaymentMethodParameter
 * @typedef {import('@/.js').PaymentMethodSelectItem} PaymentMethodSelectItem
 * @typedef {import('@/.js').PaymentMethodListItem} PaymentMethodListItem
 * @typedef {import('@/.js').PaymentMethodDetail} PaymentMethodDetail
 */

export const usePaymentMethodStore = defineStore('admin-payment-method', {
  state: () => ({
    /** @type {PaymentMethodListItem[]} */
    paymentMethods: [],
    /** @type {PaymentMethodDetail | null} */
    selectedPaymentMethod: null,
    /** @type {boolean} */
    loading: false,
    /** @type {boolean} */
    error: false,
    /** @type {string | null} */
    errorMessage: null,
    /** @type {PaginationList<PaymentMethodListItem> | null} */
    pagedPaymentMethods: null,
    /** @type {PaginationList<PaymentMethodSelectItem> | null} */
    selectPaymentMethods: null,
  }),
  actions: {
    /**
     * Fetches a paginated list of payment methods.
     * @param {QueryableParams} [params={}]
     */
    async fetchPagedPaymentMethods(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await paymentMethodService.getPagedList(params);
        if (response.succeeded) {
          this.pagedPaymentMethods = response.data;
          this.paymentMethods = response.data?.items || [];
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch payment methods.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching paged payment methods:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches a select list of payment methods.
     * @param {QueryableParams} [params={}]
     */
    async fetchSelectPaymentMethods(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await paymentMethodService.getSelectList(params);
        if (response.succeeded) {
          this.selectPaymentMethods = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch select payment methods.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching select payment methods:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches details for a single payment method by ID.
     * @param {string} id
     */
    async fetchPaymentMethodById(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await paymentMethodService.getById(id);
        if (response.succeeded) {
          this.selectedPaymentMethod = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch payment method details.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching payment method by ID:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Creates a new payment method.
     * @param {PaymentMethodParameter} payload
     * @returns {Promise<boolean>}
     */
    async createPaymentMethod(payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await paymentMethodService.create(payload);
        if (response.succeeded) {
          this.fetchPagedPaymentMethods(); // Refresh the list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to create payment method.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error creating payment method:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Updates an existing payment method.
     * @param {string} id
     * @param {PaymentMethodParameter} payload
     * @returns {Promise<boolean>}
     */
    async updatePaymentMethod(id, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await paymentMethodService.update(id, payload);
        if (response.succeeded) {
          this.fetchPaymentMethodById(id); // Refresh details
          this.fetchPagedPaymentMethods(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to update payment method.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error updating payment method:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Deletes a payment method.
     * @param {string} id
     * @returns {Promise<boolean>}
     */
    async deletePaymentMethod(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await paymentMethodService.delete(id);
        if (response.succeeded) {
          this.fetchPagedPaymentMethods(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to delete payment method.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error deleting payment method:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Restores a soft-deleted payment method.
     * @param {string} id
     * @returns {Promise<boolean>}
     */
    async restorePaymentMethod(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await paymentMethodService.restore(id);
        if (response.succeeded) {
          this.fetchPagedPaymentMethods(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to restore payment method.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error restoring payment method:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },
  },
});
