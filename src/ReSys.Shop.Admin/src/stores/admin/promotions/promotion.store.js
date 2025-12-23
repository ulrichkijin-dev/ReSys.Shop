// src/ReSys.Shop.Admin/src/stores/admin/promotions/promotion.store.js

import { defineStore } from 'pinia';
import { promotionService } from '@/services';

/**
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').PromotionParameter} PromotionParameter
 * @typedef {import('@/.js').PromotionSelectItem} PromotionSelectItem
 * @typedef {import('@/.js').PromotionListItem} PromotionListItem
 * @typedef {import('@/.js').PromotionDetail} PromotionDetail
 */

export const usePromotionStore = defineStore('admin-promotion', {
  state: () => ({
    /** @type {PromotionListItem[]} */
    promotions: [],
    /** @type {PromotionDetail | null} */
    selectedPromotion: null,
    /** @type {boolean} */
    loading: false,
    /** @type {boolean} */
    error: false,
    /** @type {string | null} */
    errorMessage: null,
    /** @type {PaginationList<PromotionListItem> | null} */
    pagedPromotions: null,
    /** @type {PaginationList<PromotionSelectItem> | null} */
    selectPromotions: null,
  }),
  actions: {
    /**
     * Fetches a paginated list of promotions.
     * @param {QueryableParams} [params={}]
     */
    async fetchPagedPromotions(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await promotionService.getPagedList(params);
        if (response.succeeded) {
          this.pagedPromotions = response.data;
          this.promotions = response.data?.items || [];
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch promotions.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching paged promotions:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches a select list of promotions.
     * @param {QueryableParams} [params={}]
     */
    async fetchSelectPromotions(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await promotionService.getSelectList(params);
        if (response.succeeded) {
          this.selectPromotions = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch select promotions.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching select promotions:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches details for a single promotion by ID.
     * @param {string} id
     */
    async fetchPromotionById(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await promotionService.getById(id);
        if (response.succeeded) {
          this.selectedPromotion = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch promotion details.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching promotion by ID:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Creates a new promotion.
     * @param {PromotionParameter} payload
     * @returns {Promise<boolean>}
     */
    async createPromotion(payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await promotionService.create(payload);
        if (response.succeeded) {
          this.fetchPagedPromotions(); // Refresh the list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to create promotion.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error creating promotion:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Updates an existing promotion.
     * @param {string} id
     * @param {PromotionParameter} payload
     * @returns {Promise<boolean>}
     */
    async updatePromotion(id, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await promotionService.update(id, payload);
        if (response.succeeded) {
          this.fetchPromotionById(id); // Refresh details
          this.fetchPagedPromotions(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to update promotion.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error updating promotion:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Deletes a promotion.
     * @param {string} id
     * @returns {Promise<boolean>}
     */
    async deletePromotion(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await promotionService.delete(id);
        if (response.succeeded) {
          this.fetchPagedPromotions(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to delete promotion.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error deleting promotion:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Activates an inactive promotion.
     * @param {string} id
     * @returns {Promise<boolean>}
     */
    async activatePromotion(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await promotionService.activate(id);
        if (response.succeeded) {
          this.fetchPromotionById(id); // Refresh details
          this.fetchPagedPromotions(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to activate promotion.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error activating promotion:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Deactivates an active promotion.
     * @param {string} id
     * @returns {Promise<boolean>}
     */
    async deactivatePromotion(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await promotionService.deactivate(id);
        if (response.succeeded) {
          this.fetchPromotionById(id); // Refresh details
          this.fetchPagedPromotions(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to deactivate promotion.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error deactivating promotion:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Validates promotion configuration and rules.
     * @param {string} id
     * @returns {Promise<boolean>}
     */
    async validatePromotion(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await promotionService.validate(id);
        if (response.succeeded) {
          // Validation does not typically update the store state for the item itself
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Promotion validation failed.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred during promotion validation.';
        console.error('Error validating promotion:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },
  },
});
