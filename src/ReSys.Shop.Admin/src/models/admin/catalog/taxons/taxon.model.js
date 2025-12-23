// src/ReSys.Shop.Admin/src/models/admin/catalog/taxons/taxon.model.js

/**
 * @typedef {import('../../common/common.model').HasPosition} HasPosition
 * @typedef {import('../../common/common.model').HasMetadata} HasMetadata
 */

/**
 * @enum {string} RulesMatchPolicy
 * @property {string} Any - Match any rule.
 * @property {string} All - Match all rules.
 */
export const RulesMatchPolicy = {
  Any: 'any',
  All: 'all',
};

/**
 * @enum {string} TaxonSortOrder
 * @property {string} Position - Sort by position.
 * @property {string} NameAsc - Sort by name ascending.
 * @property {string} NameDesc - Sort by name descending.
 * // Add other sort orders as discovered from backend
 */
export const TaxonSortOrder = {
  Position: 'position',
  NameAsc: 'name_asc',
  NameDesc: 'name_desc',
};

/**
 * @typedef {object} TaxonParameter
 * @property {string} taxonomyId - The ID of the parent taxonomy.
 * @property {string} [parentId] - The ID of the parent taxon (null for root taxons).
 * @property {string} name - The internal name of the taxon (unique within its taxonomy).
 * @property {string} presentation - The display name of the taxon.
 * @property {string} [description] - A description of the taxon.
 * @property {boolean} hideFromNav - Whether to hide this taxon from navigation.
 * @property {number} position - The display order of the taxon.
 * @property {boolean} automatic - Whether products are automatically assigned to this taxon based on rules.
 * @property {RulesMatchPolicy} [rulesMatchPolicy] - How rules should be matched ("any" or "all"). Required if `automatic` is true.
 * @property {TaxonSortOrder} [sortOrder] - The default sort order for products within this taxon. Required if `automatic` is true.
 * @property {string} [metaTitle] - SEO meta title.
 * @property {string} [metaDescription] - SEO meta description.
 * @property {string} [metaKeywords] - SEO meta keywords.
 * @property {Object.<string, any>} [publicMetadata] - Public metadata.
 * @property {Object.<string, any>} [privateMetadata] - Private metadata.
 */

/**
 * @typedef {object} TaxonSelectItem
 * @property {string} id - The unique identifier of the taxon.
 * @property {string} name - The internal name of the taxon.
 * @property {string} presentation - The display name of the taxon.
 */

/**
 * @typedef {object} TaxonListItem
 * @property {string} id - The unique identifier of the taxon.
 * @property {string} name - The internal name of the taxon.
 * @property {string} presentation - The display name of the taxon.
 * @property {string} taxonomyName - The name of the parent taxonomy.
 * @property {string} [parentName] - The name of the parent taxon.
 * @property {string} [description] - A description of the taxon.
 * @property {string} permalink - The URL-friendly path of the taxon.
 * @property {string} prettyName - Human-readable display name with parent chain.
 * @property {boolean} hideFromNav - Whether to hide this taxon from navigation.
 * @property {number} position - The display order of the taxon.
 * @property {boolean} automatic - Whether products are automatically assigned.
 * @property {string} sortOrder - The default sort order for products within this taxon.
 * @property {string} [metaTitle] - SEO meta title.
 * @property {string} [metaDescription] - SEO meta description.
 * @property {string} [metaKeywords] - SEO meta keywords.
 * @property {number} childrenCount - Number of immediate children taxons.
 * @property {string} createdAt - Date and time when the taxon was created (ISO 8601).
 * @property {string} [updatedAt] - Date and time when the taxon was last updated (ISO 8601).
 */

/**
 * @typedef {TaxonParameter & object} TaxonDetail
 * @property {string} id - The unique identifier of the taxon.
 */

