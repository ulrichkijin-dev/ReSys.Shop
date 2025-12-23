// src/ReSys.Shop.Admin/src/service/accounts/addresses/address.service.js
import { configureHttpClient } from '@/utils/http-client';
const httpClient = configureHttpClient();

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').AddressParam} AddressParam
 * @typedef {import('@/.js').AddressListItem} AddressListItem
 * @typedef {import('@/.js').AddressDetail} AddressDetail
 * @typedef {import('@/.js').AddressSelectItem} AddressSelectItem
 */

const API_BASE_ROUTE = 'api/account/addresses';

export const addressService = {
  /**
   * Creates a new user address.
   * @param {AddressParam} payload - The address data.
   * @returns {Promise<ApiResponse<AddressListItem>>}
   */
  async create(payload) {
    const response = await httpClient.post(API_BASE_ROUTE, payload);
    return response.data;
  },

  /**
   * Deletes a user address by ID.
   * @param {string} id - The ID of the address to delete.
   * @returns {Promise<ApiResponse<void>>}
   */
  async delete(id) {
    const response = await httpClient.delete(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },

  /**
   * Retrieves details of a specific user address by ID.
   * @param {string} id - The ID of the address.
   * @returns {Promise<ApiResponse<AddressDetail>>}
   */
  async getById(id) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },

  /**
   * Retrieves a paginated list of user addresses.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<AddressListItem>>>}
   */
  async getPagedList(params = {}) {
    const response = await httpClient.get(API_BASE_ROUTE, { params });
    return response.data;
  },

  /**
   * Retrieves a simplified list of user addresses for selection purposes.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<AddressSelectItem>>>}
   */
  async getSelectList(params = {}) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/select`, { params });
    return response.data;
  },

  /**
   * Updates an existing user address by ID.
   * @param {string} id - The ID of the address to update.
   * @param {AddressParam} payload - The updated address data.
   * @returns {Promise<ApiResponse<AddressDetail>>}
   */
  async update(id, payload) {
    const response = await httpClient.put(`${API_BASE_ROUTE}/${id}`, payload);
    return response.data;
  },
};
