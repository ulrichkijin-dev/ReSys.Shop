// src/ReSys.Shop.Admin/src/stores/admin/promotions/analytics/analytics.store.js

import { defineStore } from 'pinia';
import { promotionAnalyticsService } from '@/services';

/**
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').PromotionStatsResult} PromotionStatsResult
 * @typedef {import('@/.js').PromotionPreviewResult} PromotionPreviewResult
 * @typedef {import('@/.js').PromotionHistoryItem} PromotionHistoryItem
 * @typedef {import('@/.js').PreviewPromotionParameter} PreviewPromotionParameter
 */

export const usePromotionAnalyticsStore = defineStore('admin-promotion-analytics', {
  state: () => ({
    /** @type {PromotionStatsResult | null} */
    promotionStats: null,
    /** @type {PromotionPreviewResult | null} */
    promotionPreview: null,
    /** @type {PaginationList<PromotionHistoryItem> | null} */
    promotionHistory: null,
    /** @type {boolean} */
    loading: false,
    /** @type {boolean} */
    error: false,
    /** @type {string | null} */
    errorMessage: null,
  }),
  actions: {
    /**
     * Fetches usage statistics and performance metrics for a promotion.
     * @param {string} promotionId
     */
    async fetchPromotionStats(promotionId) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await promotionAnalyticsService.getStats(promotionId);
        if (response.succeeded) {
          this.promotionStats = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch promotion statistics.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching promotion statistics:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Tests promotion against a sample order without applying it.
     * @param {string} promotionId
     * @param {PreviewPromotionParameter} payload
     */
    async previewPromotion(promotionId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await promotionAnalyticsService.preview(promotionId, payload);
        if (response.succeeded) {
          this.promotionPreview = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to preview promotion.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error previewing promotion:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Retrieves the change history and audit trail for a promotion.
     * @param {string} promotionId
     * @param {QueryableParams} [params={}]
     */
    async fetchPromotionHistory(promotionId, params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await promotionAnalyticsService.getHistory(promotionId, params);
        if (response.succeeded) {
          this.promotionHistory = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch promotion history.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching promotion history:', err);
      } finally {
        this.loading = false;
      }
    },
  },
});