/**
 * @typedef {object} TaxonRuleItem
 * @property {string} id - The unique identifier of the rule.
 * @property {string} type - The type of rule (e.g., "product_attribute", "variant_price").
 * @property {string} value - The value to match against.
 * @property {RulesMatchPolicy} matchPolicy - How the value should be matched (e.g., "is_equal_to", "contains").
 * @property {string} [propertyName] - The name of the property if the rule applies to a product property.
 * @property {string} createdAt - Date and time when the rule was created (ISO 8601).
 * @property {string} [updatedAt] - Date and time when the rule was last updated (ISO 8601).
 */

/**
 * @typedef {object} TaxonRuleParameter
 * @property {string} type - The type of rule.
 * @property {string} value - The value to match against.
 * @property {RulesMatchPolicy} matchPolicy - How the value should be matched.
 * @property {string} [propertyName] - The name of the property.
 */

/**
 * @typedef {object} TaxonImageItem
 * @property {string} [id] - The unique identifier of the image.
 * @property {string} type - The type of image (e.g., "default", "square").
 * @property {string} [alt] - Alternative text for the image.
 * @property {string} [url] - URL of the image.
 * @property {number} position - The display order of the image.
 */

/**
 * @typedef {object} HierarchyParameter
 * @property {string[]} [taxonomyId] - Array of taxonomy IDs to filter by.
 * @property {string} [focusedTaxonId] - The ID of a specific taxon to focus the hierarchy on.
 * @property {boolean} [includeLeavesOnly] - Whether to only include leaf nodes.
 * @property {boolean} [includeHidden] - Whether to include hidden taxons.
 * @property {number} [maxDepth] - Maximum depth of the tree to return.
 */

/**
 * @typedef {object} TreeListItem
 * @property {TreeNodeItem[]} tree - The complete tree structure.
 * @property {TreeNodeItem[]} breadcrumbs - Breadcrumb trail to the focused taxon.
 * @property {TreeNodeItem} [focusedNode] - The focused taxon node.
 * @property {TreeNodeItem} [focusedSubtree] - Subtree rooted at the focused taxon.
 * @property {number} totalCount - Total number of nodes in the tree.
 * @property {number} maxDepth - Maximum depth of the tree.
 */

/**
 * @typedef {object} TreeNodeItem
 * @property {string} id - Unique identifier for the taxon.
 * @property {string} name - Internal normalized name.
 * @property {string} presentation - Human-readable display name.
 * @property {string} [prettyName] - Display name with parent chain.
 * @property {string} [permalink] - URL-friendly path.
 * @property {string} [parentId] - ID of the parent taxon.
 * @property {TreeNodeItem[]} children - Child nodes.
 * @property {number} sortOrder - Position/order within siblings.
 * @property {number} depth - Depth in the tree hierarchy.
 * @property {number} lft - Left value in nested set model.
 * @property {number} rgt - Right value in nested set model.
 * @property {number} productCount - Number of products directly associated.
 * @property {number} childCount - Number of immediate children.
 * @property {string} [imageUrl] - URL of the default image.
 * @property {string} [squareImageUrl] - URL of the square/thumbnail image.
 * @property {boolean} hasChildren - True if has any children.
 * @property {boolean} isLeaf - True if no children.
 * @property {boolean} isRoot - True if no parent.
 * @property {boolean} isExpanded - UI flag: whether this node should be expanded.
 * @property {boolean} isInActivePath - UI flag: whether this node is part of the active path.
 */

/**
 * @typedef {object} FlatListItem
 * @property {string} id - Unique identifier for the taxon.
 * @property {string} name - Internal normalized name.
 * @property {string} presentation - Human-readable display name.
 * @property {string} [prettyName] - Display name with parent chain.
 * @property {number} depth - Depth in the tree hierarchy.
 * @property {number} productCount - Number of products directly associated.
 * @property {boolean} hasChildren - True if has any children.
 * @property {boolean} isExpanded - UI flag: whether this node should be expanded.
 * @property {string} [parentId] - ID of the parent taxon.
 * @property {string} indent - Indentation string for display.
 * @property {string} displayName - Display name with indentation prefix.
 */
