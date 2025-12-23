// src/ReSys.Shop.Admin/src/stores/accounts/password/password.store.js

import { defineStore } from 'pinia';
import { passwordService } from '@/services';

/**
 * @typedef {import('@/.js').ChangePasswordParam} ChangePasswordParam
 * @typedef {import('@/.js').ForgotPasswordParam} ForgotPasswordParam
 * @typedef {import('@/.js').ResetPasswordParam} ResetPasswordParam
 */

export const usePasswordStore = defineStore('admin-password-account', {
  state: () => ({
    loading: false,
    error: false,
    errorMessage: null,
  }),
  actions: {
    async changePassword(payload) {
      this.loading = true;
      this.error = false;
      try {
        const response = await passwordService.change(payload);
        if (response.succeeded) return true;
        this.error = true;
        this.errorMessage = response.message;
        return false;
      } finally {
        this.loading = false;
      }
    },
    async forgotPassword(payload) {
      this.loading = true;
      this.error = false;
      try {
        const response = await passwordService.forgot(payload);
        if (response.succeeded) return true;
        this.error = true;
        this.errorMessage = response.message;
        return false;
      } finally {
        this.loading = false;
      }
    },
    async resetPassword(payload) {
      this.loading = true;
      this.error = false;
      try {
        const response = await passwordService.reset(payload);
        if (response.succeeded) return true;
        this.error = true;
        this.errorMessage = response.message;
        return false;
      } finally {
        this.loading = false;
      }
    },
  },
});
