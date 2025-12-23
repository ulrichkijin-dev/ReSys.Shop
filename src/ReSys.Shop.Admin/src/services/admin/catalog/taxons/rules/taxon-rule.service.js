// src/ReSys.Shop.Admin/src/service/admin/catalog/taxons/rules/taxon-rule.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').TaxonRuleItem} TaxonRuleItem
 * @typedef {import('@/.js').TaxonRuleListRequest} TaxonRuleListRequest
 * @typedef {import('@/.js').TaxonRuleManageRequest} TaxonRuleManageRequest
 */

const API_BASE_ROUTE = 'api/admin/catalog/taxons';
const httpClient = configureHttpClient();

export const TaxonRuleService = {
  /**
   * Retrieves the rules for an existing taxon.
   * @param {string} taxonId - The ID of the taxon.
   * @param {TaxonRuleListRequest} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<TaxonRuleItem>>>}
   */
  async getList(taxonId, params = {}) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${taxonId}/rules`, { params });
    return response.data;
  },

  /**
   * Updates the rules for an existing taxon (batch update).
   * @param {string} taxonId - The ID of the taxon.
   * @param {TaxonRuleManageRequest} payload
   * @returns {Promise<ApiResponse<TaxonRuleItem[]>>}
   */
  async update(taxonId, payload) {
    const response = await httpClient.put(`${API_BASE_ROUTE}/${taxonId}/rules`, payload);
    return response.data;
  },
};
