// src/ReSys.Shop.Admin/src/stores/admin/catalog/taxons/hierarchy/taxon-hierarchy.store.js

import { defineStore } from 'pinia';
import { TaxonHierarchyService } from '@/services';

/**
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').TaxonHierarchyRequest} TaxonHierarchyRequest
 * @typedef {import('@/.js').TreeListItem} TreeListItem
 * @typedef {import('@/.js').FlatListItem} FlatListItem
 * @typedef {import('@/.js').TaxonRebuildHierarchyRequest} TaxonRebuildHierarchyRequest
 */

export const useTaxonHierarchyStore = defineStore('admin-taxon-hierarchy', {
  state: () => ({
    /** @type {TreeListItem | null} */
    taxonTree: null,
    /** @type {PaginationList<FlatListItem> | null} */
    flatTaxons: null,
    /** @type {boolean} */
    loading: false,
    /** @type {boolean} */
    error: false,
    /** @type {string | null} */
    errorMessage: null,
  }),
  actions: {
    /**
     * Fetches the hierarchical tree structure of taxons.
     * @param {TaxonHierarchyRequest} [params={}]
     */
    async fetchTaxonTree(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await TaxonHierarchyService.getTree(params);
        if (response.succeeded) {
          this.taxonTree = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch taxon tree.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching taxon tree:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches a flattened paginated list of taxons with hierarchy indicators.
     * @param {TaxonHierarchyRequest} [params={}]
     */
    async fetchFlatTaxons(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await TaxonHierarchyService.getFlatList(params);
        if (response.succeeded) {
          this.flatTaxons = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch flat taxons.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching flat taxons:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Rebuilds nested sets and permalinks for a taxonomy.
     * @param {string} taxonomyId
     * @returns {Promise<boolean>}
     */
    async rebuildTaxonomyHierarchy(taxonomyId) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await TaxonHierarchyService.rebuild(taxonomyId);
        if (response.succeeded) {
          // Optionally refetch tree/flat list if successful
          this.fetchTaxonTree({ taxonomyId: [taxonomyId] });
          this.fetchFlatTaxons({ taxonomyId: [taxonomyId] });
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to rebuild taxonomy hierarchy.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error rebuilding taxonomy hierarchy:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Validates taxonomy hierarchy for cycles and invalid references.
     * @param {string} taxonomyId
     * @returns {Promise<boolean>}
     */
    async validateTaxonomyHierarchy(taxonomyId) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await TaxonHierarchyService.validate(taxonomyId);
        if (response.succeeded) {
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to validate taxonomy hierarchy.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error validating taxonomy hierarchy:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },
  },
});
