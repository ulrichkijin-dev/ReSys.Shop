// src/ReSys.Shop.Admin/src/service/admin/catalog/variants/option-values/variant-option-value.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').OptionValueSelectItem} OptionValueSelectItem // Re-using OptionTypeSelectItem as OptionValueSelectItem in C#
 * @typedef {import('@/.js').VariantOptionValueManageRequest} VariantOptionValueManageRequest
 * @typedef {import('@/.js').VariantOptionValueSelectListRequest} VariantOptionValueSelectListRequest
 */

const API_BASE_ROUTE = 'api/admin/catalog/variants';
const httpClient = configureHttpClient();

export const VariantOptionValueService = {
  /**
   * Retrieves option values for selection purposes, possibly filtered by variant.
   * Note: The C# endpoint `OptionValueModule.Get.SelectList.Request` is quite generic
   * and can be used to get all OptionValues or filtered ones. This service method
   * is for accessing that endpoint via the variant's base route structure.
   * @param {VariantOptionValueSelectListRequest} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<OptionValueSelectItem>>>}
   */
  async getSelectList(params = {}) {
    // This endpoint specifically maps to the general OptionValueModule's select list,
    // but the route is defined under variants.
    const response = await httpClient.get(`${API_BASE_ROUTE}/option-values`, { params });
    return response.data;
  },

  /**
   * Add or remove option values from a variant.
   * @param {string} variantId - The ID of the variant.
   * @param {VariantOptionValueManageRequest} payload
   * @returns {Promise<ApiResponse<void>>}
   */
  async manage(variantId, payload) {
    const response = await httpClient.put(`${API_BASE_ROUTE}/${variantId}/option-values`, payload);
    return response.data;
  },
};
