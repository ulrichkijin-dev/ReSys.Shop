// src/ReSys.Shop.Admin/src/stores/admin/inventories/stock-locations/stock-location.store.js

import { defineStore } from 'pinia';
import { stockLocationService } from '@/services';

/**
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').StockLocationParameter} StockLocationParameter
 * @typedef {import('@/.js').StockLocationSelectItem} StockLocationSelectItem
 * @typedef {import('@/.js').StockLocationListItem} StockLocationListItem
 * @typedef {import('@/.js').StockLocationDetail} StockLocationDetail
 */

export const useStockLocationStore = defineStore('admin-stock-location', {
  state: () => ({
    /** @type {StockLocationListItem[]} */
    stockLocations: [],
    /** @type {StockLocationDetail | null} */
    selectedStockLocation: null,
    /** @type {boolean} */
    loading: false,
    /** @type {boolean} */
    error: false,
    /** @type {string | null} */
    errorMessage: null,
    /** @type {PaginationList<StockLocationListItem> | null} */
    pagedStockLocations: null,
    /** @type {PaginationList<StockLocationSelectItem> | null} */
    selectStockLocations: null,
  }),
  actions: {
    /**
     * Fetches a paginated list of stock locations.
     * @param {QueryableParams} [params={}]
     */
    async fetchPagedStockLocations(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await stockLocationService.getPagedList(params);
        if (response.succeeded) {
          this.pagedStockLocations = response.data;
          this.stockLocations = response.data?.items || [];
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch stock locations.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching paged stock locations:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches a select list of stock locations.
     * @param {QueryableParams} [params={}]
     */
    async fetchSelectStockLocations(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await stockLocationService.getSelectList(params);
        if (response.succeeded) {
          this.selectStockLocations = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch select stock locations.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching select stock locations:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches details for a single stock location by ID.
     * @param {string} id
     */
    async fetchStockLocationById(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await stockLocationService.getById(id);
        if (response.succeeded) {
          this.selectedStockLocation = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch stock location details.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching stock location by ID:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Creates a new stock location.
     * @param {StockLocationParameter} payload
     * @returns {Promise<boolean>}
     */
    async createStockLocation(payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await stockLocationService.create(payload);
        if (response.succeeded) {
          this.fetchPagedStockLocations(); // Refresh the list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to create stock location.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error creating stock location:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Updates an existing stock location.
     * @param {string} id
     * @param {StockLocationParameter} payload
     * @returns {Promise<boolean>}
     */
    async updateStockLocation(id, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await stockLocationService.update(id, payload);
        if (response.succeeded) {
          this.fetchStockLocationById(id); // Refresh details
          this.fetchPagedStockLocations(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to update stock location.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error updating stock location:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Deletes a stock location.
     * @param {string} id
     * @returns {Promise<boolean>}
     */
    async deleteStockLocation(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await stockLocationService.delete(id);
        if (response.succeeded) {
          this.fetchPagedStockLocations(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to delete stock location.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error deleting stock location:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Restores a soft-deleted stock location.
     * @param {string} id
     * @returns {Promise<boolean>}
     */
    async restoreStockLocation(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await stockLocationService.restore(id);
        if (response.succeeded) {
          this.fetchPagedStockLocations(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to restore stock location.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error restoring stock location:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },
  },
});
