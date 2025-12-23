// src/ReSys.Shop.Admin/src/models/accounts/auth/externals/external.model.js

/**
 * @typedef {object} ExchangeTokenParam
 * @property {string} provider
 * @property {string | null} [accessToken]
 * @property {string | null} [idToken]
 * @property {string | null} [authorizationCode]
 * @property {string | null} [redirectUri]
 * @property {boolean} rememberMe
 */

/**
 * @typedef {object} VerifyExternalTokenParam
 * @property {string | null} [accessToken]
 * @property {string | null} [idToken]
 */

/**
 * @typedef {object} OAuthConfigResult
 * @property {string} provider
 * @property {string} clientId
 * @property {string} authorizationUrl
 * @property {string} tokenUrl
 * @property {string[]} scopes
 * @property {string} responseType
 * @property {Object.<string, string>} additionalParameters
 * @property {string} tokenExchangeUrl
 * @property {string} providerName
 * @property {bool} requiresPKCE
 */

/**
 * @typedef {object} ExternalProviderResult
 * @property {string} provider
 * @property {string} displayName
 * @property {string} loginUrl
 * @property {string | null} [iconUrl]
 * @property {boolean} isEnabled
 * @property {string[]} requiredScopes
 * @property {string} configurationUrl
 */
