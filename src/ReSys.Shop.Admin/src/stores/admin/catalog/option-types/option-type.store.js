// src/ReSys.Shop.Admin/src/stores/admin/catalog/option-types/option-type.store.js

import { defineStore } from 'pinia';
import { optionTypeService } from '@/services';

/**
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').OptionTypeParameter} OptionTypeParameter
 * @typedef {import('@/.js').OptionTypeSelectItem} OptionTypeSelectItem
 * @typedef {import('@/.js').OptionTypeListItem} OptionTypeListItem
 * @typedef {import('@/.js').OptionTypeDetail} OptionTypeDetail
 */

export const useOptionTypeStore = defineStore('admin-option-type', {
  state: () => ({
    /** @type {OptionTypeListItem[]} */
    optionTypes: [],
    /** @type {OptionTypeDetail | null} */
    selectedOptionType: null,
    /** @type {boolean} */
    loading: false,
    /** @type {boolean} */
    error: false,
    /** @type {string | null} */
    errorMessage: null,
    /** @type {PaginationList<OptionTypeListItem> | null} */
    pagedOptionTypes: null,
    /** @type {PaginationList<OptionTypeSelectItem> | null} */
    selectOptionTypes: null,
  }),
  actions: {
    /**
     * Fetches a paginated list of option types.
     * @param {QueryableParams} [params={}]
     */
    async fetchPagedOptionTypes(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await optionTypeService.getPagedList(params);
        if (response.succeeded) {
          this.pagedOptionTypes = response.data;
          this.optionTypes = response.data?.items || [];
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch option types.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching paged option types:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches a select list of option types.
     * @param {QueryableParams} [params={}]
     */
    async fetchSelectOptionTypes(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await optionTypeService.getSelectList(params);
        if (response.succeeded) {
          this.selectOptionTypes = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch select option types.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching select option types:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches details for a single option type by ID.
     * @param {string} id
     */
    async fetchOptionTypeById(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await optionTypeService.getById(id);
        if (response.succeeded) {
          this.selectedOptionType = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch option type details.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching option type by ID:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Creates a new option type.
     * @param {OptionTypeParameter} payload
     * @returns {Promise<boolean>}
     */
    async createOptionType(payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await optionTypeService.create(payload);
        if (response.succeeded) {
          this.fetchPagedOptionTypes(); // Refresh the list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to create option type.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error creating option type:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Updates an existing option type.
     * @param {string} id
     * @param {OptionTypeParameter} payload
     * @returns {Promise<boolean>}
     */
    async updateOptionType(id, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await optionTypeService.update(id, payload);
        if (response.succeeded) {
          this.fetchOptionTypeById(id); // Refresh details
          this.fetchPagedOptionTypes(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to update option type.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error updating option type:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Deletes an option type.
     * @param {string} id
     * @returns {Promise<boolean>}
     */
    async deleteOptionType(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await optionTypeService.delete(id);
        if (response.succeeded) {
          this.fetchPagedOptionTypes(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to delete option type.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error deleting option type:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },
  },
});
