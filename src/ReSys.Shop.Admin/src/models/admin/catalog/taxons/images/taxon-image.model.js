// src/ReSys.Shop.Admin/src/models/admin/catalog/taxons/images/taxon-image.model.js

/**
 * @typedef {object} TaxonImageParameter
 * @property {string | null} [id] - Guid
 * @property {string} type
 * @property {string | null} [alt]
 * @property {string | null} [url]
 * @property {number} position
 * @property {File | null} [file] - Represents IFormFile
 */

/**
 * @typedef {object} TaxonImageResult
 * @property {string} id - Guid
 * @property {string} type
 * @property {string} url
 * @property {string | null} [alt]
 * @property {number} position
 * @property {number} size
 * @property {string} contentType
 * @property {number | null} [width]
 * @property {number | null} [height]
 * @property {{ [key: number]: string } | null} [thumbnails]
 */

/**
 * @typedef {import('@/models/common/common.model').QueryableParams & {
 *   taxonId?: string, // Guid
 * }} TaxonImageListRequest
 */

/**
 * @typedef {object} TaxonImageBatchRequest
 * @property {TaxonImageParameter[]} data
 */
