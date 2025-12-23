// src/ReSys.Shop.Admin/src/stores/accounts/profile/profile.store.js

import { defineStore } from 'pinia';
import { profileService } from '@/services';

/**
 * @typedef {import('@/models/accounts/profile/profile.model').ProfileResult} ProfileResult
 * @typedef {import('@/models/accounts/profile/profile.model').ProfileParam} ProfileParam
 */

export const useProfileStore = defineStore('admin-profile-account', {
  state: () => ({
    /** @type {ProfileResult | null} */
    profile: null,
    /** @type {boolean} */
    loading: false,
    /** @type {boolean} */
    error: false,
    /** @type {string | null} */
    errorMessage: null,
  }),
  actions: {
    /**
     * Fetches the profile information for the current user.
     */
    async fetchProfile() {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await profileService.get();
        if (response.succeeded) {
          this.profile = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch profile.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching profile:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Updates the profile information for the current user.
     * @param {ProfileParam} payload
     * @returns {Promise<boolean>}
     */
    async updateProfile(payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await profileService.update(payload);
        if (response.succeeded) {
          await this.fetchProfile(); // Refresh profile after update
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to update profile.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error updating profile:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },
  },
});
