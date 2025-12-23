// src/ReSys.Shop.Admin/src/service/admin/settings/shipping-methods/shipping-method.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').ShippingMethodParameter} ShippingMethodParameter
 * @typedef {import('@/.js').ShippingMethodSelectItem} ShippingMethodSelectItem
 * @typedef {import('@/.js').ShippingMethodListItem} ShippingMethodListItem
 * @typedef {import('@/.js').ShippingMethodDetail} ShippingMethodDetail
 */

const API_BASE_ROUTE = 'api/admin/settings/shipping-methods';
const httpClient = configureHttpClient();

export const shippingMethodService = {
  /**
   * Creates a new shipping method.
   * @param {ShippingMethodParameter} payload
   * @returns {Promise<ApiResponse<ShippingMethodListItem>>}
   */
  async create(payload) {
    const response = await httpClient.post(API_BASE_ROUTE, payload);
    return response.data;
  },

  /**
   * Updates an existing shipping method by ID.
   * @param {string} id - The ID of the shipping method to update.
   * @param {ShippingMethodParameter} payload
   * @returns {Promise<ApiResponse<ShippingMethodListItem>>}
   */
  async update(id, payload) {
    const response = await httpClient.put(`${API_BASE_ROUTE}/${id}`, payload);
    return response.data;
  },

  /**
   * Deletes a shipping method by ID.
   * @param {string} id - The ID of the shipping method to delete.
   * @returns {Promise<ApiResponse<void>>}
   */
  async delete(id) {
    const response = await httpClient.delete(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },

  /**
   * Activates a shipping method by ID.
   * @param {string} id - The ID of the shipping method to activate.
   * @returns {Promise<ApiResponse<void>>}
   */
  async activate(id) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/${id}/activate`);
    return response.data;
  },

  /**
   * Deactivates a shipping method by ID.
   * @param {string} id - The ID of the shipping method to deactivate.
   * @returns {Promise<ApiResponse<void>>}
   */
  async deactivate(id) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/${id}/deactivate`);
    return response.data;
  },

  /**
   * Retrieves details of a specific shipping method by ID.
   * @param {string} id - The ID of the shipping method.
   * @returns {Promise<ApiResponse<ShippingMethodDetail>>}
   */
  async getById(id) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },

  /**
   * Retrieves a paginated list of shipping methods.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<ShippingMethodListItem>>>}
   */
  async getPagedList(params = {}) {
    const response = await httpClient.get(API_BASE_ROUTE, { params });
    return response.data;
  },

  /**
   * Retrieves a simplified selectable list of shipping methods.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<ShippingMethodSelectItem>>>}
   */
  async getSelectList(params = {}) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/select`, { params });
    return response.data;
  },
};
