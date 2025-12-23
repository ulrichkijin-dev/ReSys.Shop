// src/ReSys.Shop.Admin/src/models/accounts/auth/internals/internal.model.js

/**
 * @typedef {object} LoginParam
 * @property {string} credential
 * @property {string} password
 * @property {boolean} rememberMe
 */

/**
 * @typedef {object} RegisterParam
 * @property {string | null} [userName]
 * @property {string} email
 * @property {string | null} [firstName]
 * @property {string | null} [lastName]
 * @property {string | null} [phoneNumber]
 * @property {string} confirmPassword
 * @property {string} password
 * @property {string | null} [dateOfBirth] - DateTimeOffset (ISO 8601)
 */
