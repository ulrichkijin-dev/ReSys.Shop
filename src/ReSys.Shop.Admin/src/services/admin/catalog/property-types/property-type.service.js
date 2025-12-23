// src/ReSys.Shop.Admin/src/service/admin/catalog/property-types/property-type.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').PropertyTypeParameter} PropertyTypeParameter
 * @typedef {import('@/.js').PropertyTypeSelectItem} PropertyTypeSelectItem
 * @typedef {import('@/.js').PropertyTypeListItem} PropertyTypeListItem
 * @typedef {import('@/.js').PropertyTypeDetail} PropertyTypeDetail
 * @typedef {import('@/.js').UpdateDisplayOnParameter} UpdateDisplayOnParameter
 */

const API_BASE_ROUTE = 'api/admin/catalog/properties';
const httpClient = configureHttpClient();

export const propertyTypeService = {
  /**
   * Creates a new property type.
   * @param {PropertyTypeParameter} payload
   * @returns {Promise<ApiResponse<PropertyTypeListItem>>}
   */
  async create(payload) {
    const response = await httpClient.post(API_BASE_ROUTE, payload);
    return response.data;
  },

  /**
   * Updates an existing property type by ID.
   * @param {string} id - The ID of the property type to update.
   * @param {PropertyTypeParameter} payload
   * @returns {Promise<ApiResponse<PropertyTypeListItem>>}
   */
  async update(id, payload) {
    const response = await httpClient.put(`${API_BASE_ROUTE}/${id}`, payload);
    return response.data;
  },

  /**
   * Deletes a property type by ID.
   * @param {string} id - The ID of the property type to delete.
   * @returns {Promise<ApiResponse<void>>}
   */
   async delete(id) {
    const response = await httpClient.delete(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },

  /**
   * Retrieves details of a specific property type by ID.
   * @param {string} id - The ID of the property type.
   * @returns {Promise<ApiResponse<PropertyTypeDetail>>}
   */
  async getById(id) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },

  /**
   * Retrieves a paginated list of property types.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<PropertyTypeListItem>>>}
   */
  async getPagedList(params = {}) {
    const response = await httpClient.get(API_BASE_ROUTE, { params });
    return response.data;
  },

  /**
   * Retrieves a simplified selectable list of property types.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<PropertyTypeSelectItem>>>}
   */
  async getSelectList(params = {}) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/select`, { params });
    return response.data;
  },

  /**
   * Updates the 'DisplayOn' setting for a property type.
   * @param {string} id - The ID of the property type to update.
   * @param {UpdateDisplayOnParameter} payload - The new 'DisplayOn' value.
   * @returns {Promise<ApiResponse<PropertyTypeListItem>>}
   */
  async updateDisplayOn(id, payload) {
    const response = await httpClient.patch(`${API_BASE_ROUTE}/${id}/display-on`, payload);
    return response.data;
  },
};
