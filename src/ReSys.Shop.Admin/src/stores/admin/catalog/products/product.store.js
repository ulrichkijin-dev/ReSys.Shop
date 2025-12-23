// src/ReSys.Shop.Admin/src/stores/admin/catalog/products/product.store.js

import { defineStore } from 'pinia';
import { productService } from '@/services';

/**
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').ProductParameter} ProductParameter
 * @typedef {import('@/.js').ProductSelectItem} ProductSelectItem
 * @typedef {import('@/.js').ProductListItem} ProductListItem
 * @typedef {import('@/.js').ProductDetail} ProductDetail
 */

export const useProductStore = defineStore('admin-product', {
  state: () => ({
    /** @type {ProductListItem[]} */
    products: [],
    /** @type {ProductDetail | null} */
    selectedProduct: null,
    /** @type {boolean} */
    loading: false,
    /** @type {boolean} */
    error: false,
    /** @type {string | null} */
    errorMessage: null,
    /** @type {PaginationList<ProductListItem> | null} */
    pagedProducts: null,
    /** @type {PaginationList<ProductSelectItem> | null} */
    selectProducts: null,
  }),
  actions: {
    /**
     * Fetches a paginated list of products.
     * @param {QueryableParams} [params={}]
     */
    async fetchPagedProducts(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await productService.getPagedList(params);
        if (response.succeeded) {
          this.pagedProducts = response.data;
          this.products = response.data?.items || [];
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch products.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching paged products:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches a select list of products.
     * @param {QueryableParams} [params={}]
     */
    async fetchSelectProducts(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await productService.getSelectList(params);
        if (response.succeeded) {
          this.selectProducts = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch select products.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching select products:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches details for a single product by ID.
     * @param {string} id
     */
    async fetchProductById(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await productService.getById(id);
        if (response.succeeded) {
          this.selectedProduct = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch product details.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching product by ID:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Creates a new product.
     * @param {ProductParameter} payload
     * @returns {Promise<boolean>}
     */
    async createProduct(payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await productService.create(payload);
        if (response.succeeded) {
          this.fetchPagedProducts(); // Refresh the list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to create product.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error creating product:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Updates an existing product.
     * @param {string} id
     * @param {ProductParameter} payload
     * @returns {Promise<boolean>}
     */
    async updateProduct(id, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await productService.update(id, payload);
        if (response.succeeded) {
          this.fetchProductById(id); // Refresh details
          this.fetchPagedProducts(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to update product.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error updating product:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Deletes a product.
     * @param {string} id
     * @returns {Promise<boolean>}
     */
    async deleteProduct(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await productService.delete(id);
        if (response.succeeded) {
          this.fetchPagedProducts(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to delete product.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error deleting product:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Activates a product.
     * @param {string} id
     * @returns {Promise<boolean>}
     */
    async activateProduct(id) {
      this.loading = true;
      try {
        const response = await productService.activate(id);
        if (response.succeeded) {
          this.fetchProductById(id);
          this.fetchPagedProducts();
          return true;
        }
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Archives a product.
     * @param {string} id
     * @returns {Promise<boolean>}
     */
    async archiveProduct(id) {
      this.loading = true;
      try {
        const response = await productService.archive(id);
        if (response.succeeded) {
          this.fetchProductById(id);
          this.fetchPagedProducts();
          return true;
        }
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Sets a product to draft status.
     * @param {string} id
     * @returns {Promise<boolean>}
     */
    async draftProduct(id) {
      this.loading = true;
      try {
        const response = await productService.draft(id);
        if (response.succeeded) {
          this.fetchProductById(id);
          this.fetchPagedProducts();
          return true;
        }
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Discontinues a product.
     * @param {string} id
     * @returns {Promise<boolean>}
     */
    async discontinueProduct(id) {
      this.loading = true;
      try {
        const response = await productService.discontinue(id);
        if (response.succeeded) {
          this.fetchProductById(id);
          this.fetchPagedProducts();
          return true;
        }
        return false;
      } finally {
        this.loading = false;
      }
    },
  },
});
