// src/ReSys.Shop.Admin/src/stores/admin/identity/users/user.store.js

import { defineStore } from 'pinia';
import { userService } from '@/services';

/**
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').UserParameter} UserParameter
 * @typedef {import('@/.js').UserSelectItem} UserSelectItem
 * @typedef {import('@/.js').UserListItem} UserListItem
 * @typedef {import('@/.js').UserDetail} UserDetail
 * @typedef {import('@/.js').UserRoleItem} UserRoleItem
 * @typedef {import('@/.js').UserPermissionItem} UserPermissionItem
 * @typedef {import('@/.js').AssignRoleToUserParameter} AssignRoleToUserParameter
 * @typedef {import('@/.js').UnassignRoleFromUserParameter} UnassignRoleFromUserParameter
 * @typedef {import('@/.js').AssignPermissionToUserParameter} AssignPermissionToUserParameter
 * @typedef {import('@/.js').UnassignPermissionFromUserParameter} UnassignPermissionFromUserParameter
 */

export const useUserStore = defineStore('admin-user', {
  state: () => ({
    /** @type {UserListItem[]} */
    users: [],
    /** @type {UserDetail | null} */
    selectedUser: null,
    /** @type {boolean} */
    loading: false,
    /** @type {boolean} */
    error: false,
    /** @type {string | null} */
    errorMessage: null,
    /** @type {PaginationList<UserListItem> | null} */
    pagedUsers: null,
    /** @type {PaginationList<UserSelectItem> | null} */
    selectUsers: null,
    /** @type {PaginationList<UserRoleItem> | null} */
    userRoles: null,
    /** @type {PaginationList<UserPermissionItem> | null} */
    userPermissions: null,
  }),
  actions: {
    /**
     * Fetches a paginated list of users.
     * @param {QueryableParams} [params={}]
     */
    async fetchPagedUsers(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await userService.getPagedList(params);
        if (response.succeeded) {
          this.pagedUsers = response.data;
          this.users = response.data?.items || [];
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch users.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching paged users:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches a select list of users.
     * @param {QueryableParams} [params={}]
     */
    async fetchSelectUsers(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await userService.getSelectList(params);
        if (response.succeeded) {
          this.selectUsers = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch select users.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching select users:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches details for a single user by ID.
     * @param {string} id
     */
    async fetchUserById(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await userService.getById(id);
        if (response.succeeded) {
          this.selectedUser = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch user details.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching user by ID:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Creates a new user.
     * @param {UserParameter} payload
     * @returns {Promise<boolean>}
     */
    async createUser(payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await userService.create(payload);
        if (response.succeeded) {
          this.fetchPagedUsers(); // Refresh the list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to create user.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error creating user:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Updates an existing user.
     * @param {string} id
     * @param {UserParameter} payload
     * @returns {Promise<boolean>}
     */
    async updateUser(id, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await userService.update(id, payload);
        if (response.succeeded) {
          this.fetchUserById(id); // Refresh details
          this.fetchPagedUsers(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to update user.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error updating user:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Deletes a user.
     * @param {string} id
     * @returns {Promise<boolean>}
     */
    async deleteUser(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await userService.delete(id);
        if (response.succeeded) {
          this.fetchPagedUsers(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to delete user.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error deleting user:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches all roles assigned to a specific user.
     * @param {string} userId
     * @param {QueryableParams} [params={}]
     */
    async fetchUserRoles(userId, params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await userService.getRoles(userId, params);
        if (response.succeeded) {
          this.userRoles = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || `Failed to fetch roles for user ${userId}.`;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error(`Error fetching roles for user ${userId}:`, err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Assigns a role to a user.
     * @param {string} userId
     * @param {AssignRoleToUserParameter} payload
     * @returns {Promise<boolean>}
     */
    async assignRoleToUser(userId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await userService.assignRole(userId, payload);
        if (response.succeeded) {
          this.fetchUserRoles(userId); // Refresh roles for the user
          this.fetchUserById(userId); // Refresh user details
          this.fetchPagedUsers(); // Refresh paged list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || `Failed to assign role to user ${userId}.`;
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error(`Error assigning role to user ${userId}:`, err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Unassigns a role from a user.
     * @param {string} userId
     * @param {UnassignRoleFromUserParameter} payload
     * @returns {Promise<boolean>}
     */
    async unassignRoleFromUser(userId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await userService.unassignRole(userId, payload);
        if (response.succeeded) {
          this.fetchUserRoles(userId); // Refresh roles for the user
          this.fetchUserById(userId); // Refresh user details
          this.fetchPagedUsers(); // Refresh paged list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || `Failed to unassign role from user ${userId}.`;
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error(`Error unassigning role from user ${userId}:`, err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches all permissions (claims) assigned to a specific user.
     * @param {string} userId
     * @param {QueryableParams} [params={}]
     */
    async fetchUserPermissions(userId, params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await userService.getPermissions(userId, params);
        if (response.succeeded) {
          this.userPermissions = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || `Failed to fetch permissions for user ${userId}.`;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error(`Error fetching permissions for user ${userId}:`, err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Assigns a permission to a user.
     * @param {string} userId
     * @param {AssignPermissionToUserParameter} payload
     * @returns {Promise<boolean>}
     */
    async assignPermissionToUser(userId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await userService.assignPermission(userId, payload);
        if (response.succeeded) {
          this.fetchUserPermissions(userId); // Refresh permissions for the user
          this.fetchUserById(userId); // Refresh user details
          this.fetchPagedUsers(); // Refresh paged list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || `Failed to assign permission to user ${userId}.`;
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error(`Error assigning permission to user ${userId}:`, err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Unassigns a permission from a user.
     * @param {string} userId
     * @param {UnassignPermissionFromUserParameter} payload
     * @returns {Promise<boolean>}
     */
    async unassignPermissionFromUser(userId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await userService.unassignPermission(userId, payload);
        if (response.succeeded) {
          this.fetchUserPermissions(userId); // Refresh permissions for the user
          this.fetchUserById(userId); // Refresh user details
          this.fetchPagedUsers(); // Refresh paged list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || `Failed to unassign permission from user ${userId}.`;
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error(`Error unassigning permission from user ${userId}:`, err);
        return false;
      } finally {
        this.loading = false;
      }
    },
  },
});
