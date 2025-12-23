// src/ReSys.Shop.Admin/src/stores/accounts/auth/auth.store.js

import { defineStore } from 'pinia';
import { internalService } from '@/services/accounts/auth/internals/internal.service';
import { externalService } from '@/services/accounts/auth/externals/external.service';
import { sessionService } from '@/services/accounts/auth/sessions/session.service';

/**
 * @typedef {import('@/models/accounts/auth/auth.model').AuthenticationResult} AuthenticationResult
 * @typedef {import('@/models/accounts/auth/auth.model').UserProfile} UserProfile
 * @typedef {import('@/models/auth.type').LoginParam} LoginParam
 * @typedef {import('@/models/auth.type').RegisterParam} RegisterParam
 * @typedef {import('@/models/accounts/profile/profile.model').ProfileResult} ProfileResult
 * @typedef {import('@/models/auth.type').SessionResult} SessionResult
 */

export const useAuthStore = defineStore('auth', {
  state: () => ({
    /** @type {AuthenticationResult | null} */
    authResult: null,
    /** @type {UserProfile | null} */
    userProfile: null,
    /** @type {SessionResult | null} */
    session: null,
    /** @type {boolean} */
    isAuthenticated: false,
    /** @type {boolean} */
    loading: false,
    /** @type {boolean} */
    error: false,
    /** @type {string | null} */
    errorMessage: null,
  }),
  getters: {
    /** @param {ReturnType<typeof useAuthStore>} state */
    getAccessToken: (state) => state.authResult?.accessToken,
    /** @param {ReturnType<typeof useAuthStore>} state */
    getRefreshToken: (state) => state.authResult?.refreshToken,
    /** @param {ReturnType<typeof useAuthStore>} state */
    getUserEmail: (state) => state.userProfile?.email || state.session?.email,
    /** @param {ReturnType<typeof useAuthStore>} state */
    getUserName: (state) => state.userProfile?.userName || state.session?.userName || state.userProfile?.email || state.session?.email,
  },
  actions: {
    /**
     * Initializes the store from local storage (e.g., on app start).
     */
    initializeAuth() {
      const accessToken = localStorage.getItem('accessToken');
      const refreshToken = localStorage.getItem('refreshToken');
      if (accessToken && refreshToken) {
        this.isAuthenticated = true;
        this.authResult = {
          accessToken: accessToken,
          refreshToken: refreshToken,
          accessTokenExpiresAt: '',
          refreshTokenExpiresAt: '',
          tokenType: 'Bearer',
        };
        this.fetchSession();
      } else {
        this.isAuthenticated = false;
        this.authResult = null;
        this.userProfile = null;
        this.session = null;
      }
    },

    /**
     * Handles user login.
     * @param {LoginParam} payload
     * @returns {Promise<boolean>}
     */
    async login(payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await internalService.login(payload);
        if (response.succeeded && response.data) {
          this.authResult = response.data;
          this.isAuthenticated = true;
          localStorage.setItem('accessToken', response.data.accessToken);
          localStorage.setItem('refreshToken', response.data.refreshToken);
          await this.fetchSession();
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Login failed.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred during login.';
        console.error('Login error:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Handles external token exchange.
     * @param {string} provider
     * @param {import('@/models/accounts/auth/externals/external.model').ExchangeTokenParam} payload
     * @returns {Promise<boolean>}
     */
    async exchangeExternalToken(provider, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await externalService.exchangeToken(provider, payload);
        if (response.succeeded && response.data) {
          this.authResult = response.data;
          this.isAuthenticated = true;
          localStorage.setItem('accessToken', response.data.accessToken);
          localStorage.setItem('refreshToken', response.data.refreshToken);
          await this.fetchSession();
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'External login failed.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred during external login.';
        console.error('External login error:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Handles user registration.
     * @param {RegisterParam} payload
     * @returns {Promise<boolean>}
     */
    async register(payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await internalService.register(payload);
        if (response.succeeded) {
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Registration failed.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred during registration.';
        console.error('Registration error:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches current session information.
     */
    async fetchSession() {
      try {
        const response = await sessionService.get();
        if (response.succeeded) {
          this.session = response.data;
        }
      } catch (err) {
        console.error('Fetch session error:', err);
      }
    },

    /**
     * Refreshes the authentication token.
     */
    async refreshToken() {
      if (!this.authResult?.refreshToken) return false;
      try {
        const response = await sessionService.refresh({
          refreshToken: this.authResult.refreshToken,
          rememberMe: true, // Should ideally be tracked
        });
        if (response.succeeded && response.data) {
          this.authResult = response.data;
          localStorage.setItem('accessToken', response.data.accessToken);
          localStorage.setItem('refreshToken', response.data.refreshToken);
          await this.fetchSession();
          return true;
        }
        this.logout();
        return false;
      } catch (err) {
        console.error('Refresh token error:', err);
        this.logout();
        return false;
      }
    },

    /**
     * Logs out the user.
     */
    async logout() {
      if (this.authResult?.refreshToken) {
        await sessionService.logoutMe({ refreshToken: this.authResult.refreshToken });
      }
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      this.authResult = null;
      this.userProfile = null;
      this.session = null;
      this.isAuthenticated = false;
    },
  },
});
