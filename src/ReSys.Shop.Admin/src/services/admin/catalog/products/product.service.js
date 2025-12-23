// src/ReSys.Shop.Admin/src/service/admin/catalog/products/product.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').ProductParameter} ProductParameter
 * @typedef {import('@/.js').ProductSelectItem} ProductSelectItem
 * @typedef {import('@/.js').ProductListItem} ProductListItem
 * @typedef {import('@/.js').ProductDetail} ProductDetail
 */

const API_BASE_ROUTE = 'api/admin/catalog/products';
const httpClient = configureHttpClient();

export const productService = {
  /**
   * Creates a new product.
   * @param {ProductParameter} payload
   * @returns {Promise<ApiResponse<ProductListItem>>}
   */
  async create(payload) {
    const response = await httpClient.post(API_BASE_ROUTE, payload);
    return response.data;
  },

  /**
   * Updates an existing product by ID.
   * @param {string} id - The ID of the product to update.
   * @param {ProductParameter} payload
   * @returns {Promise<ApiResponse<ProductListItem>>}
   */
  async update(id, payload) {
    const response = await httpClient.put(`${API_BASE_ROUTE}/${id}`, payload);
    return response.data;
  },

  /**
   * Deletes a product by ID.
   * @param {string} id - The ID of the product to delete.
   * @returns {Promise<ApiResponse<void>>}
   */
  async delete(id) {
    const response = await httpClient.delete(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },

  /**
   * Retrieves details of a specific product by ID.
   * @param {string} id - The ID of the product.
   * @returns {Promise<ApiResponse<ProductDetail>>}
   */
  async getById(id) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },

  /**
   * Retrieves a paginated list of products.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<ProductListItem>>>}
   */
  async getPagedList(params = {}) {
    const response = await httpClient.get(API_BASE_ROUTE, { params });
    return response.data;
  },

  /**
   * Retrieves a simplified selectable list of products.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<ProductSelectItem>>>}
   */
  async getSelectList(params = {}) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/select`, { params });
    return response.data;
  },

  /**
   * Activates a product.
   * @param {string} id - The ID of the product to activate.
   * @returns {Promise<ApiResponse<void>>}
   */
  async activate(id) {
    const response = await httpClient.patch(`${API_BASE_ROUTE}/${id}/activate`);
    return response.data;
  },

  /**
   * Archives a product.
   * @param {string} id - The ID of the product to archive.
   * @returns {Promise<ApiResponse<void>>}
   */
  async archive(id) {
    const response = await httpClient.patch(`${API_BASE_ROUTE}/${id}/archive`);
    return response.data;
  },

  /**
   * Sets a product to draft status.
   * @param {string} id - The ID of the product to set to draft.
   * @returns {Promise<ApiResponse<void>>}
   */
  async draft(id) {
    const response = await httpClient.patch(`${API_BASE_ROUTE}/${id}/draft`);
    return response.data;
  },

  /**
   * Discontinues a product.
   * @param {string} id - The ID of the product to discontinue.
   * @returns {Promise<ApiResponse<void>>}
   */
  async discontinue(id) {
    const response = await httpClient.patch(`${API_BASE_ROUTE}/${id}/discontinue`);
    return response.data;
  },
};
