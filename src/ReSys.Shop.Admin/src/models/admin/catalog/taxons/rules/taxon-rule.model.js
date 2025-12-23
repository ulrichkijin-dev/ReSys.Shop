// src/ReSys.Shop.Admin/src/models/admin/catalog/taxons/rules/taxon-rule.model.js

/**
 * @typedef {object} TaxonRuleParameter
 * @property {string} type
 * @property {string} value
 * @property {string} matchPolicy
 * @property {string | null} [propertyName]
 */

/**
 * @typedef {object} TaxonRuleItem
 * @property {string} id - Guid
 * @property {string} type
 * @property {string} value
 * @property {string} matchPolicy
 * @property {string | null} [propertyName]
 * @property {string} createdAt - DateTimeOffset
 * @property {string | null} [updatedAt] - DateTimeOffset
 */

/**
 * @typedef {import('@/models/common/common.model').QueryableParams & {}} TaxonRuleListRequest
 */

/**
 * @typedef {object} TaxonRuleManageRequest
 * @property {TaxonRuleParameter[]} rules
 */
