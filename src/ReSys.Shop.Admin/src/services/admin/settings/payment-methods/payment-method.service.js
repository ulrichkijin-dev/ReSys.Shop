// src/ReSys.Shop.Admin/src/service/admin/settings/payment-methods/payment-method.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').PaymentMethodParameter} PaymentMethodParameter
 * @typedef {import('@/.js').PaymentMethodSelectItem} PaymentMethodSelectItem
 * @typedef {import('@/.js').PaymentMethodListItem} PaymentMethodListItem
 * @typedef {import('@/.js').PaymentMethodDetail} PaymentMethodDetail
 */

const API_BASE_ROUTE = 'api/admin/settings/payment-methods';
const httpClient = configureHttpClient();

export const paymentMethodService = {
  /**
   * Creates a new payment method.
   * @param {PaymentMethodParameter} payload
   * @returns {Promise<ApiResponse<PaymentMethodListItem>>}
   */
  async create(payload) {
    const response = await httpClient.post(API_BASE_ROUTE, payload);
    return response.data;
  },

  /**
   * Updates an existing payment method by ID.
   * @param {string} id - The ID of the payment method to update.
   * @param {PaymentMethodParameter} payload
   * @returns {Promise<ApiResponse<PaymentMethodListItem>>}
   */
  async update(id, payload) {
    const response = await httpClient.put(`${API_BASE_ROUTE}/${id}`, payload);
    return response.data;
  },

  /**
   * Soft deletes a payment method by ID.
   * @param {string} id - The ID of the payment method to delete.
   * @returns {Promise<ApiResponse<void>>}
   */
  async delete(id) {
    const response = await httpClient.delete(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },

  /**
   * Restores a soft-deleted payment method by ID.
   * @param {string} id - The ID of the payment method to restore.
   * @returns {Promise<ApiResponse<void>>}
   */
  async restore(id) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/${id}/restore`);
    return response.data;
  },

  /**
   * Retrieves details of a specific payment method by ID.
   * @param {string} id - The ID of the payment method.
   * @returns {Promise<ApiResponse<PaymentMethodDetail>>}
   */
  async getById(id) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },

  /**
   * Retrieves a paginated list of payment methods.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<PaymentMethodListItem>>>}
   */
  async getPagedList(params = {}) {
    const response = await httpClient.get(API_BASE_ROUTE, { params });
    return response.data;
  },

  /**
   * Retrieves a simplified selectable list of payment methods.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<PaymentMethodSelectItem>>>}
   */
  async getSelectList(params = {}) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/select`, { params });
    return response.data;
  },
};
