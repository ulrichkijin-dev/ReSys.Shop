// src/ReSys.Shop.Admin/src/service/admin/settings/setting-module/setting.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').SettingParameter} SettingParameter
 * @typedef {import('@/.js').SettingSelectItem} SettingSelectItem
 * @typedef {import('@/.js').SettingListItem} SettingListItem
 * @typedef {import('@/.js').SettingDetail} SettingDetail
 */

const API_BASE_ROUTE = 'api/admin/settings';
const httpClient = configureHttpClient();

export const settingService = {
  /**
   * Creates a new setting.
   * @param {SettingParameter} payload
   * @returns {Promise<ApiResponse<SettingListItem>>}
   */
  async create(payload) {
    const response = await httpClient.post(API_BASE_ROUTE, payload);
    return response.data;
  },

  /**
   * Updates an existing setting by ID.
   * @param {string} id - The ID of the setting to update.
   * @param {SettingParameter} payload
   * @returns {Promise<ApiResponse<SettingListItem>>}
   */
  async update(id, payload) {
    const response = await httpClient.put(`${API_BASE_ROUTE}/${id}`, payload);
    return response.data;
  },

  /**
   * Deletes a setting by ID.
   * @param {string} id - The ID of the setting to delete.
   * @returns {Promise<ApiResponse<void>>}
   */
  async delete(id) {
    const response = await httpClient.delete(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },

  /**
   * Retrieves details of a specific setting by ID.
   * @param {string} id - The ID of the setting.
   * @returns {Promise<ApiResponse<SettingDetail>>}
   */
  async getById(id) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },

  /**
   * Retrieves a paginated list of settings.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<SettingListItem>>>}
   */
  async getPagedList(params = {}) {
    const response = await httpClient.get(API_BASE_ROUTE, { params });
    return response.data;
  },

  /**
   * Retrieves a simplified selectable list of settings.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<SettingSelectItem>>>}
   */
  async getSelectList(params = {}) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/select`, { params });
    return response.data;
  },
};
