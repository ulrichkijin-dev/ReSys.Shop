// src/ReSys.Shop.Admin/src/stores/admin/catalog/option-values/option-value.store.js

import { defineStore } from 'pinia';
import { optionValueService } from '@/services';

/**
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').OptionValueParameter} OptionValueParameter
 * @typedef {import('@/.js').OptionValueSelectItem} OptionValueSelectItem
 * @typedef {import('@/.js').OptionValueListItem} OptionValueListItem
 * @typedef {import('@/.js').OptionValueDetail} OptionValueDetail
 */

export const useOptionValueStore = defineStore('admin-option-value', {
  state: () => ({
    /** @type {OptionValueListItem[]} */
    optionValues: [],
    /** @type {OptionValueDetail | null} */
    selectedOptionValue: null,
    /** @type {boolean} */
    loading: false,
    /** @type {boolean} */
    error: false,
    /** @type {string | null} */
    errorMessage: null,
    /** @type {PaginationList<OptionValueListItem> | null} */
    pagedOptionValues: null,
    /** @type {PaginationList<OptionValueSelectItem> | null} */
    selectOptionValues: null,
  }),
  actions: {
    /**
     * Fetches a paginated list of option values.
     * @param {QueryableParams} [params={}]
     */
    async fetchPagedOptionValues(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await optionValueService.getPagedList(params);
        if (response.succeeded) {
          this.pagedOptionValues = response.data;
          this.optionValues = response.data?.items || [];
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch option values.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching paged option values:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches a select list of option values.
     * @param {QueryableParams} [params={}]
     */
    async fetchSelectOptionValues(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await optionValueService.getSelectList(params);
        if (response.succeeded) {
          this.selectOptionValues = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch select option values.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching select option values:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches details for a single option value by ID.
     * @param {string} id
     */
    async fetchOptionValueById(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await optionValueService.getById(id);
        if (response.succeeded) {
          this.selectedOptionValue = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch option value details.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching option value by ID:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Creates a new option value.
     * @param {OptionValueParameter} payload
     * @returns {Promise<boolean>}
     */
    async createOptionValue(payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await optionValueService.create(payload);
        if (response.succeeded) {
          this.fetchPagedOptionValues(); // Refresh the list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to create option value.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error creating option value:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Updates an existing option value.
     * @param {string} id
     * @param {OptionValueParameter} payload
     * @returns {Promise<boolean>}
     */
    async updateOptionValue(id, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await optionValueService.update(id, payload);
        if (response.succeeded) {
          this.fetchOptionValueById(id); // Refresh details
          this.fetchPagedOptionValues(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to update option value.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error updating option value:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Deletes an option value.
     * @param {string} id
     * @returns {Promise<boolean>}
     */
    async deleteOptionValue(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await optionValueService.delete(id);
        if (response.succeeded) {
          this.fetchPagedOptionValues(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to delete option value.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error deleting option value:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },
  },
});
