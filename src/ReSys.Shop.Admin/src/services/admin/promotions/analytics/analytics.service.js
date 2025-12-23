// src/ReSys.Shop.Admin/src/service/admin/promotions/analytics/analytics.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').PromotionStatsResult} PromotionStatsResult
 * @typedef {import('@/.js').PromotionPreviewResult} PromotionPreviewResult
 * @typedef {import('@/.js').PromotionHistoryItem} PromotionHistoryItem
 * @typedef {object} PreviewPromotionParameter
 * @property {string} orderId - The ID of the order to preview against.
 */

const API_BASE_ROUTE = 'api/admin/promotions';
const httpClient = configureHttpClient();

export const promotionAnalyticsService = {
  /**
   * Retrieves usage statistics and performance metrics for a promotion.
   * @param {string} promotionId - The ID of the promotion.
   * @returns {Promise<ApiResponse<PromotionStatsResult>>}
   */
  async getStats(promotionId) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${promotionId}/stats`);
    return response.data;
  },

  /**
   * Tests promotion against a sample order without applying it.
   * @param {string} promotionId - The ID of the promotion.
   * @param {PreviewPromotionParameter} payload
   * @returns {Promise<ApiResponse<PromotionPreviewResult>>}
   */
  async preview(promotionId, payload) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/${promotionId}/preview`, payload);
    return response.data;
  },

  /**
   * Retrieves the change history and audit trail for a promotion.
   * @param {string} promotionId - The ID of the promotion.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<PromotionHistoryItem>>>}
   */
  async getHistory(promotionId, params = {}) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${promotionId}/history`, { params });
    return response.data;
  },
};
