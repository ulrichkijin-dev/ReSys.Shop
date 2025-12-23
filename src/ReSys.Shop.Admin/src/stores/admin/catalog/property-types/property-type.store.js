// src/ReSys.Shop.Admin/src/stores/admin/catalog/property-types/property-type.store.js

import { defineStore } from 'pinia';
import { propertyTypeService } from '@/services';

/**
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').PropertyTypeParameter} PropertyTypeParameter
 * @typedef {import('@/.js').PropertyTypeSelectItem} PropertyTypeSelectItem
 * @typedef {import('@/.js').PropertyTypeListItem} PropertyTypeListItem
 * @typedef {import('@/.js').PropertyTypeDetail} PropertyTypeDetail
 * @typedef {import('@/.js').UpdateDisplayOnParameter} UpdateDisplayOnParameter
 */

export const usePropertyTypeStore = defineStore('admin-property-type', {
  state: () => ({
    /** @type {PropertyTypeListItem[]} */
    propertyTypes: [],
    /** @type {PropertyTypeDetail | null} */
    selectedPropertyType: null,
    /** @type {boolean} */
    loading: false,
    /** @type {boolean} */
    error: false,
    /** @type {string | null} */
    errorMessage: null,
    /** @type {PaginationList<PropertyTypeListItem> | null} */
    pagedPropertyTypes: null,
    /** @type {PaginationList<PropertyTypeSelectItem> | null} */
    selectPropertyTypes: null,
  }),
  actions: {
    /**
     * Fetches a paginated list of property types.
     * @param {QueryableParams} [params={}]
     */
    async fetchPagedPropertyTypes(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await propertyTypeService.getPagedList(params);
        if (response.succeeded) {
          this.pagedPropertyTypes = response.data;
          this.propertyTypes = response.data?.items || [];
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch property types.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching paged property types:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches a select list of property types.
     * @param {QueryableParams} [params={}]
     */
    async fetchSelectPropertyTypes(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await propertyTypeService.getSelectList(params);
        if (response.succeeded) {
          this.selectPropertyTypes = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch select property types.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching select property types:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches details for a single property type by ID.
     * @param {string} id
     */
    async fetchPropertyTypeById(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await propertyTypeService.getById(id);
        if (response.succeeded) {
          this.selectedPropertyType = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch property type details.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching property type by ID:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Creates a new property type.
     * @param {PropertyTypeParameter} payload
     * @returns {Promise<boolean>}
     */
    async createPropertyType(payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await propertyTypeService.create(payload);
        if (response.succeeded) {
          this.fetchPagedPropertyTypes(); // Refresh the list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to create property type.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error creating property type:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Updates an existing property type.
     * @param {string} id
     * @param {PropertyTypeParameter} payload
     * @returns {Promise<boolean>}
     */
    async updatePropertyType(id, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await propertyTypeService.update(id, payload);
        if (response.succeeded) {
          this.fetchPropertyTypeById(id); // Refresh details
          this.fetchPagedPropertyTypes(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to update property type.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error updating property type:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Deletes a property type.
     * @param {string} id
     * @returns {Promise<boolean>}
     */
    async deletePropertyType(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await propertyTypeService.delete(id);
        if (response.succeeded) {
          this.fetchPagedPropertyTypes(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to delete property type.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error deleting property type:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Updates the 'DisplayOn' setting for a property type.
     * @param {string} id
     * @param {UpdateDisplayOnParameter} payload
     * @returns {Promise<boolean>}
     */
    async updatePropertyTypeDisplayOn(id, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await propertyTypeService.updateDisplayOn(id, payload);
        if (response.succeeded) {
          this.fetchPropertyTypeById(id); // Refresh details
          this.fetchPagedPropertyTypes(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to update property type display on setting.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error updating property type display on setting:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },
  },
});
