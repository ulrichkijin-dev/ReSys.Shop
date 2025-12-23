// src/ReSys.Shop.Admin/src/service/accounts/auth/internals/internal.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/models/auth.type').LoginParam} LoginParam
 * @typedef {import('@/models/auth.type').RegisterParam} RegisterParam
 * @typedef {import('@/models/accounts/auth/auth.model').AuthenticationResult} AuthenticationResult
 * @typedef {import('@/models/accounts/profile/profile.model').ProfileResult} ProfileResult
 */

const API_BASE_ROUTE = 'api/account/auth/internal';
const httpClient = configureHttpClient(); // Get the configured instance

export const internalService = {
  /**
   * Registers a new user.
   * @param {RegisterParam} payload - The registration data.
   * @returns {Promise<ApiResponse<ProfileResult>>}
   */
  async register(payload) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/register`, payload);
    return response.data;
  },

  /**
   * Authenticates a user using credential + password.
   * @param {LoginParam} payload - The login credentials.
   * @returns {Promise<ApiResponse<AuthenticationResult>>}
   */
  async login(payload) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/login`, payload);
    return response.data;
  },
};
