// src/ReSys.Shop.Admin/src/stores/admin/catalog/products/properties/product-property.store.js

import { defineStore } from 'pinia';
import { ProductPropertyService } from '@/services';

/**
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').ProductPropertyParameter} ProductPropertyParameter
 * @typedef {import('@/.js').ProductPropertyResult} ProductPropertyResult
 * @typedef {import('@/.js').ProductPropertyGetListRequest} ProductPropertyGetListRequest
 * @typedef {import('@/.js').ProductPropertyManageRequest} ProductPropertyManageRequest
 */

export const useProductPropertyStore = defineStore('admin-product-property', {
  state: () => ({
    /** @type {ProductPropertyResult[]} */
    productProperties: [],
    /** @type {boolean} */
    loading: false,
    /** @type {boolean} */
    error: false,
    /** @type {string | null} */
    errorMessage: null,
    /** @type {PaginationList<ProductPropertyResult> | null} */
    pagedProductProperties: null,
  }),
  actions: {
    /**
     * Fetches a paginated list of product properties.
     * @param {ProductPropertyGetListRequest} [params={}]
     */
    async fetchProductProperties(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await ProductPropertyService.getList(params);
        if (response.succeeded) {
          this.pagedProductProperties = response.data;
          this.productProperties = response.data?.items || [];
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
     * Manages (synchronizes) a product's properties.
     * @param {string} productId
     * @param {ProductPropertyManageRequest} payload
     * @returns {Promise<boolean>}
     */
    async manageProductProperties(productId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await ProductPropertyService.manage(productId, payload);
        if (response.succeeded) {
          this.fetchProductProperties({ productId: [productId] }); // Refresh list
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
