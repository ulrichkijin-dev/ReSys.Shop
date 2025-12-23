// src/ReSys.Shop.Admin/src/service/admin/promotions/rules/rule.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').PromotionRuleParameter} PromotionRuleParameter
 * @typedef {import('@/.js').PromotionRuleItem} PromotionRuleItem
 * @typedef {import('@/.js').PromotionTaxonRuleParameter} PromotionTaxonRuleParameter
 * @typedef {import('@/.js').PromotionTaxonRuleItem} PromotionTaxonRuleItem
 * @typedef {import('@/.js').PromotionUsersRuleParameter} PromotionUsersRuleParameter
 * @typedef {import('@/.js').PromotionUsersRuleItem} PromotionUsersRuleItem
 */

const API_BASE_ROUTE = 'api/admin/promotions';
const httpClient = configureHttpClient();

export const promotionRuleService = {
  /**
   * Retrieves rules for a promotion.
   * @param {string} promotionId - The ID of the promotion.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<PromotionRuleItem>>>}
   */
  async getRules(promotionId, params = {}) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${promotionId}/rules`, { params });
    return response.data;
  },

  /**
   * Adds a new rule to a promotion.
   * @param {string} promotionId - The ID of the promotion.
   * @param {PromotionRuleParameter} payload
   * @returns {Promise<ApiResponse<PromotionRuleItem>>}
   */
  async addRule(promotionId, payload) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/${promotionId}/rules`, payload);
    return response.data;
  },

  /**
   * Updates a specific rule for a promotion.
   * @param {string} promotionId - The ID of the promotion.
   * @param {string} ruleId - The ID of the rule to update.
   * @param {PromotionRuleParameter} payload
   * @returns {Promise<ApiResponse<PromotionRuleItem>>}
   */
  async updateRule(promotionId, ruleId, payload) {
    const response = await httpClient.put(`${API_BASE_ROUTE}/${promotionId}/rules/${ruleId}`, payload);
    return response.data;
  },

  /**
   * Deletes a specific rule from a promotion.
   * @param {string} promotionId - The ID of the promotion.
   * @param {string} ruleId - The ID of the rule to delete.
   * @returns {Promise<ApiResponse<void>>}
   */
  async deleteRule(promotionId, ruleId) {
    const response = await httpClient.delete(`${API_BASE_ROUTE}/${promotionId}/rules/${ruleId}`);
    return response.data;
  },

  /**
   * Retrieves taxons associated with a specific promotion rule.
   * @param {string} promotionId - The ID of the promotion.
   * @param {string} ruleId - The ID of the rule.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<PromotionTaxonRuleItem>>>}
   */
  async getRuleTaxons(promotionId, ruleId, params = {}) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${promotionId}/rules/${ruleId}/taxons`, { params });
    return response.data;
  },

  /**
   * Reconciles the taxons associated with a promotion rule.
   * @param {string} promotionId - The ID of the promotion.
   * @param {string} ruleId - The ID of the rule.
   * @param {{data: PromotionTaxonRuleParameter[]}} payload - Array of taxon rule parameters.
   * @returns {Promise<ApiResponse<void>>}
   */
  async manageRuleTaxons(promotionId, ruleId, payload) {
    const response = await httpClient.put(`${API_BASE_ROUTE}/${promotionId}/rules/${ruleId}/taxons`, payload);
    return response.data;
  },

  /**
   * Retrieves users associated with a specific promotion rule.
   * @param {string} promotionId - The ID of the promotion.
   * @param {string} ruleId - The ID of the rule.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<PromotionUsersRuleItem>>>}
   */
  async getRuleUsers(promotionId, ruleId, params = {}) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${promotionId}/rules/${ruleId}/users`, { params });
    return response.data;
  },

  /**
   * Reconciles the users associated with a promotion rule.
   * @param {string} promotionId - The ID of the promotion.
   * @param {string} ruleId - The ID of the rule.
   * @param {{data: PromotionUsersRuleParameter[]}} payload - Array of user rule parameters.
   * @returns {Promise<ApiResponse<void>>}
   */
  async manageRuleUsers(promotionId, ruleId, payload) {
    const response = await httpClient.put(`${API_BASE_ROUTE}/${promotionId}/rules/${ruleId}/users`, payload);
    return response.data;
  },
};
