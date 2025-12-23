// src/ReSys.Shop.Admin/src/models/accounts/email/email.model.js

/**
 * @typedef {object} ChangeEmailParam
 * @property {string} currentEmail
 * @property {string} newEmail
 * @property {string} password
 */

/**
 * @typedef {object} ConfirmEmailParam
 * @property {string} userId
 * @property {string} code
 * @property {string | null} [changedEmail]
 */

/**
 * @typedef {object} ResendConfirmationParam
 * @property {string | null} [email]
 */
