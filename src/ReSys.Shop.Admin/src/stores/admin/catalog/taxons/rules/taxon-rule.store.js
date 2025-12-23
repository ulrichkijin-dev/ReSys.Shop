// src/ReSys.Shop.Admin/src/stores/admin/catalog/taxons/rules/taxon-rule.store.js

import { defineStore } from 'pinia';
import { TaxonRuleService } from '@/services';

/**
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').TaxonRuleItem} TaxonRuleItem
 * @typedef {import('@/.js').TaxonRuleListRequest} TaxonRuleListRequest
 * @typedef {import('@/.js').TaxonRuleManageRequest} TaxonRuleManageRequest
 */

export const useTaxonRuleStore = defineStore('admin-taxon-rule', {
  state: () => ({
    /** @type {TaxonRuleItem[]} */
    taxonRules: [],
    /** @type {boolean} */
    loading: false,
    /** @type {boolean} */
    error: false,
    /** @type {string | null} */
    errorMessage: null,
    /** @type {PaginationList<TaxonRuleItem> | null} */
    pagedTaxonRules: null,
  }),
  actions: {
    /**
     * Fetches a paginated list of taxon rules.
     * @param {string} taxonId
     * @param {TaxonRuleListRequest} [params={}]
     */
    async fetchTaxonRules(taxonId, params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await TaxonRuleService.getList(taxonId, params);
        if (response.succeeded) {
          this.pagedTaxonRules = response.data;
          this.taxonRules = response.data?.items || [];
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch taxon rules.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching taxon rules:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Updates the rules for an existing taxon (batch update).
     * @param {string} taxonId
     * @param {TaxonRuleManageRequest} payload
     * @returns {Promise<boolean>}
     */
    async updateTaxonRules(taxonId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await TaxonRuleService.update(taxonId, payload);
        if (response.succeeded) {
          this.fetchTaxonRules(taxonId); // Refresh rules for this taxon
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to update taxon rules.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error updating taxon rules:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },
  },
});
