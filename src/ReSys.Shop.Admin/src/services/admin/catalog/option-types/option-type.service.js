// src/ReSys.Shop.Admin/src/service/admin/catalog/option-types/option-type.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').OptionTypeParameter} OptionTypeParameter
 * @typedef {import('@/.js').OptionTypeSelectItem} OptionTypeSelectItem
 * @typedef {import('@/.js').OptionTypeListItem} OptionTypeListItem
 * @typedef {import('@/.js').OptionTypeDetail} OptionTypeDetail
 */

const API_BASE_ROUTE = 'api/admin/catalog/option-types';
const httpClient = configureHttpClient();

export const optionTypeService = {
  /**
   * Creates a new option type.
   * @param {OptionTypeParameter} payload
   * @returns {Promise<ApiResponse<OptionTypeListItem>>}
   */
  async create(payload) {
    const response = await httpClient.post(API_BASE_ROUTE, payload);
    return response.data;
  },

  /**
   * Updates an existing option type by ID.
   * @param {string} id - The ID of the option type to update.
   * @param {OptionTypeParameter} payload
   * @returns {Promise<ApiResponse<OptionTypeListItem>>}
   */
  async update(id, payload) {
    const response = await httpClient.put(`${API_BASE_ROUTE}/${id}`, payload);
    return response.data;
  },

  /**
   * Deletes an option type by ID.
   * @param {string} id - The ID of the option type to delete.
   * @returns {Promise<ApiResponse<void>>}
   */
  async delete(id) {
    const response = await httpClient.delete(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },

  /**
   * Retrieves details of a specific option type by ID.
   * @param {string} id - The ID of the option type.
   * @returns {Promise<ApiResponse<OptionTypeDetail>>}
   */
  async getById(id) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },

  /**
   * Retrieves a paginated list of option types.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<OptionTypeListItem>>>}
   */
  async getPagedList(params = {}) {
    const response = await httpClient.get(API_BASE_ROUTE, { params });
    return response.data;
  },

  /**
   * Retrieves a simplified selectable list of option types.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<OptionTypeSelectItem>>>}
   */
  async getSelectList(params = {}) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/select`, { params });
    return response.data;
  },
};
