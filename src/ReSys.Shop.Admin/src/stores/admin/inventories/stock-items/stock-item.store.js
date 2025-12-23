import { defineStore } from 'pinia';
import { stockItemService } from '@/services';

/**
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').StockItemParameter} StockItemParameter
 * @typedef {import('@/.js').StockItemListItem} StockItemListItem
 * @typedef {import('@/.js').StockItemDetail} StockItemDetail
 * @typedef {import('@/.js').StockMovementItem} StockMovementItem
 * @typedef {import('@/.js').AdjustStockParameter} AdjustStockParameter
 * @typedef {import('@/.js').ReserveStockParameter} ReserveStockParameter
 * @typedef {import('@/.js').ReleaseStockParameter} ReleaseStockParameter
 */

export const useStockItemStore = defineStore('admin-stock-item', {
  state: () => ({
    /** @type {StockItemListItem[]} */
    stockItems: [],
    /** @type {StockItemDetail | null} */
    selectedStockItem: null,
    /** @type {boolean} */
    loading: false,
    /** @type {boolean} */
    error: false,
    /** @type {string | null} */
    errorMessage: null,
    /** @type {PaginationList<StockItemListItem> | null} */
    pagedStockItems: null,
    /** @type {PaginationList<StockMovementItem> | null} */
    stockMovements: null,
  }),
  actions: {
    /**
     * Fetches a paginated list of stock items.
     * @param {QueryableParams} [params={}]
     */
    async fetchPagedStockItems(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await stockItemService.getPagedList(params);
        if (response.succeeded) {
          this.pagedStockItems = response.data;
          this.stockItems = response.data?.items || [];
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch stock items.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching paged stock items:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches details for a single stock item by ID.
     * @param {string} id
     */
    async fetchStockItemById(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await stockItemService.getById(id);
        if (response.succeeded) {
          this.selectedStockItem = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch stock item details.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching stock item by ID:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Creates a new stock item.
     * @param {StockItemParameter} payload
     * @returns {Promise<boolean>}
     */
    async createStockItem(payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await stockItemService.create(payload);
        if (response.succeeded) {
          this.fetchPagedStockItems(); // Refresh the list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to create stock item.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error creating stock item:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Updates an existing stock item.
     * @param {string} id
     * @param {StockItemParameter} payload
     * @returns {Promise<boolean>}
     */
    async updateStockItem(id, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await stockItemService.update(id, payload);
        if (response.succeeded) {
          this.fetchStockItemById(id); // Refresh details
          this.fetchPagedStockItems(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to update stock item.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error updating stock item:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Deletes a stock item.
     * @param {string} id
     * @returns {Promise<boolean>}
     */
    async deleteStockItem(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await stockItemService.delete(id);
        if (response.succeeded) {
          this.fetchPagedStockItems(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to delete stock item.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error deleting stock item:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Adjusts the stock quantity for a stock item.
     * @param {string} id
     * @param {AdjustStockParameter} payload
     * @returns {Promise<boolean>}
     */
    async adjustStock(id, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await stockItemService.adjust(id, payload);
        if (response.succeeded) {
          this.fetchStockItemById(id); // Refresh details
          this.fetchPagedStockItems(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to adjust stock quantity.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error adjusting stock quantity:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Reserves stock for an order.
     * @param {string} id
     * @param {ReserveStockParameter} payload
     * @returns {Promise<boolean>}
     */
    async reserveStock(id, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await stockItemService.reserve(id, payload);
        if (response.succeeded) {
          this.fetchStockItemById(id); // Refresh details
          this.fetchPagedStockItems(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to reserve stock.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error reserving stock:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Releases previously reserved stock.
     * @param {string} id
     * @param {ReleaseStockParameter} payload
     * @returns {Promise<boolean>}
     */
    async releaseStock(id, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await stockItemService.release(id, payload);
        if (response.succeeded) {
          this.fetchStockItemById(id); // Refresh details
          this.fetchPagedStockItems(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to release reserved stock.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error releasing reserved stock:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches stock movement history for a stock item.
     * @param {string} id
     * @param {QueryableParams} [params={}]
     */
    async fetchStockMovements(id, params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await stockItemService.getMovements(id, params);
        if (response.succeeded) {
          this.stockMovements = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch stock movements.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching stock movements:', err);
      } finally {
        this.loading = false;
      }
    },
  },
});
