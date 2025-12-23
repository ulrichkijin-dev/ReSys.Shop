// src/ReSys.Shop.Admin/src/stores/admin/identity/permissions/permission.store.js

import { defineStore } from 'pinia';
import { permissionService } from '@/services';

/**
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').PermissionSelectItem} PermissionSelectItem
 * @typedef {import('@/.js').PermissionListItem} PermissionListItem
 * @typedef {import('@/.js').PermissionDetail} PermissionDetail
 */

export const usePermissionStore = defineStore('admin-permission', {
  state: () => ({
    /** @type {PermissionListItem[]} */
    permissions: [],
    /** @type {PermissionDetail | null} */
    selectedPermission: null,
    /** @type {boolean} */
    loading: false,
    /** @type {boolean} */
    error: false,
    /** @type {string | null} */
    errorMessage: null,
    /** @type {PaginationList<PermissionListItem> | null} */
    pagedPermissions: null,
    /** @type {PaginationList<PermissionSelectItem> | null} */
    selectPermissions: null,
  }),
  actions: {
    /**
     * Fetches a paginated list of permissions.
     * @param {QueryableParams} [params={}]
     */
    async fetchPagedPermissions(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await permissionService.getPagedList(params);
        if (response.succeeded) {
          this.pagedPermissions = response.data;
          this.permissions = response.data?.items || [];
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch permissions.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching paged permissions:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches a select list of permissions.
     * @param {QueryableParams} [params={}]
     */
    async fetchSelectPermissions(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await permissionService.getSelectList(params);
        if (response.succeeded) {
          this.selectPermissions = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch select permissions.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching select permissions:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches details for a single permission by ID.
     * @param {string} id
     */
    async fetchPermissionById(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await permissionService.getById(id);
        if (response.succeeded) {
          this.selectedPermission = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch permission details.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching permission by ID:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches details for a single permission by Name.
     * @param {string} name
     */
    async fetchPermissionByName(name) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await permissionService.getByName(name);
        if (response.succeeded) {
          this.selectedPermission = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch permission details by name.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching permission by name:', err);
      } finally {
        this.loading = false;
      }
    },
  },
});
