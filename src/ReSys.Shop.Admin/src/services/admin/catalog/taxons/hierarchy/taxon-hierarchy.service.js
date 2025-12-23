// src/ReSys.Shop.Admin/src/service/admin/catalog/taxons/hierarchy/taxon-hierarchy.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').TaxonHierarchyRequest} TaxonHierarchyRequest
 * @typedef {import('@/.js').TreeListItem} TreeListItem
 * @typedef {import('@/.js').FlatListItem} FlatListItem
 * @typedef {import('@/.js').TaxonRebuildHierarchyRequest} TaxonRebuildHierarchyRequest
 */

const API_BASE_ROUTE = 'api/admin/catalog/taxons';
const httpClient = configureHttpClient();

export const TaxonHierarchyService = {
  /**
   * Retrieves hierarchical tree structure of taxons.
   * @param {TaxonHierarchyRequest} [params={}] - Query parameters.
   * @returns {Promise<ApiResponse<TreeListItem>>}
   */
  async getTree(params = {}) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/tree`, { params });
    return response.data;
  },

  /**
   * Retrieves a flattened paginated list of taxons with hierarchy indicators.
   * @param {TaxonHierarchyRequest} [params={}] - Query parameters.
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
  async rebuild(taxonomyId) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/rebuild/${taxonomyId}`);
    return response.data;
  },

  /**
   * Validates taxonomy hierarchy for cycles and invalid references.
   * @param {string} taxonomyId - The ID of the taxonomy to validate.
   * @returns {Promise<ApiResponse<void>>}
   */
  async validate(taxonomyId) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/validate/${taxonomyId}`);
    return response.data;
  },
};
