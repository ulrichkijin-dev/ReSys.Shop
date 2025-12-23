// src/ReSys.Shop.Admin/src/service/admin/catalog/variants/variant.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').VariantParameter} VariantParameter
 * @typedef {import('@/.js').VariantSelectItem} VariantSelectItem
 * @typedef {import('@/.js').VariantListItem} VariantListItem
 * @typedef {import('@/.js').VariantDetail} VariantDetail
 * @typedef {import('@/.js').VariantPriceItem} VariantPriceItem
 * @typedef {import('@/.js').VariantPriceSetRequest} VariantPriceParameter
 */

const API_BASE_ROUTE = 'api/admin/catalog/variants';
const httpClient = configureHttpClient();

export const variantService = {
  /**
   * Creates a new product variant.
   * @param {VariantParameter} payload
   * @returns {Promise<ApiResponse<VariantListItem>>}
   */
  async create(payload) {
    const response = await httpClient.post(API_BASE_ROUTE, payload);
    return response.data;
  },

  /**
   * Updates an existing product variant by ID.
   * @param {string} id - The ID of the variant to update.
   * @param {VariantParameter} payload
   * @returns {Promise<ApiResponse<VariantListItem>>}
   */
  async update(id, payload) {
    const response = await httpClient.put(`${API_BASE_ROUTE}/${id}`, payload);
    return response.data;
  },

  /**
   * Deletes a product variant by ID.
   * @param {string} id - The ID of the variant to delete.
   * @returns {Promise<ApiResponse<void>>}
   */
  async delete(id) {
    const response = await httpClient.delete(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },

  /**
   * Retrieves details of a specific product variant by ID.
   * @param {string} id - The ID of the variant.
   * @returns {Promise<ApiResponse<VariantDetail>>}
   */
  async getById(id) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },

  /**
   * Retrieves a paginated list of product variants.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<VariantListItem>>>}
   */
  async getPagedList(params = {}) {
    const response = await httpClient.get(API_BASE_ROUTE, { params });
    return response.data;
  },

  /**
   * Retrieves a simplified selectable list of product variants.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<VariantSelectItem>>>}
   */
  async getSelectList(params = {}) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/select`, { params });
    return response.data;
  },

  /**
   * Discontinues a product variant.
   * @param {string} id - The ID of the variant to discontinue.
   * @returns {Promise<ApiResponse<void>>}
   */
  async discontinue(id) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/${id}/discontinue`);
    return response.data;
  },

  /**
   * Sets the price for a variant.
   * @param {string} variantId - The ID of the variant.
   * @param {VariantPriceParameter} payload - The price data.
   * @returns {Promise<ApiResponse<VariantPriceItem>>}
   */
  async setPrice(variantId, payload) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/${variantId}/prices`, payload);
    return response.data;
  },

  /**
   * Retrieves all prices for a variant.
   * @param {string} variantId - The ID of the variant.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering (though typically not paged for prices).
   * @returns {Promise<ApiResponse<PaginationList<VariantPriceItem>>>}
   */
  async getPrices(variantId, params = {}) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${variantId}/prices`, { params });
    return response.data;
  },

  /**
   * Updates the option values associated with a variant.
   * @param {string} variantId - The ID of the variant.
   * @param {{optionValueIds: string[]}} payload - Object containing an array of option value IDs.
   * @returns {Promise<ApiResponse<void>>}
   */
  async updateOptionValues(variantId, payload) {
    const response = await httpClient.put(`${API_BASE_ROUTE}/${variantId}/option-values`, payload);
    return response.data;
  },
};
