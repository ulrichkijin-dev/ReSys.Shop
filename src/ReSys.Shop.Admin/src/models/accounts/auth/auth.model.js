// src/ReSys.Shop.Admin/src/models/accounts/auth/auth.model.js

/**
 * @typedef {object} AuthenticationResult
 * @property {string} accessToken
 * @property {string} accessTokenExpiresAt - DateTimeOffset (ISO 8601)
 * @property {string} refreshToken
 * @property {string} refreshTokenExpiresAt - DateTimeOffset (ISO 8601)
 * @property {string} tokenType
 */

/**
 * @typedef {object} UserProfile
 * @property {string} email
 * @property {string | null} [firstName]
 * @property {string | null} [lastName]
 * @property {string | null} [profilePictureUrl]
 * @property {boolean} emailVerified
 * @property {boolean} hasExternalLogins
 * @property {string[]} externalProviders
 * @property {Object.<string, string>} additionalClaims
 */
