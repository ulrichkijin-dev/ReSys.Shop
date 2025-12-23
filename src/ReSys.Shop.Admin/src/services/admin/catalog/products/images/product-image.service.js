// src/ReSys.Shop.Admin/src/service/admin/catalog/products/images/product-image.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').ProductImageParameter} ProductImageParameter
 * @typedef {import('@/.js').ProductImageUploadParameter} ProductImageUploadParameter
 * @typedef {import('@/.js').ProductImageResult} ProductImageResult
 */

const API_BASE_ROUTE = 'api/admin/catalog/products';
const httpClient = configureHttpClient();

export const productImageService = {
  /**
   * Manages (add/update/remove) product images in a batch.
   * @param {string} productId - The ID of the product.
   * @param {FormData} payload - FormData containing image data.
   * @returns {Promise<ApiResponse<ProductImageResult[]>>}
   */
  async manage(productId, payload) {
    const response = await httpClient.put(`${API_BASE_ROUTE}/${productId}/images/batch`, payload, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return response.data;
  },

  /**
   * Uploads a new product image.
   * @param {string} productId - The ID of the product.
   * @param {FormData} payload - FormData containing the image file and metadata.
   * @returns {Promise<ApiResponse<ProductImageResult>>}
   */
  async upload(productId, payload) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/${productId}/images`, payload, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return response.data;
  },

  /**
   * Edits an existing product image.
   * @param {string} productId - The ID of the product.
   * @param {string} imageId - The ID of the image to edit.
   * @param {FormData} payload - FormData containing updated image file and/or metadata.
   * @returns {Promise<ApiResponse<ProductImageResult>>}
   */
  async edit(productId, imageId, payload) {
    const response = await httpClient.put(`${API_BASE_ROUTE}/${productId}/images/${imageId}`, payload, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return response.data;
  },

  /**
   * Removes a product image.
   * @param {string} productId - The ID of the product.
   * @param {string} imageId - The ID of the image to remove.
   * @returns {Promise<ApiResponse<void>>}
   */
  async remove(productId, imageId) {
    const response = await httpClient.delete(`${API_BASE_ROUTE}/${productId}/images/${imageId}`);
    return response.data;
  },

  /**
   * Retrieves all images for a product or based on other query parameters.
   * @param {QueryableParams} [params={}] - Query parameters, can include productId or variantId.
   * @returns {Promise<ApiResponse<PaginationList<ProductImageResult>>>}
   */
  async get(params = {}) {
    // The C# endpoint is GET /products/images and takes productId as a query param.
    const response = await httpClient.get(`${API_BASE_ROUTE}/images`, { params });
    return response.data;
  },
};
