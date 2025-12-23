// src/ReSys.Shop.Admin/src/models/accounts/auth/sessions/session.model.js

/**
 * @typedef {object} LogOutParam
 * @property {string | null} [refreshToken]
 */

/**
 * @typedef {object} SessionResult
 * @property {string} userId - Guid
 * @property {string} userName
 * @property {string} email
 * @property {string | null} [phoneNumber]
 * @property {boolean} isEmailConfirmed
 * @property {boolean} isPhoneNumberConfirmed
 * @property {string[]} roles
 * @property {string[]} permissions
 */

/**
 * @typedef {object} RefreshTokenParam
 * @property {string} refreshToken
 * @property {boolean} rememberMe
 */
