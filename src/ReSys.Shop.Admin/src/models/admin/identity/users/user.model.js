// src/ReSys.Shop.Admin/src/models/admin/identity/users/user.model.js

/**
 * @typedef {object} UserParameter
 * @property {string} email - The user's email address.
 * @property {string} userName - The user's unique username.
 * @property {string} [password] - The user's password (required for create, optional for update).
 * @property {string} [firstName] - The user's first name.
 * @property {string} [lastName] - The user's last name.
 * @property {string} [dateOfBirth] - The user's date of birth (ISO 8601).
 * @property {string} [phoneNumber] - The user's phone number.
 * @property {string} [profileImagePath] - URL to the user's profile image.
 * @property {boolean} emailConfirmed - Whether the user's email has been confirmed.
 * @property {boolean} phoneNumberConfirmed - Whether the user's phone number has been confirmed.
 */

/**
 * @typedef {object} UserSelectItem
 * @property {string} id - The unique identifier of the user.
 * @property {string} userName - The user's unique username.
 * @property {string} [fullName] - The full name of the user.
 */

/**
 * @typedef {object} UserListItem
 * @property {string} id - The unique identifier of the user.
 * @property {string} userName - The user's unique username.
 * @property {string} [fullName] - The full name of the user.
 * @property {string} email - The user's email address.
 * @property {string} [phoneNumber] - The user's phone number.
 * @property {boolean} emailConfirmed - Whether the user's email has been confirmed.
 * @property {boolean} phoneNumberConfirmed - Whether the user's phone number has been confirmed.
 * @property {string} createdAt - Date and time when the user was created (ISO 8601).
 * @property {string} [updatedAt] - Date and time when the user was last updated (ISO 8601).
 */

/**
 * @typedef {UserListItem & object} UserDetail
 * @property {string} [dateOfBirth] - The user's date of birth (ISO 8601).
 * @property {string} [profileImagePath] - URL to the user's profile image.
 * @property {string} [lastSignInAt] - Date and time of the user's last sign-in (ISO 8601).
 * @property {string} [lastSignInIp] - IP address of the user's last sign-in.
 * @property {string} [currentSignInAt] - Date and time of the user's current sign-in (ISO 8601).
 * @property {string} [currentSignInIp] - IP address of the user's current sign-in.
 * @property {number} signInCount - Number of times the user has signed in.
 * @property {boolean} lockoutEnabled - Whether the user's account can be locked out.
 * @property {string} [lockoutEnd] - Date and time when the user's lockout ends (ISO 8601).
 * @property {number} accessFailedCount - Number of failed access attempts.
 */

/**
 * @typedef {object} UserRoleItem
 * @property {string} id - The unique identifier of the role.
 * @property {string} name - The name of the role.
 */

/**
 * @typedef {object} UserPermissionItem
 * @property {string} name - The name of the permission.
 * @property {string} displayName - The display name of the permission.
 * @property {string} description - A description of the permission.
 */

/**
 * @typedef {object} AssignRoleToUserParameter
 * @property {string} roleName - The name of the role to assign.
 */

/**
 * @typedef {object} UnassignRoleFromUserParameter
 * @property {string} roleName - The name of the role to unassign.
 */

/**
 * @typedef {object} AssignPermissionToUserParameter
 * @property {string} claimType - The type of permission claim.
 * @property {string} claimValue - The value of the permission claim.
 */

/**
 * @typedef {object} UnassignPermissionFromUserParameter
 * @property {string} claimType - The type of permission claim.
 * @property {string} claimValue - The value of the permission claim.
 */
