// src/ReSys.Shop.Admin/src/service/admin/inventories/stock-items/stock-item.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').StockItemParameter} StockItemParameter
 * @typedef {import('@/.js').StockItemListItem} StockItemListItem
 * @typedef {import('@/.js').StockItemDetail} StockItemDetail
 * @typedef {import('@/.js').StockMovementItem} StockMovementItem
 * @typedef {import('@/.js').AdjustStockParameter} AdjustStockParameter
 * @typedef {import('@/.js').ReserveStockParameter} ReserveStockParameter
 * @typedef {import('@/.js').ReleaseStockParameter} ReleaseStockParameter
 */

const API_BASE_ROUTE = 'api/admin/inventories/stock-items';
const httpClient = configureHttpClient();

export const stockItemService = {
  /**
   * Creates a new stock item.
   * @param {StockItemParameter} payload
   * @returns {Promise<ApiResponse<StockItemListItem>>}
   */
  async create(payload) {
    const response = await httpClient.post(API_BASE_ROUTE, payload);
    return response.data;
  },

  /**
   * Updates an existing stock item by ID.
   * @param {string} id - The ID of the stock item to update.
   * @param {StockItemParameter} payload
   * @returns {Promise<ApiResponse<StockItemListItem>>}
   */
  async update(id, payload) {
    const response = await httpClient.put(`${API_BASE_ROUTE}/${id}`, payload);
    return response.data;
  },

  /**
   * Deletes a stock item by ID.
   * @param {string} id - The ID of the stock item to delete.
   * @returns {Promise<ApiResponse<void>>}
   */
  async delete(id) {
    const response = await httpClient.delete(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },

  /**
   * Retrieves details of a specific stock item by ID.
   * @param {string} id - The ID of the stock item.
   * @returns {Promise<ApiResponse<StockItemDetail>>}
   */
  async getById(id) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },

  /**
   * Retrieves a paginated list of stock items.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<StockItemListItem>>>}
   */
  async getPagedList(params = {}) {
    const response = await httpClient.get(API_BASE_ROUTE, { params });
    return response.data;
  },

  /**
   * Adjusts the stock quantity for a stock item.
   * @param {string} id - The ID of the stock item.
   * @param {AdjustStockParameter} payload
   * @returns {Promise<ApiResponse<void>>}
   */
  async adjust(id, payload) {
    const response = await httpClient.patch(`${API_BASE_ROUTE}/${id}/adjust`, payload);
    return response.data;
  },

  /**
   * Reserves stock for an order.
   * @param {string} id - The ID of the stock item.
   * @param {ReserveStockParameter} payload
   * @returns {Promise<ApiResponse<void>>}
   */
  async reserve(id, payload) {
    const response = await httpClient.patch(`${API_BASE_ROUTE}/${id}/reserve`, payload);
    return response.data;
  },

  /**
   * Releases previously reserved stock.
   * @param {string} id - The ID of the stock item.
   * @param {ReleaseStockParameter} payload
   * @returns {Promise<ApiResponse<void>>}
   */
  async release(id, payload) {
    const response = await httpClient.patch(`${API_BASE_ROUTE}/${id}/release`, payload);
    return response.data;
  },

  /**
   * Retrieves stock movement history for a stock item.
   * @param {string} id - The ID of the stock item.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<StockMovementItem>>>}
   */
  async getMovements(id, params = {}) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${id}/movements`, { params });
    return response.data;
  },
};
