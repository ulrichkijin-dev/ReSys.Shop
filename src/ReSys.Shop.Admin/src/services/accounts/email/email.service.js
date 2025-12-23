// src/ReSys.Shop.Admin/src/service/accounts/email/email.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/.js').ChangeEmailParam} ChangeEmailParam
 * @typedef {import('@/.js').ConfirmEmailParam} ConfirmEmailParam
 * @typedef {import('@/.js').ResendConfirmationParam} ResendConfirmationParam
 */

const API_BASE_ROUTE = 'api/account/email';
const httpClient = configureHttpClient();

export const emailService = {
  /**
   * Changes a user's email address.
   * @param {ChangeEmailParam} payload
   * @returns {Promise<ApiResponse<any>>}
   */
  async change(payload) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/change`, payload);
    return response.data;
  },

  /**
   * Confirms a user's email address.
   * @param {ConfirmEmailParam} payload
   * @returns {Promise<ApiResponse<any>>}
   */
  async confirm(payload) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/confirm`, payload);
    return response.data;
  },

  /**
   * Resends a user's email confirmation link.
   * @param {ResendConfirmationParam} payload
   * @returns {Promise<ApiResponse<any>>}
   */
  async resendConfirmation(payload) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/resend-confirmation`, payload);
    return response.data;
  },
};
