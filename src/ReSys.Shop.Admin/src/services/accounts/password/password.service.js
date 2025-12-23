// src/ReSys.Shop.Admin/src/service/accounts/password/password.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/.js').ChangePasswordParam} ChangePasswordParam
 * @typedef {import('@/.js').ForgotPasswordParam} ForgotPasswordParam
 * @typedef {import('@/.js').ResetPasswordParam} ResetPasswordParam
 */

const API_BASE_ROUTE = 'api/account/password';
const httpClient = configureHttpClient();

export const passwordService = {
  /**
   * Changes a user's password.
   * @param {ChangePasswordParam} payload
   * @returns {Promise<ApiResponse<any>>}
   */
  async change(payload) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/change`, payload);
    return response.data;
  },

  /**
   * Initiates the password reset process.
   * @param {ForgotPasswordParam} payload
   * @returns {Promise<ApiResponse<any>>}
   */
  async forgot(payload) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/forgot`, payload);
    return response.data;
  },

  /**
   * Resets a user's password.
   * @param {ResetPasswordParam} payload
   * @returns {Promise<ApiResponse<any>>}
   */
  async reset(payload) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/reset`, payload);
    return response.data;
  },
};
