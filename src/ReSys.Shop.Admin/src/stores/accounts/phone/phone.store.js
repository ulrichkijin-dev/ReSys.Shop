// src/ReSys.Shop.Admin/src/stores/accounts/phone/phone.store.js

import { defineStore } from 'pinia';
import { phoneService } from '@/services';

/**
 * @typedef {import('@/.js').ChangePhoneParam} ChangePhoneParam
 * @typedef {import('@/.js').ConfirmPhoneParam} ConfirmPhoneParam
 * @typedef {import('@/.js').ResendPhoneVerificationParam} ResendPhoneVerificationParam
 */

export const usePhoneStore = defineStore('admin-phone-account', {
  state: () => ({
    loading: false,
    error: false,
    errorMessage: null,
  }),
  actions: {
    async changePhone(payload) {
      this.loading = true;
      this.error = false;
      try {
        const response = await phoneService.change(payload);
        if (response.succeeded) return true;
        this.error = true;
        this.errorMessage = response.message;
        return false;
      } finally {
        this.loading = false;
      }
    },
    async confirmPhone(payload) {
      this.loading = true;
      this.error = false;
      try {
        const response = await phoneService.confirm(payload);
        if (response.succeeded) return true;
        this.error = true;
        this.errorMessage = response.message;
        return false;
      } finally {
        this.loading = false;
      }
    },
    async resendVerification(payload) {
      this.loading = true;
      this.error = false;
      try {
        const response = await phoneService.resend(payload);
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
