// src/ReSys.Shop.Admin/src/models/admin/catalog/taxons/hierarchy/taxon-hierarchy.model.js

/**
 * @typedef {import('@/models/common/common.model').QueryableParams & {
 *   taxonomyId?: string[] | null, // Guid[]
 *   focusedTaxonId?: string | null, // Guid
 *   includeLeavesOnly?: boolean | null,
 *   includeHidden?: boolean,
 *   maxDepth?: number | null,
 * }} TaxonHierarchyRequest
 */

/**
 * @typedef {object} TreeNodeItem
 * @property {string} id - Guid
 * @property {string} name
 * @property {string} presentation
 * @property {string | null} [prettyName]
 * @property {string | null} [permalink]
 * @property {string | null} [parentId] - Guid
 * @property {TreeNodeItem[]} children
 * @property {number} sortOrder
 * @property {number} depth
 * @property {number} lft
 * @property {number} rgt
 * @property {number} productCount
 * @property {number} childCount
 * @property {string | null} [imageUrl]
 * @property {string | null} [squareImageUrl]
 * @property {boolean} hasChildren
 * @property {boolean} isLeaf
 * @property {boolean} isRoot
 * @property {boolean} isChild
 * @property {boolean} isExpanded
 * @property {boolean} isInActivePath
 */

/**
 * @typedef {object} TreeListItem
 * @property {TreeNodeItem[]} tree
 * @property {TreeNodeItem[]} breadcrumbs
 * @property {TreeNodeItem | null} [focusedNode]
 * @property {TreeNodeItem | null} [focusedSubtree]
 * @property {number} totalCount
 * @property {number} maxDepth
 */

/**
 * @typedef {object} FlatListItem
 * @property {string} id - Guid
 * @property {string} name
 * @property {string} presentation
 * @property {string | null} [prettyName]
 * @property {number} depth
 * @property {number} productCount
 * @property {boolean} hasChildren
 * @property {boolean} isExpanded
 * @property {string | null} [parentId] - Guid
 * @property {string} indent
 * @property {string} displayName
 */

/**
 * @typedef {object} TaxonRebuildHierarchyRequest
 * @property {string} taxonomyId - Guid
 */
