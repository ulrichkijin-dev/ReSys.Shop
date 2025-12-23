// src/ReSys.Shop.Admin/src/stores/admin/promotions/rules/rule.store.js

import { defineStore } from 'pinia';
import { promotionRuleService } from '@/services';

/**
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').PromotionRuleParameter} PromotionRuleParameter
 * @typedef {import('@/.js').PromotionRuleItem} PromotionRuleItem
 * @typedef {import('@/.js').PromotionTaxonRuleParameter} PromotionTaxonRuleParameter
 * @typedef {import('@/.js').PromotionTaxonRuleItem} PromotionTaxonRuleItem
 * @typedef {import('@/.js').PromotionUsersRuleParameter} PromotionUsersRuleParameter
 * @typedef {import('@/.js').PromotionUsersRuleItem} PromotionUsersRuleItem
 */

export const usePromotionRuleStore = defineStore('admin-promotion-rule', {
  state: () => ({
    /** @type {PromotionRuleItem[]} */
    rules: [],
    /** @type {PromotionRuleItem | null} */
    selectedRule: null,
    /** @type {boolean} */
    loading: false,
    /** @type {boolean} */
    error: false,
    /** @type {string | null} */
    errorMessage: null,
    /** @type {PaginationList<PromotionRuleItem> | null} */
    pagedRules: null,
    /** @type {PaginationList<PromotionTaxonRuleItem> | null} */
    ruleTaxons: null,
    /** @type {PaginationList<PromotionUsersRuleItem> | null} */
    ruleUsers: null,
  }),
  actions: {
    /**
     * Fetches rules for a specific promotion.
     * @param {string} promotionId
     * @param {QueryableParams} [params={}]
     */
    async fetchRules(promotionId, params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await promotionRuleService.getRules(promotionId, params);
        if (response.succeeded) {
          this.pagedRules = response.data;
          this.rules = response.data?.items || [];
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch promotion rules.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching promotion rules:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Adds a new rule to a promotion.
     * @param {string} promotionId
     * @param {PromotionRuleParameter} payload
     * @returns {Promise<boolean>}
     */
    async addPromotionRule(promotionId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await promotionRuleService.addRule(promotionId, payload);
        if (response.succeeded) {
          this.fetchRules(promotionId); // Refresh the list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to add promotion rule.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error adding promotion rule:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Updates a specific rule for a promotion.
     * @param {string} promotionId
     * @param {string} ruleId
     * @param {PromotionRuleParameter} payload
     * @returns {Promise<boolean>}
     */
    async updatePromotionRule(promotionId, ruleId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await promotionRuleService.updateRule(promotionId, ruleId, payload);
        if (response.succeeded) {
          this.fetchRules(promotionId); // Refresh the list
          // Optionally update selectedRule if it's the one being updated
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to update promotion rule.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error updating promotion rule:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Deletes a specific rule from a promotion.
     * @param {string} promotionId
     * @param {string} ruleId
     * @returns {Promise<boolean>}
     */
    async deletePromotionRule(promotionId, ruleId) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await promotionRuleService.deleteRule(promotionId, ruleId);
        if (response.succeeded) {
          this.fetchRules(promotionId); // Refresh the list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to delete promotion rule.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error deleting promotion rule:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches taxons associated with a specific promotion rule.
     * @param {string} promotionId
     * @param {string} ruleId
     * @param {QueryableParams} [params={}]
     */
    async fetchRuleTaxons(promotionId, ruleId, params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await promotionRuleService.getRuleTaxons(promotionId, ruleId, params);
        if (response.succeeded) {
          this.ruleTaxons = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch rule taxons.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching rule taxons:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Reconciles the taxons associated with a promotion rule.
     * @param {string} promotionId
     * @param {string} ruleId
     * @param {{data: PromotionTaxonRuleParameter[]}} payload
     * @returns {Promise<boolean>}
     */
    async manageRuleTaxons(promotionId, ruleId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await promotionRuleService.manageRuleTaxons(promotionId, ruleId, payload);
        if (response.succeeded) {
          this.fetchRuleTaxons(promotionId, ruleId); // Refresh the list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to manage rule taxons.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error managing rule taxons:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches users associated with a specific promotion rule.
     * @param {string} promotionId
     * @param {string} ruleId
     * @param {QueryableParams} [params={}]
     */
    async fetchRuleUsers(promotionId, ruleId, params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await promotionRuleService.getRuleUsers(promotionId, ruleId, params);
        if (response.succeeded) {
          this.ruleUsers = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch rule users.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching rule users:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Reconciles the users associated with a promotion rule.
     * @param {string} promotionId
     * @param {string} ruleId
     * @param {{data: PromotionUsersRuleParameter[]}} payload
     * @returns {Promise<boolean>}
     */
    async manageRuleUsers(promotionId, ruleId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await promotionRuleService.manageRuleUsers(promotionId, ruleId, payload);
        if (response.succeeded) {
          this.fetchRuleUsers(promotionId, ruleId); // Refresh the list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to manage rule users.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error managing rule users:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },
  },
});
