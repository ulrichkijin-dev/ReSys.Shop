// src/ReSys.Shop.Admin/src/models/admin/identity/roles/role.model.js

/**
 * @typedef {object} RoleParameter
 * @property {string} name - The internal name of the role (unique).
 * @property {string} [displayName] - The display name of the role.
 * @property {string} [description] - A description of the role.
 * @property {number} priority - The priority of the role (higher means more important).
 * @property {boolean} isSystemRole - Indicates if this is a system-defined role.
 */

/**
 * @typedef {object} RoleSelectItem
 * @property {string} id - The unique identifier of the role.
 * @property {string} [name] - The internal name of the role.
 * @property {string} [description] - A description of the role.
 */

/**
 * @typedef {object} RoleUserItem
 * @property {string} id - The unique identifier of the user.
 * @property {string} userName - The username of the user.
 * @property {string} [fullName] - The full name of the user.
 */

/**
 * @typedef {object} RolePermissionItem
 * @property {string} name - The name of the permission.
 * @property {string} displayName - The display name of the permission.
 * @property {string} [description] - A description of the permission.
 */

/**
 * @typedef {object} RoleListItem
 * @property {string} id - The unique identifier of the role.
 * @property {string} name - The internal name of the role.
 * @property {string} [displayName] - The display name of the role.
 * @property {string} [description] - A description of the role.
 * @property {number} priority - The priority of the role.
 * @property {boolean} isSystemRole - Indicates if this is a system-defined role.
 * @property {boolean} isDefault - Indicates if this is a default role.
 * @property {string} createdAt - Date and time when the role was created (ISO 8601).
 * @property {string} [createdBy] - The user who created the role.
 * @property {number} userCount - Number of users assigned to this role.
 * @property {number} permissionCount - Number of permissions assigned to this role.
 */

/**
 * @typedef {RoleListItem & object} RoleDetail
 * @property {string} [updatedAt] - Date and time when the role was last updated (ISO 8601).
 * @property {string} [updatedBy] - The user who last updated the role.
 */

/**
 * @typedef {object} AssignUserToRoleParameter
 * @property {string} userId - The ID of the user to assign.
 */

/**
 * @typedef {object} UnassignUserFromRoleParameter
 * @property {string} userId - The ID of the user to unassign.
 */

/**
 * @typedef {object} AssignPermissionToRoleParameter
 * @property {string} claimValue - The permission value to assign.
 */

/**
 * @typedef {object} UnassignPermissionFromRoleParameter
 * @property {string} claimValue - The permission value to unassign.
 */
