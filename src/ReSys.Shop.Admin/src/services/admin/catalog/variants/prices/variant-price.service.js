// src/ReSys.Shop.Admin/src/service/admin/catalog/variants/prices/variant-price.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').VariantPriceSetRequest} VariantPriceSetRequest
 * @typedef {import('@/.js').VariantPriceItem} VariantPriceItem
 * @typedef {import('@/.js').VariantPriceListRequest} VariantPriceListRequest
 */

const API_BASE_ROUTE = 'api/admin/catalog/variants';
const httpClient = configureHttpClient();

export const VariantPriceService = {
  /**
   * Sets the price for a variant in a specific currency.
   * @param {string} variantId - The ID of the variant.
   * @param {VariantPriceSetRequest} payload
   * @returns {Promise<ApiResponse<VariantPriceItem>>}
   */
  async setPrice(variantId, payload) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/${variantId}/prices`, payload);
    return response.data;
  },

  /**
   * Retrieves all prices for a variant.
   * @param {string} variantId - The ID of the variant.
   * @param {VariantPriceListRequest} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<VariantPriceItem>>>}
   */
  async getList(variantId, params = {}) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${variantId}/prices`, { params });
    return response.data;
  },
};
