// src/ReSys.Shop.Admin/src/service/admin/identity/users/user.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').UserParameter} UserParameter
 * @typedef {import('@/.js').UserSelectItem} UserSelectItem
 * @typedef {import('@/.js').UserListItem} UserListItem
 * @typedef {import('@/.js').UserDetail} UserDetail
 * @typedef {import('@/.js').UserRoleItem} UserRoleItem
 * @typedef {import('@/.js').UserPermissionItem} UserPermissionItem
 * @typedef {import('@/.js').AssignRoleToUserParameter} AssignRoleToUserParameter
 * @typedef {import('@/.js').UnassignRoleFromUserParameter} UnassignRoleFromUserParameter
 * @typedef {import('@/.js').AssignPermissionToUserParameter} AssignPermissionToUserParameter
 * @typedef {import('@/.js').UnassignPermissionFromUserParameter} UnassignPermissionFromUserParameter
 */

const API_BASE_ROUTE = 'api/admin/identity/users';
const httpClient = configureHttpClient();

export const userService = {
  /**
   * Creates a new user.
   * @param {UserParameter} payload
   * @returns {Promise<ApiResponse<UserListItem>>}
   */
  async create(payload) {
    const response = await httpClient.post(API_BASE_ROUTE, payload);
    return response.data;
  },

  /**
   * Updates an existing user by ID.
   * @param {string} id - The ID of the user to update.
   * @param {UserParameter} payload
   * @returns {Promise<ApiResponse<UserListItem>>}
   */
  async update(id, payload) {
    const response = await httpClient.put(`${API_BASE_ROUTE}/${id}`, payload);
    return response.data;
  },

  /**
   * Deletes a user by ID.
   * @param {string} id - The ID of the user to delete.
   * @returns {Promise<ApiResponse<void>>}
   */
  async delete(id) {
    const response = await httpClient.delete(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },

  /**
   * Retrieves details of a specific user by ID.
   * @param {string} id - The ID of the user.
   * @returns {Promise<ApiResponse<UserDetail>>}
   */
  async getById(id) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },

  /**
   * Retrieves a paginated list of users.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<UserListItem>>>}
   */
  async getPagedList(params = {}) {
    const response = await httpClient.get(API_BASE_ROUTE, { params });
    return response.data;
  },

  /**
   * Retrieves a simplified selectable list of users.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<UserSelectItem>>>}
   */
  async getSelectList(params = {}) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/select`, { params });
    return response.data;
  },

  /**
   * Retrieves all roles assigned to a specific user.
   * @param {string} userId - The ID of the user.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<UserRoleItem>>>}
   */
  async getRoles(userId, params = {}) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${userId}/roles`, { params });
    return response.data;
  },

  /**
   * Assigns a role to a user.
   * @param {string} userId - The ID of the user.
   * @param {AssignRoleToUserParameter} payload
   * @returns {Promise<ApiResponse<void>>}
   */
  async assignRole(userId, payload) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/${userId}/roles/assign`, payload);
    return response.data;
  },

  /**
   * Unassigns a role from a user.
   * @param {string} userId - The ID of the user.
   * @param {UnassignRoleFromUserParameter} payload
   * @returns {Promise<ApiResponse<void>>}
   */
  async unassignRole(userId, payload) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/${userId}/roles/unassign`, payload);
    return response.data;
  },

  /**
   * Retrieves all permissions (claims) assigned to a specific user.
   * @param {string} userId - The ID of the user.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<UserPermissionItem>>>}
   */
  async getPermissions(userId, params = {}) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${userId}/permissions`, { params });
    return response.data;
  },

  /**
   * Assigns a permission to a user.
   * @param {string} userId - The ID of the user.
   * @param {AssignPermissionToUserParameter} payload
   * @returns {Promise<ApiResponse<void>>}
   */
  async assignPermission(userId, payload) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/${userId}/permissions/assign`, payload);
    return response.data;
  },

  /**
   * Unassigns a permission from a user.
   * @param {string} userId - The ID of the user.
   * @param {UnassignPermissionFromUserParameter} payload
   * @returns {Promise<ApiResponse<void>>}
   */
  async unassignPermission(userId, payload) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/${userId}/permissions/unassign`, payload);
    return response.data;
  },
};
