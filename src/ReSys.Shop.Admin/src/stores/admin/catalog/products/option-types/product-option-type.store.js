// src/ReSys.Shop.Admin/src/stores/admin/catalog/products/option-types/product-option-type.store.js

import { defineStore } from 'pinia';
import { productOptionTypeService } from '@/services';

/**
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').ProductOptionTypeParameter} ProductOptionTypeParameter
 * @typedef {import('@/.js').ProductOptionTypeResult} ProductOptionTypeResult
 */

export const useProductOptionTypeStore = defineStore('admin-product-option-type', {
  state: () => ({
    /** @type {ProductOptionTypeResult[]} */
    productOptionTypes: [],
    /** @type {boolean} */
    loading: false,
    /** @type {boolean} */
    error: false,
    /** @type {string | null} */
    errorMessage: null,
    /** @type {PaginationList<ProductOptionTypeResult> | null} */
    pagedProductOptionTypes: null,
  }),
  actions: {
    /**
     * Fetches product option types for a given product ID or based on query parameters.
     * @param {QueryableParams} [params={}] - Can include `productId`.
     */
    async fetchProductOptionTypes(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await productOptionTypeService.get(params);
        if (response.succeeded) {
          this.pagedProductOptionTypes = response.data;
          this.productOptionTypes = response.data?.items || [];
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch product option types.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching product option types:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Manages product option types (add/update/remove).
     * @param {string} productId - The ID of the product.
     * @param {{data: ProductOptionTypeParameter[]}} payload - Object containing an array of option type parameters.
     * @returns {Promise<boolean>}
     */
    async manageProductOptionTypes(productId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await productOptionTypeService.manage(productId, payload);
        if (response.succeeded) {
          this.fetchProductOptionTypes({ productId }); // Refresh option types for the product
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to manage product option types.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error managing product option types:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },
  },
});
