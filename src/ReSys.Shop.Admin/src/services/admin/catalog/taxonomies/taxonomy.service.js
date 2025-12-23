// src/ReSys.Shop.Admin/src/service/admin/catalog/taxonomies/taxonomy.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').TaxonomyParameter} TaxonomyParameter
 * @typedef {import('@/.js').TaxonomySelectItem} TaxonomySelectItem
 * @typedef {import('@/.js').TaxonomyListItem} TaxonomyListItem
 * @typedef {import('@/.js').TaxonomyDetail} TaxonomyDetail
 */

const API_BASE_ROUTE = 'api/admin/catalog/taxonomies';
const httpClient = configureHttpClient();

export const taxonomyService = {
  /**
   * Creates a new taxonomy.
   * @param {TaxonomyParameter} payload
   * @returns {Promise<ApiResponse<TaxonomyListItem>>}
   */
  async create(payload) {
    const response = await httpClient.post(API_BASE_ROUTE, payload);
    return response.data;
  },

  /**
   * Updates an existing taxonomy by ID.
   * @param {string} id - The ID of the taxonomy to update.
   * @param {TaxonomyParameter} payload
   * @returns {Promise<ApiResponse<TaxonomyListItem>>}
   */
  async update(id, payload) {
    const response = await httpClient.put(`${API_BASE_ROUTE}/${id}`, payload);
    return response.data;
  },

  /**
   * Deletes a taxonomy by ID.
   * @param {string} id - The ID of the taxonomy to delete.
   * @returns {Promise<ApiResponse<void>>}
   */
  async delete(id) {
    const response = await httpClient.delete(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },

  /**
   * Retrieves details of a specific taxonomy by ID.
   * @param {string} id - The ID of the taxonomy.
   * @returns {Promise<ApiResponse<TaxonomyDetail>>}
   */
  async getById(id) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },

  /**
   * Retrieves a paginated list of taxonomies.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<TaxonomyListItem>>>}
   */
  async getPagedList(params = {}) {
    const response = await httpClient.get(API_BASE_ROUTE, { params });
    return response.data;
  },

  /**
   * Retrieves a simplified selectable list of taxonomies.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<TaxonomySelectItem>>>}
   */
  async getSelectList(params = {}) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/select`, { params });
    return response.data;
  },
};
