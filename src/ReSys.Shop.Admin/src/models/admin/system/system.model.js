/**
 * @enum {number}
 */
export const AuditSeverity = {
  Information: 0,
  Warning: 1,
  Error: 2,
  Critical: 3,
};

/**
 * @typedef {object} AuditLogItem
 * @property {string} id
 * @property {string | null} entityName
 * @property {string | null} entityId
 * @property {string | null} action
 * @property {string} timestamp
 * @property {string | null} userId
 * @property {string | null} userName
 * @property {string | null} userEmail
 * @property {string | null} ipAddress
 * @property {string | null} userAgent
 * @property {string | null} oldValues
 * @property {string | null} newValues
 * @property {string | null} changedProperties
 * @property {string | null} reason
 * @property {AuditSeverity} severity
 */
