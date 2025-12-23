// src/ReSys.Shop.Admin/src/service/admin/catalog/option-values/option-value.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').OptionValueParameter} OptionValueParameter
 * @typedef {import('@/.js').OptionValueSelectItem} OptionValueSelectItem
 * @typedef {import('@/.js').OptionValueListItem} OptionValueListItem
 * @typedef {import('@/.js').OptionValueDetail} OptionValueDetail
 */

const API_BASE_ROUTE = 'api/admin/catalog/option-values';
const httpClient = configureHttpClient();

export const optionValueService = {
  /**
   * Creates a new option value.
   * @param {OptionValueParameter} payload
   * @returns {Promise<ApiResponse<OptionValueListItem>>}
   */
  async create(payload) {
    const response = await httpClient.post(API_BASE_ROUTE, payload);
    return response.data;
  },

  /**
   * Updates an existing option value by ID.
   * @param {string} id - The ID of the option value to update.
   * @param {OptionValueParameter} payload
   * @returns {Promise<ApiResponse<OptionValueListItem>>}
   */
  async update(id, payload) {
    const response = await httpClient.put(`${API_BASE_ROUTE}/${id}`, payload);
    return response.data;
  },

  /**
   * Deletes an option value by ID.
   * @param {string} id - The ID of the option value to delete.
   * @returns {Promise<ApiResponse<void>>}
   */
  async delete(id) {
    const response = await httpClient.delete(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },

  /**
   * Retrieves details of a specific option value by ID.
   * @param {string} id - The ID of the option value.
   * @returns {Promise<ApiResponse<OptionValueDetail>>}
   */
  async getById(id) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },

  /**
   * Retrieves a paginated list of option values.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<OptionValueListItem>>>}
   */
  async getPagedList(params = {}) {
    const response = await httpClient.get(API_BASE_ROUTE, { params });
    return response.data;
  },

  /**
   * Retrieves a simplified selectable list of option values.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<OptionValueSelectItem>>>}
   */
  async getSelectList(params = {}) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/select`, { params });
    return response.data;
  },
};
