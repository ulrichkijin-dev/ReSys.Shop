// src/ReSys.Shop.Admin/src/service/admin/inventories/stock-transfers/stock-transfer.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').StockTransferParameter} StockTransferParameter
 * @typedef {import('@/.js').StockTransferListItem} StockTransferListItem
 * @typedef {import('@/.js').StockTransferDetail} StockTransferDetail
 * @typedef {import('@/.js').ExecuteStockTransferParameter} ExecuteStockTransferParameter
 * @typedef {import('@/.js').ReceiveStockParameter} ReceiveStockParameter
 */

const API_BASE_ROUTE = 'api/admin/inventories/stock-transfers';
const httpClient = configureHttpClient();

export const stockTransferService = {
  /**
   * Creates a new stock transfer.
   * @param {StockTransferParameter} payload
   * @returns {Promise<ApiResponse<StockTransferListItem>>}
   */
  async create(payload) {
    const response = await httpClient.post(API_BASE_ROUTE, payload);
    return response.data;
  },

  /**
   * Updates an existing stock transfer by ID.
   * @param {string} id - The ID of the stock transfer to update.
   * @param {StockTransferParameter} payload
   * @returns {Promise<ApiResponse<StockTransferListItem>>}
   */
  async update(id, payload) {
    const response = await httpClient.put(`${API_BASE_ROUTE}/${id}`, payload);
    return response.data;
  },

  /**
   * Deletes a stock transfer by ID.
   * @param {string} id - The ID of the stock transfer to delete.
   * @returns {Promise<ApiResponse<void>>}
   */
  async delete(id) {
    const response = await httpClient.delete(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },

  /**
   * Retrieves details of a specific stock transfer by ID.
   * @param {string} id - The ID of the stock transfer.
   * @returns {Promise<ApiResponse<StockTransferDetail>>}
   */
  async getById(id) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },

  /**
   * Retrieves a paginated list of stock transfers.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<StockTransferListItem>>>}
   */
  async getPagedList(params = {}) {
    const response = await httpClient.get(API_BASE_ROUTE, { params });
    return response.data;
  },

  /**
   * Executes a stock transfer between locations.
   * @param {string} id - The ID of the stock transfer.
   * @param {ExecuteStockTransferParameter} payload
   * @returns {Promise<ApiResponse<void>>}
   */
  async executeTransfer(id, payload) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/${id}/execute`, payload);
    return response.data;
  },

  /**
   * Receives stock from supplier (no source location).
   * @param {string} id - The ID of the stock transfer.
   * @param {ReceiveStockParameter} payload
   * @returns {Promise<ApiResponse<void>>}
   */
  async receiveStock(id, payload) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/${id}/receive`, payload);
    return response.data;
  },
};
