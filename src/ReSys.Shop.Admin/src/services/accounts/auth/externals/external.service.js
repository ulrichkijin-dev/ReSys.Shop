// src/ReSys.Shop.Admin/src/service/accounts/auth/externals/external.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/.js').OAuthConfigResult} OAuthConfigResult
 * @typedef {import('@/.js').ExternalProviderResult} ExternalProviderResult
 * @typedef {import('@/models/accounts/auth/externals/external.model').ExchangeTokenParam} ExchangeTokenParam
 * @typedef {import('@/models/accounts/auth/auth.model').AuthenticationResult} AuthenticationResult
 * @typedef {import('@/.js').VerifyExternalTokenParam} VerifyExternalTokenParam
 */

const API_BASE_ROUTE = 'api/account/auth/external';
const httpClient = configureHttpClient(); // Get the configured instance

export const externalService = {
  /**
   * Retrieves OAuth configuration for a specific provider.
   * @param {string} provider - The name of the OAuth provider (e.g., 'google', 'facebook').
   * @returns {Promise<ApiResponse<OAuthConfigResult>>}
   */
  async getOAuthConfig(provider) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${provider}/configuration`);
    return response.data;
  },

  /**
   * Retrieves a list of available external authentication providers.
   * @returns {Promise<ApiResponse<ExternalProviderResult[]>>}
   */
  async getExternalProviders() {
    const response = await httpClient.get(`${API_BASE_ROUTE}/providers`);
    return response.data;
  },

  /**
   * Exchanges an external provider token for application tokens.
   * @param {string} provider - The name of the OAuth provider.
   * @param {ExchangeTokenParam} payload - The token exchange parameters.
   * @returns {Promise<ApiResponse<AuthenticationResult>>}
   */
  async exchangeToken(provider, payload) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/${provider}/exchange`, payload);
    return response.data;
  },

  /**
   * Verifies an external provider token without creating a session.
   * @param {string} provider - The name of the OAuth provider.
   * @param {VerifyExternalTokenParam} payload - The token verification parameters.
   * @returns {Promise<ApiResponse<any>>}
   */
  async verifyExternalToken(provider, payload) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/${provider}/verify`, payload);
    return response.data;
  },
};
