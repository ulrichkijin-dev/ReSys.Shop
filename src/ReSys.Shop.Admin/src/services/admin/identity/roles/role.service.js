// src/ReSys.Shop.Admin/src/service/admin/identity/roles/role.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').RoleParameter} RoleParameter
 * @typedef {import('@/.js').RoleSelectItem} RoleSelectItem
 * @typedef {import('@/.js').RoleListItem} RoleListItem
 * @typedef {import('@/.js').RoleDetail} RoleDetail
 * @typedef {import('@/.js').RoleUserItem} RoleUserItem
 * @typedef {import('@/.js').RolePermissionItem} RolePermissionItem
 * @typedef {import('@/.js').AssignUserToRoleParameter} AssignUserToRoleParameter
 * @typedef {import('@/.js').UnassignUserFromRoleParameter} UnassignUserFromRoleParameter
 * @typedef {import('@/.js').AssignPermissionToRoleParameter} AssignPermissionToRoleParameter
 * @typedef {import('@/.js').UnassignPermissionFromRoleParameter} UnassignPermissionFromRoleParameter
 */

const API_BASE_ROUTE = 'api/admin/identity/roles';
const httpClient = configureHttpClient();

export const roleService = {
  /**
   * Creates a new role.
   * @param {RoleParameter} payload
   * @returns {Promise<ApiResponse<RoleListItem>>}
   */
  async create(payload) {
    const response = await httpClient.post(API_BASE_ROUTE, payload);
    return response.data;
  },

  /**
   * Updates an existing role by ID.
   * @param {string} id - The ID of the role to update.
   * @param {RoleParameter} payload
   * @returns {Promise<ApiResponse<RoleListItem>>}
   */
  async update(id, payload) {
    const response = await httpClient.put(`${API_BASE_ROUTE}/${id}`, payload);
    return response.data;
  },

  /**
   * Deletes a role by ID.
   * @param {string} id - The ID of the role to delete.
   * @returns {Promise<ApiResponse<void>>}
   */
  async delete(id) {
    const response = await httpClient.delete(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },

  /**
   * Retrieves details of a specific role by ID.
   * @param {string} id - The ID of the role.
   * @returns {Promise<ApiResponse<RoleDetail>>}
   */
  async getById(id) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },

  /**
   * Retrieves a paginated list of roles.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<RoleListItem>>>}
   */
  async getPagedList(params = {}) {
    const response = await httpClient.get(API_BASE_ROUTE, { params });
    return response.data;
  },

  /**
   * Retrieves a simplified selectable list of roles.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<RoleSelectItem>>>}
   */
  async getSelectList(params = {}) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/select`, { params });
    return response.data;
  },

  /**
   * Retrieves a paginated list of users assigned to a specific role.
   * @param {string} roleId - The ID of the role.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<RoleUserItem>>>}
   */
  async getUsers(roleId, params = {}) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${roleId}/users`, { params });
    return response.data;
  },

  /**
   * Assigns a user to a role.
   * @param {string} roleId - The ID of the role.
   * @param {AssignUserToRoleParameter} payload
   * @returns {Promise<ApiResponse<void>>}
   */
  async assignUser(roleId, payload) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/${roleId}/users/assign`, payload);
    return response.data;
  },

  /**
   * Unassigns a user from a role.
   * @param {string} roleId - The ID of the role.
   * @param {UnassignUserFromRoleParameter} payload
   * @returns {Promise<ApiResponse<void>>}
   */
  async unassignUser(roleId, payload) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/${roleId}/users/unassign`, payload);
    return response.data;
  },

  /**
   * Retrieves a list of permissions assigned to a specific role.
   * @param {string} roleId - The ID of the role.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<RolePermissionItem>>>}
   */
  async getPermissions(roleId, params = {}) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${roleId}/permissions`, { params });
    return response.data;
  },

  /**
   * Assigns a permission to a role.
   * @param {string} roleId - The ID of the role.
   * @param {AssignPermissionToRoleParameter} payload
   * @returns {Promise<ApiResponse<void>>}
   */
  async assignPermission(roleId, payload) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/${roleId}/permissions/assign`, payload);
    return response.data;
  },

  /**
   * Unassigns a permission from a role.
   * @param {string} roleId - The ID of the role.
   * @param {UnassignPermissionFromRoleParameter} payload
   * @returns {Promise<ApiResponse<void>>}
   */
  async unassignPermission(roleId, payload) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/${roleId}/permissions/unassign`, payload);
    return response.data;
  },
};
