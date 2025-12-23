// src/ReSys.Shop.Admin/src/service/admin/inventories/stock-locations/stock-location.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').StockLocationParameter} StockLocationParameter
 * @typedef {import('@/.js').StockLocationSelectItem} StockLocationSelectItem
 * @typedef {import('@/.js').StockLocationListItem} StockLocationListItem
 * @typedef {import('@/.js').StockLocationDetail} StockLocationDetail
 */

const API_BASE_ROUTE = 'api/admin/inventories/stock-locations';
const httpClient = configureHttpClient();

export const stockLocationService = {
  /**
   * Creates a new stock location.
   * @param {StockLocationParameter} payload
   * @returns {Promise<ApiResponse<StockLocationListItem>>}
   */
  async create(payload) {
    const response = await httpClient.post(API_BASE_ROUTE, payload);
    return response.data;
  },

  /**
   * Updates an existing stock location by ID.
   * @param {string} id - The ID of the stock location to update.
   * @param {StockLocationParameter} payload
   * @returns {Promise<ApiResponse<StockLocationListItem>>}
   */
  async update(id, payload) {
    const response = await httpClient.put(`${API_BASE_ROUTE}/${id}`, payload);
    return response.data;
  },

  /**
   * Soft deletes a stock location by ID.
   * @param {string} id - The ID of the stock location to delete.
   * @returns {Promise<ApiResponse<void>>}
   */
  async delete(id) {
    const response = await httpClient.delete(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },

  /**
   * Restores a soft-deleted stock location by ID.
   * @param {string} id - The ID of the stock location to restore.
   * @returns {Promise<ApiResponse<void>>}
   */
  async restore(id) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/${id}/restore`);
    return response.data;
  },

  /**
   * Retrieves details of a specific stock location by ID.
   * @param {string} id - The ID of the stock location.
   * @returns {Promise<ApiResponse<StockLocationDetail>>}
   */
  async getById(id) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },

  /**
   * Retrieves a paginated list of stock locations.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<StockLocationListItem>>>}
   */
  async getPagedList(params = {}) {
    const response = await httpClient.get(API_BASE_ROUTE, { params });
    return response.data;
  },

  /**
   * Retrieves a simplified selectable list of stock locations.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<StockLocationSelectItem>>>}
   */
  async getSelectList(params = {}) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/select`, { params });
    return response.data;
  },
};
