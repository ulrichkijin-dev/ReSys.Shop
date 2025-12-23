// src/ReSys.Shop.Admin/src/service/admin/orders/order.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').OrderListItem} OrderListItem
 * @typedef {import('@/.js').OrderDetail} OrderDetail
 * @typedef {import('@/.js').OrderCreateParameter} OrderCreateRequest
 * @typedef {import('@/.js').OrderUpdateParameter} OrderUpdateRequest
 * @typedef {import('@/.js').OrderApplyCouponParameter} ApplyCouponRequest
 * @typedef {import('@/.js').OrderHistoryItem} OrderHistoryItem
 */

const API_BASE_ROUTE = 'api/admin/orders';
const httpClient = configureHttpClient();

export const orderService = {
  /**
   * Retrieves a paginated list of orders.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<OrderListItem>>>}
   */
  async getPagedList(params = {}) {
    const response = await httpClient.get(API_BASE_ROUTE, { params });
    return response.data;
  },

  /**
   * Creates a new order.
   * @param {OrderCreateRequest} payload
   * @returns {Promise<ApiResponse<OrderDetail>>}
   */
  async create(payload) {
    const response = await httpClient.post(API_BASE_ROUTE, payload);
    return response.data;
  },

  /**
   * Retrieves details of a specific order by ID.
   * @param {string} id - The ID of the order.
   * @returns {Promise<ApiResponse<OrderDetail>>}
   */
  async getById(id) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },

  /**
   * Deletes an order by ID.
   * @param {string} id - The ID of the order to delete.
   * @returns {Promise<ApiResponse<void>>}
   */
  async delete(id) {
    const response = await httpClient.delete(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },

  /**
   * Updates an order by ID.
   * @param {string} id - The ID of the order to update.
   * @param {OrderUpdateRequest} payload
   * @returns {Promise<ApiResponse<OrderDetail>>}
   */
  async update(id, payload) {
    const response = await httpClient.patch(`${API_BASE_ROUTE}/${id}`, payload);
    return response.data;
  },

  /**
   * Advances the order state.
   * @param {string} id - The ID of the order.
   * @returns {Promise<ApiResponse<void>>}
   */
  async advanceOrder(id) {
    const response = await httpClient.patch(`${API_BASE_ROUTE}/${id}/advance`);
    return response.data;
  },

  /**
   * Moves the order to the next state.
   * @param {string} id - The ID of the order.
   * @returns {Promise<ApiResponse<void>>}
   */
  async nextOrderState(id) {
    const response = await httpClient.patch(`${API_BASE_ROUTE}/${id}/next`);
    return response.data;
  },

  /**
   * Completes an order.
   * @param {string} id - The ID of the order.
   * @returns {Promise<ApiResponse<void>>}
   */
  async completeOrder(id) {
    const response = await httpClient.patch(`${API_BASE_ROUTE}/${id}/complete`);
    return response.data;
  },

  /**
   * Empties the order cart.
   * @param {string} id - The ID of the order.
   * @returns {Promise<ApiResponse<void>>}
   */
  async emptyOrder(id) {
    const response = await httpClient.patch(`${API_BASE_ROUTE}/${id}/empty`);
    return response.data;
  },

  /**
   * Approves an order.
   * @param {string} id - The ID of the order.
   * @returns {Promise<ApiResponse<void>>}
   */
  async approveOrder(id) {
    const response = await httpClient.patch(`${API_BASE_ROUTE}/${id}/approve`);
    return response.data;
  },

  /**
   * Cancels an order.
   * @param {string} id - The ID of the order.
   * @returns {Promise<ApiResponse<void>>}
   */
  async cancelOrder(id) {
    const response = await httpClient.patch(`${API_BASE_ROUTE}/${id}/cancel`);
    return response.data;
  },

  /**
   * Applies a coupon code to the order.
   * @param {string} id - The ID of the order.
   * @param {ApplyCouponRequest} payload
   * @returns {Promise<ApiResponse<void>>}
   */
  async applyCoupon(id, payload) {
    const response = await httpClient.patch(`${API_BASE_ROUTE}/${id}/apply_coupon`, payload);
    return response.data;
  },

  /**
   * Retrieves applied coupons for a specific order by ID.
   * @param {string} id - The ID of the order.
   * @returns {Promise<ApiResponse<any[]>>}
   */
  async getCoupons(id) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${id}/coupons`);
    return response.data;
  },

  /**
   * Removes an applied coupon from a specific order by ID.
   * @param {string} id - The ID of the order.
   * @returns {Promise<ApiResponse<void>>}
   */
  async removeAppliedCoupon(id) {
    const response = await httpClient.delete(`${API_BASE_ROUTE}/${id}/apply_coupon`);
    return response.data;
  },
};
