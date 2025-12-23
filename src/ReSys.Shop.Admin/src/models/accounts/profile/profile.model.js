// src/ReSys.Shop.Admin/src/models/accounts/profile/profile.model.js

/**
 * @typedef {object} ProfileParam
 * @property {string} username
 * @property {string | null} [firstName]
 * @property {string | null} [lastName]
 * @property {string | null} [dateOfBirth] - DateTimeOffset (ISO 8601)
 * @property {string | null} [profileImagePath]
 */

/**
 * @typedef {object} ProfileResult
 * @property {string} id - Guid
 * @property {string} email
 * @property {string | null} [phoneNumber]
 * @property {string} username
 * @property {string | null} [firstName]
 * @property {string | null} [lastName]
 * @property {string | null} [dateOfBirth] - DateTimeOffset (ISO 8601)
 * @property {string | null} [profileImagePath]
 * @property {string | null} [lastSignInAt] - DateTimeOffset (ISO 8601)
 * @property {string | null} [lastSignInIp]
 */
