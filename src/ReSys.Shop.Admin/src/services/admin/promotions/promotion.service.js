// src/ReSys.Shop.Admin/src/service/admin/promotions/promotion.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').PromotionParameter} PromotionParameter
 * @typedef {import('@/.js').PromotionSelectItem} PromotionSelectItem
 * @typedef {import('@/.js').PromotionListItem} PromotionListItem
 * @typedef {import('@/.js').PromotionDetail} PromotionDetail
 */

const API_BASE_ROUTE = 'api/admin/promotions';
const httpClient = configureHttpClient();

export const promotionService = {
  /**
   * Creates a new promotion.
   * @param {PromotionParameter} payload
   * @returns {Promise<ApiResponse<PromotionListItem>>}
   */
  async create(payload) {
    const response = await httpClient.post(API_BASE_ROUTE, payload);
    return response.data;
  },

  /**
   * Updates an existing promotion by ID.
   * @param {string} id - The ID of the promotion to update.
   * @param {PromotionParameter} payload
   * @returns {Promise<ApiResponse<PromotionListItem>>}
   */
  async update(id, payload) {
    const response = await httpClient.put(`${API_BASE_ROUTE}/${id}`, payload);
    return response.data;
  },

  /**
   * Deletes a promotion by ID.
   * @param {string} id - The ID of the promotion to delete.
   * @returns {Promise<ApiResponse<void>>}
   */
  async delete(id) {
    const response = await httpClient.delete(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },

  /**
   * Retrieves details of a specific promotion by ID.
   * @param {string} id - The ID of the promotion.
   * @returns {Promise<ApiResponse<PromotionDetail>>}
   */
  async getById(id) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },

  /**
   * Retrieves a paginated list of promotions.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<PromotionListItem>>>}
   */
  async getPagedList(params = {}) {
    const response = await httpClient.get(API_BASE_ROUTE, { params });
    return response.data;
  },

  /**
   * Retrieves a simplified selectable list of promotions.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<PromotionSelectItem>>>}
   */
  async getSelectList(params = {}) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/select`, { params });
    return response.data;
  },

  /**
   * Activates an inactive promotion.
   * @param {string} id - The ID of the promotion to activate.
   * @returns {Promise<ApiResponse<void>>}
   */
  async activate(id) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/${id}/activate`);
    return response.data;
  },

  /**
   * Deactivates an active promotion.
   * @param {string} id - The ID of the promotion to deactivate.
   * @returns {Promise<ApiResponse<void>>}
   */
  async deactivate(id) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/${id}/deactivate`);
    return response.data;
  },

  /**
   * Validates promotion configuration and rules.
   * @param {string} id - The ID of the promotion to validate.
   * @returns {Promise<ApiResponse<void>>}
   */
  async validate(id) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${id}/validate`);
    return response.data;
  },
};
