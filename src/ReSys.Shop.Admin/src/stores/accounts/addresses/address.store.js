// src/ReSys.Shop.Admin/src/stores/accounts/address.store.js

import { defineStore } from 'pinia';
import { addressService } from '@/services';

/**
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/.js').AddressListItem} AddressListItem
 * @typedef {import('@/.js').AddressDetail} AddressDetail
 * @typedef {import('@/.js').AddressParam} AddressParam
 * @typedef {import('@/.js').AddressSelectItem} AddressSelectItem
 */

export const useAddressStore = defineStore('address', {
  state: () => ({
    /** @type {AddressListItem[]} */
    addresses: [],
    /** @type {AddressDetail | null} */
    selectedAddress: null,
    /** @type {boolean} */
    loading: false,
    /** @type {boolean} */
    error: false,
    /** @type {string | null} */
    errorMessage: null,
    /** @type {import('@/models/common/common.model').PaginationList<AddressListItem> | null} */
    pagedAddresses: null,
    /** @type {import('@/models/common/common.model').PaginationList<AddressSelectItem> | null} */
    selectAddresses: null,
  }),
  getters: {
    // You can add getters here if needed, e.g., filtered addresses
    /** @param {import('@/models/common/common.model').PaginationList<AddressListItem>} state */
    // getActiveAddresses: (state) => state.addresses.filter(addr => addr.isActive),
  },
  actions: {
    /**
     * Fetches a paginated list of addresses.
     * @param {QueryableParams} [params={}]
     */
    async fetchPagedAddresses(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await addressService.getPagedList(params);
        if (response.succeeded) {
          this.pagedAddresses = response.data;
          this.addresses = response.data?.items || [];
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch addresses.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching paged addresses:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches a select list of addresses.
     * @param {QueryableParams} [params={}]
     */
    async fetchSelectAddresses(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await addressService.getSelectList(params);
        if (response.succeeded) {
          this.selectAddresses = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch select addresses.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching select addresses:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches details for a single address by ID.
     * @param {string} id
     */
    async fetchAddressById(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await addressService.getById(id);
        if (response.succeeded) {
          this.selectedAddress = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch address details.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching address by ID:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Creates a new address.
     * @param {AddressParam} payload
     * @returns {Promise<boolean>} - True if successful, false otherwise.
     */
    async createAddress(payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await addressService.create(payload);
        if (response.succeeded) {
          // Optionally refresh the list or add the new item to state
          this.fetchPagedAddresses(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to create address.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error creating address:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Updates an existing address.
     * @param {string} id
     * @param {AddressParam} payload
     * @returns {Promise<boolean>} - True if successful, false otherwise.
     */
    async updateAddress(id, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await addressService.update(id, payload);
        if (response.succeeded) {
          // Optionally refresh the selected address or the list
          this.fetchAddressById(id); // Refresh details
          this.fetchPagedAddresses(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to update address.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error updating address:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Deletes an address.
     * @param {string} id
     * @returns {Promise<boolean>} - True if successful, false otherwise.
     */
    async deleteAddress(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await addressService.delete(id);
        if (response.succeeded) {
          // Remove from state or refresh list
          this.addresses = this.addresses.filter(addr => addr.id !== id);
          this.fetchPagedAddresses(); // Ensure pagination is updated
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to delete address.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error deleting address:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },
  },
});
