// src/ReSys.Shop.Admin/src/service/admin/catalog/products/property-types/product-property-type.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').ProductPropertyParameter} ProductPropertyParameter
 * @typedef {import('@/.js').ProductPropertyResult} ProductPropertyResult
 */

const API_BASE_ROUTE = 'api/admin/catalog/products';
const httpClient = configureHttpClient();

export const productPropertyTypeService = {
  /**
   * Manages product properties (add/update/remove).
   * @param {string} productId - The ID of the product.
   * @param {{data: ProductPropertyParameter[]}} payload - Object containing an array of property parameters.
   * @returns {Promise<ApiResponse<ProductPropertyResult[]>>}
   */
  async manage(productId, payload) {
    const response = await httpClient.put(`${API_BASE_ROUTE}/${productId}/properties`, payload);
    return response.data;
  },

  /**
   * Retrieves all properties assigned to a product or based on other query parameters.
   * @param {QueryableParams} [params={}] - Query parameters, can include productId or propertyTypeId.
   * @returns {Promise<ApiResponse<PaginationList<ProductPropertyResult>>>}
   */
  async get(params = {}) {
    // The C# endpoint is GET /products/properties and takes productId/propertyTypeId as query params.
    const response = await httpClient.get(`${API_BASE_ROUTE}/properties`, { params });
    return response.data;
  },
};
