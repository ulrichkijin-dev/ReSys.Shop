// src/ReSys.Shop.Admin/src/stores/admin/catalog/products/images/product-image.store.js

import { defineStore } from 'pinia';
import { productImageService } from '@/services';

/**
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').ProductImageParameter} ProductImageParameter
 * @typedef {import('@/.js').ProductImageUploadParameter} ProductImageUploadParameter
 * @typedef {import('@/.js').ProductImageResult} ProductImageResult
 */

export const useProductImageStore = defineStore('admin-product-image', {
  state: () => ({
    /** @type {ProductImageResult[]} */
    productImages: [],
    /** @type {boolean} */
    loading: false,
    /** @type {boolean} */
    error: false,
    /** @type {string | null} */
    errorMessage: null,
    /** @type {PaginationList<ProductImageResult> | null} */
    pagedProductImages: null,
  }),
  actions: {
    /**
     * Fetches product images for a given product ID or based on query parameters.
     * @param {QueryableParams} [params={}] - Can include `productId` and `variantId`.
     */
    async fetchProductImages(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await productImageService.get(params);
        if (response.succeeded) {
          this.pagedProductImages = response.data;
          this.productImages = response.data?.items || [];
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch product images.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching product images:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Manages (add/update/remove) product images in a batch.
     * @param {string} productId - The ID of the product.
     * @param {FormData} payload - FormData containing image data.
     * @returns {Promise<boolean>}
     */
    async manageProductImages(productId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await productImageService.manage(productId, payload);
        if (response.succeeded) {
          this.fetchProductImages({ productId }); // Refresh images for the product
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to manage product images.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error managing product images:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Uploads a new product image.
     * @param {string} productId - The ID of the product.
     * @param {FormData} payload - FormData containing the image file and metadata.
     * @returns {Promise<boolean>}
     */
    async uploadProductImage(productId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await productImageService.upload(productId, payload);
        if (response.succeeded) {
          this.fetchProductImages({ productId }); // Refresh images for the product
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to upload product image.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error uploading product image:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Edits an existing product image.
     * @param {string} productId - The ID of the product.
     * @param {string} imageId - The ID of the image to edit.
     * @param {FormData} payload - FormData containing updated image file and/or metadata.
     * @returns {Promise<boolean>}
     */
    async editProductImage(productId, imageId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await productImageService.edit(productId, imageId, payload);
        if (response.succeeded) {
          this.fetchProductImages({ productId }); // Refresh images for the product
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to edit product image.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error editing product image:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Removes a product image.
     * @param {string} productId - The ID of the product.
     * @param {string} imageId - The ID of the image to remove.
     * @returns {Promise<boolean>}
     */
    async removeProductImage(productId, imageId) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await productImageService.remove(productId, imageId);
        if (response.succeeded) {
          this.fetchProductImages({ productId }); // Refresh images for the product
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to remove product image.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error removing product image:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },
  },
});
