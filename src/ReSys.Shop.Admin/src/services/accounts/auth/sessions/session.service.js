// src/ReSys.Shop.Admin/src/service/accounts/auth/sessions/session.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/models/accounts/auth/auth.model').AuthenticationResult} AuthenticationResult
 * @typedef {import('@/models/auth.type').SessionResult} SessionResult
 * @typedef {import('@/.js').LogOutParam} LogOutParam
 * @typedef {import('@/models/auth.type').RefreshTokenParam} RefreshTokenParam
 */

const API_BASE_ROUTE = 'api/account/auth/session';
const httpClient = configureHttpClient();

export const sessionService = {
  /**
   * Retrieves the session information for the current user.
   * @returns {Promise<ApiResponse<SessionResult>>}
   */
  async get() {
    const response = await httpClient.get(API_BASE_ROUTE);
    return response.data;
  },

  /**
   * Refreshes the authentication token.
   * @param {RefreshTokenParam} payload
   * @returns {Promise<ApiResponse<AuthenticationResult>>}
   */
  async refresh(payload) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/refresh`, payload);
    return response.data;
  },

  /**
   * Logs out the currently authenticated user from the current session.
   * @param {LogOutParam} payload
   * @returns {Promise<ApiResponse<void>>}
   */
  async logoutMe(payload) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/logout/me`, payload);
    return response.data;
  },

  /**
   * Logs out the currently authenticated user from all sessions.
   * @param {LogOutParam} payload
   * @returns {Promise<ApiResponse<void>>}
   */
  async logoutAll(payload) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/logout/all`, payload);
    return response.data;
  },
};
