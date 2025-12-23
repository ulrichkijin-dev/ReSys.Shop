// src/ReSys.Shop.Admin/src/service/admin/catalog/products/classifications/product-classification.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').ProductClassificationParameter} ProductClassificationParameter
 * @typedef {import('@/.js').ProductClassificationResult} ProductClassificationResult
 */

const API_BASE_ROUTE = 'api/admin/catalog/products';
const httpClient = configureHttpClient();

export const productClassificationService = {
  /**
   * Manages product classifications (add/update/remove).
   * @param {string} productId - The ID of the product.
   * @param {{data: ProductClassificationParameter[]}} payload - Object containing an array of classification parameters.
   * @returns {Promise<ApiResponse<void>>}
   */
  async manage(productId, payload) {
    const response = await httpClient.put(`${API_BASE_ROUTE}/${productId}/classifications`, payload);
    return response.data;
  },

  /**
   * Retrieves all taxon classifications assigned to a product or based on other query parameters.
   * @param {QueryableParams} [params={}] - Query parameters, can include productId or taxonId.
   * @returns {Promise<ApiResponse<PaginationList<ProductClassificationResult>>>}
   */
  async get(params = {}) {
    // The C# endpoint is GET /products/classifications and takes productId/taxonId as query params.
    const response = await httpClient.get(`${API_BASE_ROUTE}/classifications`, { params });
    return response.data;
  },
};
