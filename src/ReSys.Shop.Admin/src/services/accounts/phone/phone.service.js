// src/ReSys.Shop.Admin/src/service/accounts/phone/phone.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/.js').ChangePhoneParam} ChangePhoneParam
 * @typedef {import('@/.js').ConfirmPhoneParam} ConfirmPhoneParam
 * @typedef {import('@/.js').ResendPhoneVerificationParam} ResendPhoneVerificationParam
 */

const API_BASE_ROUTE = 'api/account/phone';
const httpClient = configureHttpClient();

export const phoneService = {
  /**
   * Changes a user's phone number.
   * @param {ChangePhoneParam} payload
   * @returns {Promise<ApiResponse<any>>}
   */
  async change(payload) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/change`, payload);
    return response.data;
  },

  /**
   * Confirms a user's phone change.
   * @param {ConfirmPhoneParam} payload
   * @returns {Promise<ApiResponse<any>>}
   */
  async confirm(payload) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/confirm`, payload);
    return response.data;
  },

  /**
   * Resends the phone verification SMS.
   * @param {ResendPhoneVerificationParam} payload
   * @returns {Promise<ApiResponse<any>>}
   */
  async resend(payload) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/resend`, payload);
    return response.data;
  },
};
