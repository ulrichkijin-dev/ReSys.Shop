// src/ReSys.Shop.Admin/src/service/accounts/profile/profile.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/models/accounts/profile/profile.model').ProfileResult} ProfileResult
 * @typedef {import('@/models/accounts/profile/profile.model').ProfileParam} ProfileParam
 */

const API_BASE_ROUTE = 'api/account/profile';
const httpClient = configureHttpClient();

export const profileService = {
  /**
   * Retrieves the profile information for the current user.
   * @returns {Promise<ApiResponse<ProfileResult>>}
   */
  async get() {
    const response = await httpClient.get(API_BASE_ROUTE);
    return response.data;
  },

  /**
   * Updates the profile information for the current user.
   * @param {ProfileParam} payload
   * @returns {Promise<ApiResponse<void>>}
   */
  async update(payload) {
    const response = await httpClient.put(API_BASE_ROUTE, payload);
    return response.data;
  },
};
