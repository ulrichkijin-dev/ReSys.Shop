// src/ReSys.Shop.Admin/src/service/admin/catalog/products/properties/product-property.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').ProductPropertyResult} ProductPropertyResult
 * @typedef {import('@/.js').ProductPropertyGetListRequest} ProductPropertyGetListRequest
 * @typedef {import('@/.js').ProductPropertyManageRequest} ProductPropertyManageRequest
 */

const API_BASE_ROUTE = 'api/admin/catalog/products';
const httpClient = configureHttpClient();

export const ProductPropertyService = {
  /**
   * Retrieves all properties assigned to a product.
   * @param {ProductPropertyGetListRequest} [params={}] - Query parameters.
   * @returns {Promise<ApiResponse<PaginationList<ProductPropertyResult>>>}
   */
  async getList(params = {}) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/properties`, { params });
    return response.data;
  },

  /**
   * Fully synchronizes product properties (add/update/remove).
   * @param {string} productId - The ID of the product.
   * @param {ProductPropertyManageRequest} payload
   * @returns {Promise<ApiResponse<ProductPropertyResult[]>>}
   */
  async manage(productId, payload) {
    const response = await httpClient.put(`${API_BASE_ROUTE}/${productId}/properties`, payload);
    return response.data;
  },
};
