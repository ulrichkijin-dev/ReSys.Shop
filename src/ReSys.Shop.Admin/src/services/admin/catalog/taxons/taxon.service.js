// src/ReSys.Shop.Admin/src/service/admin/catalog/taxons/taxon.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').TaxonParameter} TaxonParameter
 * @typedef {import('@/.js').TaxonSelectItem} TaxonSelectItem
 * @typedef {import('@/.js').TaxonListItem} TaxonListItem
 * @typedef {import('@/.js').TaxonDetail} TaxonDetail
 * @typedef {import('@/.js').TaxonImageItem} TaxonImageItem
 * @typedef {import('@/.js').TaxonRuleItem} TaxonRuleItem
 * @typedef {import('@/.js').TaxonRuleParameter} TaxonRuleParameter
 * @typedef {import('@/.js').HierarchyParameter} HierarchyParameter
 * @typedef {import('@/.js').TreeListItem} TreeListItem
 * @typedef {import('@/.js').FlatListItem} FlatListItem
 */

const API_BASE_ROUTE = 'api/admin/catalog/taxons';
const httpClient = configureHttpClient();

export const taxonService = {
  /**
   * Creates a new taxon.
   * @param {TaxonParameter} payload
   * @returns {Promise<ApiResponse<TaxonListItem>>}
   */
  async create(payload) {
    const response = await httpClient.post(API_BASE_ROUTE, payload);
    return response.data;
  },

  /**
   * Updates an existing taxon by ID.
   * @param {string} id - The ID of the taxon to update.
   * @param {TaxonParameter} payload
   * @returns {Promise<ApiResponse<TaxonListItem>>}
   */
  async update(id, payload) {
    // Note: C# endpoint uses POST for update, but PUT is semantically more correct for full resource updates.
    // Following C# for now, but keeping in mind for future refactor.
    const response = await httpClient.post(`${API_BASE_ROUTE}/${id}`, payload);
    return response.data;
  },

  /**
   * Deletes a taxon by ID.
   * @param {string} id - The ID of the taxon to delete.
   * @returns {Promise<ApiResponse<void>>}
   */
  async delete(id) {
    const response = await httpClient.delete(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },

  /**
   * Retrieves details of a specific taxon by ID.
   * @param {string} id - The ID of the taxon.
   * @returns {Promise<ApiResponse<TaxonDetail>>}
   */
  async getById(id) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },

  /**
   * Retrieves a paginated list of taxons.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<TaxonListItem>>>}
   */
  async getPagedList(params = {}) {
    const response = await httpClient.get(API_BASE_ROUTE, { params });
    return response.data;
  },

  /**
   * Retrieves a simplified selectable list of taxons.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<TaxonSelectItem>>>}
   */
  async getSelectList(params = {}) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/select`, { params });
    return response.data;
  },

  /**
   * Updates taxon images (batch update).
   * @param {string} id - The ID of the taxon.
   * @param {FormData} payload - FormData containing image files and metadata.
   * @returns {Promise<ApiResponse<TaxonImageItem[]>>}
   */
  async updateImages(id, payload) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/${id}/images`, payload, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },

  /**
   * Retrieves images for a specific taxon.
   * @param {string} id - The ID of the taxon.
   * @param {QueryableParams} [params={}]
   * @returns {Promise<ApiResponse<PaginationList<TaxonImageItem>>>}
   */
  async getImages(id, params = {}) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${id}/images`, { params });
    return response.data;
  },

  /**
   * Retrieves the hierarchical tree structure of taxons.
   * @param {HierarchyParameter} [params={}]
   * @returns {Promise<ApiResponse<TreeListItem>>}
   */
  async getTree(params = {}) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/tree`, { params });
    return response.data;
  },

  /**
   * Retrieves a flattened list of taxons with hierarchy indicators.
   * @param {HierarchyParameter} [params={}]
   * @returns {Promise<ApiResponse<PaginationList<FlatListItem>>>}
   */
  async getFlatList(params = {}) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/flat`, { params });
    return response.data;
  },

  /**
   * Rebuilds nested sets and permalinks for a taxonomy.
   * @param {string} taxonomyId - The ID of the taxonomy to rebuild.
   * @returns {Promise<ApiResponse<void>>}
   */
  async rebuildHierarchy(taxonomyId) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/rebuild/${taxonomyId}`);
    return response.data;
  },

  /**
   * Validates taxonomy hierarchy for cycles and invalid references.
   * @param {string} taxonomyId - The ID of the taxonomy to validate.
   * @returns {Promise<ApiResponse<void>>}
   */
  async validateHierarchy(taxonomyId) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/validate/${taxonomyId}`);
    return response.data;
  },

  /**
   * Retrieves the rules for an existing taxon.
   * @param {string} id - The ID of the taxon.
   * @param {QueryableParams} [params={}]
   * @returns {Promise<ApiResponse<PaginationList<TaxonRuleItem>>>}
   */
  async getRules(id, params = {}) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${id}/rules`, { params });
    return response.data;
  },

  /**
   * Updates the rules for an existing taxon.
   * @param {string} id - The ID of the taxon.
   * @param {{rules: TaxonRuleParameter[]}} payload - Object containing an array of rule parameters.
   * @returns {Promise<ApiResponse<{taxonId: string, rules: TaxonRuleItem[]}>>}
   */
  async updateRules(id, payload) {
    const response = await httpClient.put(`${API_BASE_ROUTE}/${id}/rules`, payload);
    return response.data;
  },
};
