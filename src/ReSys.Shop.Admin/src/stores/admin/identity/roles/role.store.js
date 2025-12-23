// src/ReSys.Shop.Admin/src/stores/admin/identity/roles/role.store.js

import { defineStore } from 'pinia';
import { roleService } from '@/services';

/**
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').RoleParameter} RoleParameter
 * @typedef {import('@/.js').RoleSelectItem} RoleSelectItem
 * @typedef {import('@/.js').RoleListItem} RoleListItem
 * @typedef {import('@/.js').RoleDetail} RoleDetail
 * @typedef {import('@/.js').RoleUserItem} RoleUserItem
 * @typedef {import('@/.js').RolePermissionItem} RolePermissionItem
 * @typedef {import('@/.js').AssignUserToRoleParameter} AssignUserToRoleParameter
 * @typedef {import('@/.js').UnassignUserFromRoleParameter} UnassignUserFromRoleParameter
 * @typedef {import('@/.js').AssignPermissionToRoleParameter} AssignPermissionToRoleParameter
 * @typedef {import('@/.js').UnassignPermissionFromRoleParameter} UnassignPermissionFromRoleParameter
 */

export const useRoleStore = defineStore('admin-role', {
  state: () => ({
    /** @type {RoleListItem[]} */
    roles: [],
    /** @type {RoleDetail | null} */
    selectedRole: null,
    /** @type {boolean} */
    loading: false,
    /** @type {boolean} */
    error: false,
    /** @type {string | null} */
    errorMessage: null,
    /** @type {PaginationList<RoleListItem> | null} */
    pagedRoles: null,
    /** @type {PaginationList<RoleSelectItem> | null} */
    selectRoles: null,
    /** @type {PaginationList<RoleUserItem> | null} */
    roleUsers: null,
    /** @type {PaginationList<RolePermissionItem> | null} */
    rolePermissions: null,
  }),
  actions: {
    /**
     * Fetches a paginated list of roles.
     * @param {QueryableParams} [params={}]
     */
    async fetchPagedRoles(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await roleService.getPagedList(params);
        if (response.succeeded) {
          this.pagedRoles = response.data;
          this.roles = response.data?.items || [];
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch roles.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching paged roles:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches a select list of roles.
     * @param {QueryableParams} [params={}]
     */
    async fetchSelectRoles(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await roleService.getSelectList(params);
        if (response.succeeded) {
          this.selectRoles = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch select roles.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching select roles:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches details for a single role by ID.
     * @param {string} id
     */
    async fetchRoleById(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await roleService.getById(id);
        if (response.succeeded) {
          this.selectedRole = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch role details.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching role by ID:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Creates a new role.
     * @param {RoleParameter} payload
     * @returns {Promise<boolean>}
     */
    async createRole(payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await roleService.create(payload);
        if (response.succeeded) {
          this.fetchPagedRoles(); // Refresh the list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to create role.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error creating role:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Updates an existing role.
     * @param {string} id
     * @param {RoleParameter} payload
     * @returns {Promise<boolean>}
     */
    async updateRole(id, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await roleService.update(id, payload);
        if (response.succeeded) {
          this.fetchRoleById(id); // Refresh details
          this.fetchPagedRoles(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to update role.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error updating role:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Deletes a role.
     * @param {string} id
     * @returns {Promise<boolean>}
     */
    async deleteRole(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await roleService.delete(id);
        if (response.succeeded) {
          this.fetchPagedRoles(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to delete role.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error deleting role:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches a paginated list of users assigned to a specific role.
     * @param {string} roleId
     * @param {QueryableParams} [params={}]
     */
    async fetchRoleUsers(roleId, params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await roleService.getUsers(roleId, params);
        if (response.succeeded) {
          this.roleUsers = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || `Failed to fetch users for role ${roleId}.`;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error(`Error fetching users for role ${roleId}:`, err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Assigns a user to a role.
     * @param {string} roleId
     * @param {AssignUserToRoleParameter} payload
     * @returns {Promise<boolean>}
     */
    async assignUserToRole(roleId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await roleService.assignUser(roleId, payload);
        if (response.succeeded) {
          this.fetchRoleUsers(roleId); // Refresh users for the role
          this.fetchRoleById(roleId); // Update user count in role detail
          this.fetchPagedRoles(); // Update user count in paged list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || `Failed to assign user to role ${roleId}.`;
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error(`Error assigning user to role ${roleId}:`, err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Unassigns a user from a role.
     * @param {string} roleId
     * @param {UnassignUserFromRoleParameter} payload
     * @returns {Promise<boolean>}
     */
    async unassignUserFromRole(roleId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await roleService.unassignUser(roleId, payload);
        if (response.succeeded) {
          this.fetchRoleUsers(roleId); // Refresh users for the role
          this.fetchRoleById(roleId); // Update user count in role detail
          this.fetchPagedRoles(); // Update user count in paged list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || `Failed to unassign user from role ${roleId}.`;
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error(`Error unassigning user from role ${roleId}:`, err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches a list of permissions assigned to a specific role.
     * @param {string} roleId
     * @param {QueryableParams} [params={}]
     */
    async fetchRolePermissions(roleId, params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await roleService.getPermissions(roleId, params);
        if (response.succeeded) {
          this.rolePermissions = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || `Failed to fetch permissions for role ${roleId}.`;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error(`Error fetching permissions for role ${roleId}:`, err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Assigns a permission to a role.
     * @param {string} roleId
     * @param {AssignPermissionToRoleParameter} payload
     * @returns {Promise<boolean>}
     */
    async assignPermissionToRole(roleId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await roleService.assignPermission(roleId, payload);
        if (response.succeeded) {
          this.fetchRolePermissions(roleId); // Refresh permissions for the role
          this.fetchRoleById(roleId); // Update permission count in role detail
          this.fetchPagedRoles(); // Update permission count in paged list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || `Failed to assign permission to role ${roleId}.`;
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error(`Error assigning permission to role ${roleId}:`, err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Unassigns a permission from a role.
     * @param {string} roleId
     * @param {UnassignPermissionFromRoleParameter} payload
     * @returns {Promise<boolean>}
     */
    async unassignPermissionFromRole(roleId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await roleService.unassignPermission(roleId, payload);
        if (response.succeeded) {
          this.fetchRolePermissions(roleId); // Refresh permissions for the role
          this.fetchRoleById(roleId); // Update permission count in role detail
          this.fetchPagedRoles(); // Update permission count in paged list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || `Failed to unassign permission from role ${roleId}.`;
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error(`Error unassigning permission from role ${roleId}:`, err);
        return false;
      } finally {
        this.loading = false;
      }
    },
  },
});
