// src/ReSys.Shop.Admin/src/stores/admin/catalog/taxons/taxon.store.js

import { defineStore } from 'pinia';
import { taxonService } from '@/services';

/**
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

export const useTaxonStore = defineStore('admin-taxon', {
  state: () => ({
    /** @type {TaxonListItem[]} */
    taxons: [],
    /** @type {TaxonDetail | null} */
    selectedTaxon: null,
    /** @type {boolean} */
    loading: false,
    /** @type {boolean} */
    error: false,
    /** @type {string | null} */
    errorMessage: null,
    /** @type {PaginationList<TaxonListItem> | null} */
    pagedTaxons: null,
    /** @type {PaginationList<TaxonSelectItem> | null} */
    selectTaxons: null,
    /** @type {TreeListItem | null} */
    taxonTree: null,
    /** @type {PaginationList<FlatListItem> | null} */
    flatTaxons: null,
    /** @type {PaginationList<TaxonImageItem> | null} */
    taxonImages: null,
    /** @type {PaginationList<TaxonRuleItem> | null} */
    taxonRules: null,
  }),
  actions: {
    /**
     * Fetches a paginated list of taxons.
     * @param {QueryableParams} [params={}]
     */
    async fetchPagedTaxons(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await taxonService.getPagedList(params);
        if (response.succeeded) {
          this.pagedTaxons = response.data;
          this.taxons = response.data?.items || [];
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch taxons.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching paged taxons:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches a select list of taxons.
     * @param {QueryableParams} [params={}]
     */
    async fetchSelectTaxons(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await taxonService.getSelectList(params);
        if (response.succeeded) {
          this.selectTaxons = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch select taxons.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching select taxons:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches details for a single taxon by ID.
     * @param {string} id
     */
    async fetchTaxonById(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await taxonService.getById(id);
        if (response.succeeded) {
          this.selectedTaxon = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch taxon details.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching taxon by ID:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Creates a new taxon.
     * @param {TaxonParameter} payload
     * @returns {Promise<boolean>}
     */
    async createTaxon(payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await taxonService.create(payload);
        if (response.succeeded) {
          this.fetchPagedTaxons(); // Refresh the list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to create taxon.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error creating taxon:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Updates an existing taxon.
     * @param {string} id
     * @param {TaxonParameter} payload
     * @returns {Promise<boolean>}
     */
    async updateTaxon(id, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await taxonService.update(id, payload);
        if (response.succeeded) {
          this.fetchTaxonById(id); // Refresh details
          this.fetchPagedTaxons(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to update taxon.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error updating taxon:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Deletes a taxon.
     * @param {string} id
     * @returns {Promise<boolean>}
     */
    async deleteTaxon(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await taxonService.delete(id);
        if (response.succeeded) {
          this.fetchPagedTaxons(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to delete taxon.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error deleting taxon:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches the hierarchical tree structure of taxons.
     * @param {HierarchyParameter} [params={}]
     */
    async fetchTaxonTree(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await taxonService.getTree(params);
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
     * Fetches a flattened list of taxons with hierarchy indicators.
     * @param {HierarchyParameter} [params={}]
     */
    async fetchFlatTaxons(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await taxonService.getFlatList(params);
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
        const response = await taxonService.rebuildHierarchy(taxonomyId);
        if (response.succeeded) {
          // Optionally refetch tree/flat list
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
        const response = await taxonService.validateHierarchy(taxonomyId);
        if (response.succeeded) {
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to validate taxonomy hierarchy.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred during hierarchy validation.';
        console.error('Error validating taxonomy hierarchy:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Updates taxon images (batch update).
     * @param {string} id - The ID of the taxon.
     * @param {FormData} payload - FormData containing image files and metadata.
     * @returns {Promise<boolean>}
     */
    async updateTaxonImages(id, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await taxonService.updateImages(id, payload);
        if (response.succeeded) {
          this.fetchTaxonImages(id); // Refresh images for the taxon
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to update taxon images.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error updating taxon images:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches images for a specific taxon.
     * @param {string} id - The ID of the taxon.
     * @param {QueryableParams} [params={}]
     */
    async fetchTaxonImages(id, params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await taxonService.getImages(id, params);
        if (response.succeeded) {
          this.taxonImages = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch taxon images.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching taxon images:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Updates the rules for an existing taxon.
     * @param {string} id - The ID of the taxon.
     * @param {{rules: TaxonRuleParameter[]}} payload - Object containing an array of rule parameters.
     * @returns {Promise<boolean>}
     */
    async updateTaxonRules(id, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await taxonService.updateRules(id, payload);
        if (response.succeeded) {
          this.fetchTaxonRules(id); // Refresh rules for the taxon
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

    /**
     * Retrieves the rules for an existing taxon.
     * @param {string} id - The ID of the taxon.
     * @param {QueryableParams} [params={}]
     */
    async fetchTaxonRules(id, params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await taxonService.getRules(id, params);
        if (response.succeeded) {
          this.taxonRules = response.data;
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
  },
});
