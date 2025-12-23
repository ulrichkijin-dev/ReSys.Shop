// src/ReSys.Shop.Admin/src/models/admin/identity/permissions/permission.model.js

/**
 * @typedef {object} PermissionSelectItem
 * @property {string} id - The unique identifier of the permission.
 * @property {string} name - The internal name of the permission.
 * @property {string} displayName - The display name of the permission.
 */

/**
 * @typedef {object} PermissionListItem
 * @property {string} id - The unique identifier of the permission.
 * @property {string} name - The internal name of the permission.
 * @property {string} area - The functional area the permission belongs to.
 * @property {string} resource - The resource the permission acts upon.
 * @property {string} action - The action the permission allows.
 * @property {string} [displayName] - The human-readable name of the permission.
 * @property {string} [description] - A description of the permission.
 * @property {string} [value] - The combined value of the permission (e.g., "Catalog.Product.View").
 * @property {string} [category] - The category of the permission.
 * @property {string} createdAt - Date and time when the permission was created (ISO 8601).
 * @property {string} [updatedAt] - Date and time when the permission was last updated (ISO 8601).
 */

/**
 * @typedef {PermissionListItem & object} PermissionDetail
 */
