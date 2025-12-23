// src/ReSys.Shop.Admin/src/service/admin/identity/permissions/permission.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').PermissionSelectItem} PermissionSelectItem
 * @typedef {import('@/.js').PermissionListItem} PermissionListItem
 * @typedef {import('@/.js').PermissionDetail} PermissionDetail
 */

const API_BASE_ROUTE = 'api/admin/identity/permissions';
const httpClient = configureHttpClient();

export const permissionService = {
  /**
   * Retrieves details of a specific access permission by ID.
   * @param {string} id - The ID of the permission.
   * @returns {Promise<ApiResponse<PermissionDetail>>}
   */
  async getById(id) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },

  /**
   * Retrieves details of a specific access permission by Name.
   * @param {string} name - The name of the permission.
   * @returns {Promise<ApiResponse<PermissionDetail>>}
   */
  async getByName(name) {
    // Assuming a RESTful endpoint pattern for lookup by name
    const response = await httpClient.get(`${API_BASE_ROUTE}/by-name/${name}`);
    return response.data;
  },

  /**
   * Retrieves a paginated list of access permissions.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<PermissionListItem>>>}
   */
  async getPagedList(params = {}) {
    const response = await httpClient.get(API_BASE_ROUTE, { params });
    return response.data;
  },

  /**
   * Retrieves a simplified selectable list of access permissions.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<PermissionSelectItem>>>}
   */
  async getSelectList(params = {}) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/select`, { params });
    return response.data;
  },
};
