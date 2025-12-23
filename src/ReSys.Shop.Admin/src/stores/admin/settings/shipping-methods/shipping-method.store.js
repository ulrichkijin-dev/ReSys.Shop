// src/ReSys.Shop.Admin/src/stores/admin/settings/shipping-methods/shipping-method.store.js

import { defineStore } from 'pinia';
import { shippingMethodService } from '@/services';

/**
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').ShippingMethodParameter} ShippingMethodParameter
 * @typedef {import('@/.js').ShippingMethodSelectItem} ShippingMethodSelectItem
 * @typedef {import('@/.js').ShippingMethodListItem} ShippingMethodListItem
 * @typedef {import('@/.js').ShippingMethodDetail} ShippingMethodDetail
 */

export const useShippingMethodStore = defineStore('admin-shipping-method', {
  state: () => ({
    /** @type {ShippingMethodListItem[]} */
    shippingMethods: [],
    /** @type {ShippingMethodDetail | null} */
    selectedShippingMethod: null,
    /** @type {boolean} */
    loading: false,
    /** @type {boolean} */
    error: false,
    /** @type {string | null} */
    errorMessage: null,
    /** @type {PaginationList<ShippingMethodListItem> | null} */
    pagedShippingMethods: null,
    /** @type {PaginationList<ShippingMethodSelectItem> | null} */
    selectShippingMethods: null,
  }),
  actions: {
    /**
     * Fetches a paginated list of shipping methods.
     * @param {QueryableParams} [params={}]
     */
    async fetchPagedShippingMethods(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await shippingMethodService.getPagedList(params);
        if (response.succeeded) {
          this.pagedShippingMethods = response.data;
          this.shippingMethods = response.data?.items || [];
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch shipping methods.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching paged shipping methods:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches a select list of shipping methods.
     * @param {QueryableParams} [params={}]
     */
    async fetchSelectShippingMethods(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await shippingMethodService.getSelectList(params);
        if (response.succeeded) {
          this.selectShippingMethods = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch select shipping methods.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching select shipping methods:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches details for a single shipping method by ID.
     * @param {string} id
     */
    async fetchShippingMethodById(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await shippingMethodService.getById(id);
        if (response.succeeded) {
          this.selectedShippingMethod = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch shipping method details.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching shipping method by ID:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Creates a new shipping method.
     * @param {ShippingMethodParameter} payload
     * @returns {Promise<boolean>}
     */
    async createShippingMethod(payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await shippingMethodService.create(payload);
        if (response.succeeded) {
          this.fetchPagedShippingMethods(); // Refresh the list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to create shipping method.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error creating shipping method:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Updates an existing shipping method.
     * @param {string} id
     * @param {ShippingMethodParameter} payload
     * @returns {Promise<boolean>}
     */
    async updateShippingMethod(id, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await shippingMethodService.update(id, payload);
        if (response.succeeded) {
          this.fetchShippingMethodById(id); // Refresh details
          this.fetchPagedShippingMethods(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to update shipping method.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error updating shipping method:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Deletes a shipping method.
     * @param {string} id
     * @returns {Promise<boolean>}
     */
    async deleteShippingMethod(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await shippingMethodService.delete(id);
        if (response.succeeded) {
          this.fetchPagedShippingMethods(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to delete shipping method.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error deleting shipping method:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Activates a shipping method.
     * @param {string} id
     * @returns {Promise<boolean>}
     */
    async activateShippingMethod(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await shippingMethodService.activate(id);
        if (response.succeeded) {
          this.fetchShippingMethodById(id); // Refresh details
          this.fetchPagedShippingMethods(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to activate shipping method.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error activating shipping method:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Deactivates a shipping method.
     * @param {string} id
     * @returns {Promise<boolean>}
     */
    async deactivateShippingMethod(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await shippingMethodService.deactivate(id);
        if (response.succeeded) {
          this.fetchShippingMethodById(id); // Refresh details
          this.fetchPagedShippingMethods(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to deactivate shipping method.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error deactivating shipping method:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },
  },
});
