// src/ReSys.Shop.Admin/src/stores/admin/catalog/products/classifications/product-classification.store.js

import { defineStore } from 'pinia';
import { productClassificationService } from '@/services';

/**
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').ProductClassificationParameter} ProductClassificationParameter
 * @typedef {import('@/.js').ProductClassificationResult} ProductClassificationResult
 */

export const useProductClassificationStore = defineStore('admin-product-classification', {
  state: () => ({
    /** @type {ProductClassificationResult[]} */
    productClassifications: [],
    /** @type {boolean} */
    loading: false,
    /** @type {boolean} */
    error: false,
    /** @type {string | null} */
    errorMessage: null,
    /** @type {PaginationList<ProductClassificationResult> | null} */
    pagedProductClassifications: null,
  }),
  actions: {
    /**
     * Fetches product classifications for a given product ID or based on query parameters.
     * @param {QueryableParams} [params={}] - Can include `productId` and `taxonId`.
     */
    async fetchProductClassifications(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await productClassificationService.get(params);
        if (response.succeeded) {
          this.pagedProductClassifications = response.data;
          this.productClassifications = response.data?.items || [];
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch product classifications.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching product classifications:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Manages product classifications (add/update/remove).
     * @param {string} productId - The ID of the product.
     * @param {{data: ProductClassificationParameter[]}} payload - Object containing an array of classification parameters.
     * @returns {Promise<boolean>}
     */
    async manageProductClassifications(productId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await productClassificationService.manage(productId, payload);
        if (response.succeeded) {
          this.fetchProductClassifications({ productId }); // Refresh classifications for the product
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to manage product classifications.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error managing product classifications:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },
  },
});
