// src/ReSys.Shop.Admin/src/stores/admin/catalog/variants/variant.store.js

import { defineStore } from 'pinia';
import { variantService } from '@/services';

/**
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').VariantParameter} VariantParameter
 * @typedef {import('@/.js').VariantSelectItem} VariantSelectItem
 * @typedef {import('@/.js').VariantListItem} VariantListItem
 * @typedef {import('@/.js').VariantDetail} VariantDetail
 * @typedef {import('@/.js').VariantPriceItem} VariantPriceItem
 * @typedef {import('@/.js').VariantPriceSetRequest} VariantPriceParameter
 */

export const useVariantStore = defineStore('admin-variant', {
  state: () => ({
    /** @type {VariantListItem[]} */
    variants: [],
    /** @type {VariantDetail | null} */
    selectedVariant: null,
    /** @type {boolean} */
    loading: false,
    /** @type {boolean} */
    error: false,
    /** @type {string | null} */
    errorMessage: null,
    /** @type {PaginationList<VariantListItem> | null} */
    pagedVariants: null,
    /** @type {PaginationList<VariantSelectItem> | null} */
    selectVariants: null,
    /** @type {PaginationList<VariantPriceItem> | null} */
    variantPrices: null,
  }),
  actions: {
    /**
     * Fetches a paginated list of variants.
     * @param {QueryableParams} [params={}]
     */
    async fetchPagedVariants(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await variantService.getPagedList(params);
        if (response.succeeded) {
          this.pagedVariants = response.data;
          this.variants = response.data?.items || [];
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch variants.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching paged variants:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches a select list of variants.
     * @param {QueryableParams} [params={}]
     */
    async fetchSelectVariants(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await variantService.getSelectList(params);
        if (response.succeeded) {
          this.selectVariants = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch select variants.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching select variants:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches details for a single variant by ID.
     * @param {string} id
     */
    async fetchVariantById(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await variantService.getById(id);
        if (response.succeeded) {
          this.selectedVariant = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch variant details.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching variant by ID:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Creates a new variant.
     * @param {VariantParameter} payload
     * @returns {Promise<boolean>}
     */
    async createVariant(payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await variantService.create(payload);
        if (response.succeeded) {
          this.fetchPagedVariants(); // Refresh the list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to create variant.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error creating variant:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Updates an existing variant.
     * @param {string} id
     * @param {VariantParameter} payload
     * @returns {Promise<boolean>}
     */
    async updateVariant(id, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await variantService.update(id, payload);
        if (response.succeeded) {
          this.fetchVariantById(id); // Refresh details
          this.fetchPagedVariants(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to update variant.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error updating variant:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Deletes a variant.
     * @param {string} id
     * @returns {Promise<boolean>}
     */
    async deleteVariant(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await variantService.delete(id);
        if (response.succeeded) {
          this.fetchPagedVariants(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to delete variant.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error deleting variant:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Discontinues a variant.
     * @param {string} id
     * @returns {Promise<boolean>}
     */
    async discontinueVariant(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await variantService.discontinue(id);
        if (response.succeeded) {
          this.fetchPagedVariants(); // Refresh list to reflect status change
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to discontinue variant.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error discontinuing variant:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches all prices for a specific variant.
     * @param {string} variantId
     * @param {QueryableParams} [params={}]
     */
    async fetchVariantPrices(variantId, params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await variantService.getPrices(variantId, params);
        if (response.succeeded) {
          this.variantPrices = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch variant prices.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error(`Error fetching prices for variant ${variantId}:`, err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Sets the price for a variant.
     * @param {string} variantId
     * @param {VariantPriceParameter} payload
     * @returns {Promise<boolean>}
     */
    async setVariantPrice(variantId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await variantService.setPrice(variantId, payload);
        if (response.succeeded) {
          this.fetchVariantPrices(variantId); // Refresh prices for the variant
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to set variant price.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error(`Error setting price for variant ${variantId}:`, err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches option values for a specific variant.
     * Note: Option values are typically part of the VariantDetail object.
     * This action assumes `selectedVariant` has been fetched via `fetchVariantById`.
     * If a dedicated API for fetching *only* option values is exposed, it should be used here.
     * For now, it will extract from `selectedVariant`.
     * @returns {string[] | null} An array of option value names.
     */
    getVariantOptionValues() {
      if (this.selectedVariant) {
        return this.selectedVariant.optionValueNames;
      }
      return null;
    },

    /**
     * Updates the option values associated with a variant.
     * @param {string} variantId
     * @param {{optionValueIds: string[]}} payload
     * @returns {Promise<boolean>}
     */
    async updateVariantOptionValues(variantId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await variantService.updateOptionValues(variantId, payload);
        if (response.succeeded) {
          this.fetchVariantById(variantId); // Refresh variant details to get updated option values
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to update variant option values.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error(`Error updating option values for variant ${variantId}:`, err);
        return false;
      } finally {
        this.loading = false;
      }
    },
  },
});
