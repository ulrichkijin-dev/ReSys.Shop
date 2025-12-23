/**
 * @typedef {object} QueryableParams
 * @property {number} [pageIndex]
 * @property {number} [pageSize]
 * @property {string} [search]
 * @property {string} [orderBy]
 * @property {string} [filter]
 */

/**
 * @template T
 * @typedef {object} PaginationList
 * @property {T[]} items
 * @property {number} totalCount
 * @property {number} pageNumber
 * @property {number} pageSize
 * @property {number} totalPages
 * @property {boolean} hasPreviousPage
 * @property {boolean} hasNextPage
 */

/**
 * @typedef {object} ApiError
 * @property {string} code
 * @property {string} description
 */

/**
 * @template T
 * @typedef {object} ApiResponse
 * @property {boolean} succeeded
 * @property {string | null} [message]
 * @property {ApiError[] | null} [errors]
 * @property {T} [data]
 * @property {any} [meta]
 */
