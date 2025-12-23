// src/ReSys.Shop.Admin/src/stores/accounts/email/email.store.js

import { defineStore } from 'pinia';
import { emailService } from '@/services';

/**
 * @typedef {import('@/.js').ChangeEmailParam} ChangeEmailParam
 * @typedef {import('@/.js').ConfirmEmailParam} ConfirmEmailParam
 * @typedef {import('@/.js').ResendConfirmationParam} ResendConfirmationParam
 */

export const useEmailStore = defineStore('admin-email-account', {
  state: () => ({
    loading: false,
    error: false,
    errorMessage: null,
  }),
  actions: {
    async changeEmail(payload) {
      this.loading = true;
      this.error = false;
      try {
        const response = await emailService.change(payload);
        if (response.succeeded) return true;
        this.error = true;
        this.errorMessage = response.message;
        return false;
      } finally {
        this.loading = false;
      }
    },
    async confirmEmail(payload) {
      this.loading = true;
      this.error = false;
      try {
        const response = await emailService.confirm(payload);
        if (response.succeeded) return true;
        this.error = true;
        this.errorMessage = response.message;
        return false;
      } finally {
        this.loading = false;
      }
    },
    async resendConfirmation(payload) {
      this.loading = true;
      this.error = false;
      try {
        const response = await emailService.resendConfirmation(payload);
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
