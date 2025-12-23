// src/ReSys.Shop.Admin/src/stores/admin/catalog/products/property-types/product-property-type.store.js

import { defineStore } from 'pinia';
import { productPropertyTypeService } from '@/services';

/**
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').ProductPropertyParameter} ProductPropertyParameter
 * @typedef {import('@/.js').ProductPropertyResult} ProductPropertyResult
 */

export const useProductPropertyTypeStore = defineStore('admin-product-property-type', {
  state: () => ({
    /** @type {ProductPropertyResult[]} */
    productPropertyTypes: [],
    /** @type {boolean} */
    loading: false,
    /** @type {boolean} */
    error: false,
    /** @type {string | null} */
    errorMessage: null,
    /** @type {PaginationList<ProductPropertyResult> | null} */
    pagedProductPropertyTypes: null,
  }),
  actions: {
    /**
     * Fetches product property types for a given product ID or based on query parameters.
     * @param {QueryableParams} [params={}] - Can include `productId` and `propertyTypeId`.
     */
    async fetchProductPropertyTypes(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await productPropertyTypeService.get(params);
        if (response.succeeded) {
          this.pagedProductPropertyTypes = response.data;
          this.productPropertyTypes = response.data?.items || [];
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch product properties.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching product properties:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Manages product properties (add/update/remove).
     * @param {string} productId - The ID of the product.
     * @param {{data: ProductPropertyParameter[]}} payload - Object containing an array of property parameters.
     * @returns {Promise<boolean>}
     */
    async manageProductPropertyTypes(productId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await productPropertyTypeService.manage(productId, payload);
        if (response.succeeded) {
          this.fetchProductPropertyTypes({ productId }); // Refresh properties for the product
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to manage product properties.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error managing product properties:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },
  },
});
