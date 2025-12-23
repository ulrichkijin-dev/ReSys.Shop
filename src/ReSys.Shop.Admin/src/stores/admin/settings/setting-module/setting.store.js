// src/ReSys.Shop.Admin/src/stores/admin/settings/setting-module/setting.store.js

import { defineStore } from 'pinia';
import { settingService } from '@/services';

/**
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').SettingParameter} SettingParameter
 * @typedef {import('@/.js').SettingSelectItem} SettingSelectItem
 * @typedef {import('@/.js').SettingListItem} SettingListItem
 * @typedef {import('@/.js').SettingDetail} SettingDetail
 */

export const useSettingStore = defineStore('admin-setting', {
  state: () => ({
    /** @type {SettingListItem[]} */
    settings: [],
    /** @type {SettingDetail | null} */
    selectedSetting: null,
    /** @type {boolean} */
    loading: false,
    /** @type {boolean} */
    error: false,
    /** @type {string | null} */
    errorMessage: null,
    /** @type {PaginationList<SettingListItem> | null} */
    pagedSettings: null,
    /** @type {PaginationList<SettingSelectItem> | null} */
    selectSettings: null,
  }),
  actions: {
    /**
     * Fetches a paginated list of settings.
     * @param {QueryableParams} [params={}]
     */
    async fetchPagedSettings(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await settingService.getPagedList(params);
        if (response.succeeded) {
          this.pagedSettings = response.data;
          this.settings = response.data?.items || [];
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch settings.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching paged settings:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches a select list of settings.
     * @param {QueryableParams} [params={}]
     */
    async fetchSelectSettings(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await settingService.getSelectList(params);
        if (response.succeeded) {
          this.selectSettings = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch select settings.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching select settings:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches details for a single setting by ID.
     * @param {string} id
     */
    async fetchSettingById(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await settingService.getById(id);
        if (response.succeeded) {
          this.selectedSetting = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch setting details.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching setting by ID:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Creates a new setting.
     * @param {SettingParameter} payload
     * @returns {Promise<boolean>}
     */
    async createSetting(payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await settingService.create(payload);
        if (response.succeeded) {
          this.fetchPagedSettings(); // Refresh the list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to create setting.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error creating setting:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Updates an existing setting.
     * @param {string} id
     * @param {SettingParameter} payload
     * @returns {Promise<boolean>}
     */
    async updateSetting(id, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await settingService.update(id, payload);
        if (response.succeeded) {
          this.fetchSettingById(id); // Refresh details
          this.fetchPagedSettings(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to update setting.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error updating setting:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Deletes a setting.
     * @param {string} id
     * @returns {Promise<boolean>}
     */
    async deleteSetting(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await settingService.delete(id);
        if (response.succeeded) {
          this.fetchPagedSettings(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to delete setting.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error deleting setting:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },
  },
});
