// src/ReSys.Shop.Admin/src/stores/admin/inventories/stock-transfers/stock-transfer.store.js

import { defineStore } from 'pinia';
import { stockTransferService } from '@/services';

/**
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').StockTransferParameter} StockTransferParameter
 * @typedef {import('@/.js').StockTransferListItem} StockTransferListItem
 * @typedef {import('@/.js').StockTransferDetail} StockTransferDetail
 * @typedef {import('@/.js').ExecuteStockTransferParameter} ExecuteStockTransferParameter
 * @typedef {import('@/.js').ReceiveStockParameter} ReceiveStockParameter
 */

export const useStockTransferStore = defineStore('admin-stock-transfer', {
  state: () => ({
    /** @type {StockTransferListItem[]} */
    stockTransfers: [],
    /** @type {StockTransferDetail | null} */
    selectedStockTransfer: null,
    /** @type {boolean} */
    loading: false,
    /** @type {boolean} */
    error: false,
    /** @type {string | null} */
    errorMessage: null,
    /** @type {PaginationList<StockTransferListItem> | null} */
    pagedStockTransfers: null,
  }),
  actions: {
    /**
     * Fetches a paginated list of stock transfers.
     * @param {QueryableParams} [params={}]
     */
    async fetchPagedStockTransfers(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await stockTransferService.getPagedList(params);
        if (response.succeeded) {
          this.pagedStockTransfers = response.data;
          this.stockTransfers = response.data?.items || [];
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch stock transfers.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching paged stock transfers:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches details for a single stock transfer by ID.
     * @param {string} id
     */
    async fetchStockTransferById(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await stockTransferService.getById(id);
        if (response.succeeded) {
          this.selectedStockTransfer = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch stock transfer details.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching stock transfer by ID:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Creates a new stock transfer.
     * @param {StockTransferParameter} payload
     * @returns {Promise<boolean>}
     */
    async createStockTransfer(payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await stockTransferService.create(payload);
        if (response.succeeded) {
          this.fetchPagedStockTransfers(); // Refresh the list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to create stock transfer.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error creating stock transfer:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Updates an existing stock transfer.
     * @param {string} id
     * @param {StockTransferParameter} payload
     * @returns {Promise<boolean>}
     */
    async updateStockTransfer(id, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await stockTransferService.update(id, payload);
        if (response.succeeded) {
          this.fetchStockTransferById(id); // Refresh details
          this.fetchPagedStockTransfers(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to update stock transfer.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error updating stock transfer:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Deletes a stock transfer.
     * @param {string} id
     * @returns {Promise<boolean>}
     */
    async deleteStockTransfer(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await stockTransferService.delete(id);
        if (response.succeeded) {
          this.fetchPagedStockTransfers(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to delete stock transfer.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error deleting stock transfer:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Executes a stock transfer.
     * @param {string} id
     * @param {ExecuteStockTransferParameter} payload
     * @returns {Promise<boolean>}
     */
    async executeStockTransfer(id, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await stockTransferService.executeTransfer(id, payload);
        if (response.succeeded) {
          this.fetchStockTransferById(id); // Refresh details
          this.fetchPagedStockTransfers(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to execute stock transfer.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error executing stock transfer:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Receives stock for a transfer.
     * @param {string} id
     * @param {ReceiveStockParameter} payload
     * @returns {Promise<boolean>}
     */
    async receiveStockTransfer(id, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await stockTransferService.receiveStock(id, payload);
        if (response.succeeded) {
          this.fetchStockTransferById(id); // Refresh details
          this.fetchPagedStockTransfers(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to receive stock transfer.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error receiving stock transfer:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },
  },
});
