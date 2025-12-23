// src/ReSys.Shop.Admin/src/stores/admin/catalog/taxonomies/taxonomy.store.js

import { defineStore } from 'pinia';
import { taxonomyService } from '@/services';

/**
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').TaxonomyParameter} TaxonomyParameter
 * @typedef {import('@/.js').TaxonomySelectItem} TaxonomySelectItem
 * @typedef {import('@/.js').TaxonomyListItem} TaxonomyListItem
 * @typedef {import('@/.js').TaxonomyDetail} TaxonomyDetail
 */

export const useTaxonomyStore = defineStore('admin-taxonomy', {
  state: () => ({
    /** @type {TaxonomyListItem[]} */
    taxonomies: [],
    /** @type {TaxonomyDetail | null} */
    selectedTaxonomy: null,
    /** @type {boolean} */
    loading: false,
    /** @type {boolean} */
    error: false,
    /** @type {string | null} */
    errorMessage: null,
    /** @type {PaginationList<TaxonomyListItem> | null} */
    pagedTaxonomies: null,
    /** @type {PaginationList<TaxonomySelectItem> | null} */
    selectTaxonomies: null,
  }),
  actions: {
    /**
     * Fetches a paginated list of taxonomies.
     * @param {QueryableParams} [params={}]
     */
    async fetchPagedTaxonomies(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await taxonomyService.getPagedList(params);
        if (response.succeeded) {
          this.pagedTaxonomies = response.data;
          this.taxonomies = response.data?.items || [];
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch taxonomies.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching paged taxonomies:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches a select list of taxonomies.
     * @param {QueryableParams} [params={}]
     */
    async fetchSelectTaxonomies(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await taxonomyService.getSelectList(params);
        if (response.succeeded) {
          this.selectTaxonomies = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch select taxonomies.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching select taxonomies:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches details for a single taxonomy by ID.
     * @param {string} id
     */
    async fetchTaxonomyById(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await taxonomyService.getById(id);
        if (response.succeeded) {
          this.selectedTaxonomy = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch taxonomy details.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching taxonomy by ID:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Creates a new taxonomy.
     * @param {TaxonomyParameter} payload
     * @returns {Promise<boolean>}
     */
    async createTaxonomy(payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await taxonomyService.create(payload);
        if (response.succeeded) {
          this.fetchPagedTaxonomies(); // Refresh the list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to create taxonomy.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error creating taxonomy:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Updates an existing taxonomy.
     * @param {string} id
     * @param {TaxonomyParameter} payload
     * @returns {Promise<boolean>}
     */
    async updateTaxonomy(id, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await taxonomyService.update(id, payload);
        if (response.succeeded) {
          this.fetchTaxonomyById(id); // Refresh details
          this.fetchPagedTaxonomies(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to update taxonomy.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error updating taxonomy:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Deletes a taxonomy.
     * @param {string} id
     * @returns {Promise<boolean>}
     */
    async deleteTaxonomy(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await taxonomyService.delete(id);
        if (response.succeeded) {
          this.fetchPagedTaxonomies(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to delete taxonomy.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error deleting taxonomy:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },
  },
});
