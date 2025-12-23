// src/ReSys.Shop.Admin/src/service/admin/catalog/products/option-types/product-option-type.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').ProductOptionTypeParameter} ProductOptionTypeParameter
 * @typedef {import('@/.js').ProductOptionTypeResult} ProductOptionTypeResult
 */

const API_BASE_ROUTE = 'api/admin/catalog/products';
const httpClient = configureHttpClient();

export const productOptionTypeService = {
  /**
   * Manages product option types (add/update/remove).
   * @param {string} productId - The ID of the product.
   * @param {{data: ProductOptionTypeParameter[]}} payload - Object containing an array of option type parameters.
   * @returns {Promise<ApiResponse<void>>}
   */
  async manage(productId, payload) {
    const response = await httpClient.put(`${API_BASE_ROUTE}/${productId}/option-types`, payload);
    return response.data;
  },

  /**
   * Retrieves all option types assigned to a product or based on other query parameters.
   * @param {QueryableParams} [params={}] - Query parameters, can include productId.
   * @returns {Promise<ApiResponse<PaginationList<ProductOptionTypeResult>>>}
   */
  async get(params = {}) {
    // The C# endpoint is GET /products/option-types and takes productId as a query param.
    const response = await httpClient.get(`${API_BASE_ROUTE}/option-types`, { params });
    return response.data;
  },
};
