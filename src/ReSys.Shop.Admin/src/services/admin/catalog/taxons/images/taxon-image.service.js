// src/ReSys.Shop.Admin/src/service/admin/catalog/taxons/images/taxon-image.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').TaxonImageResult} TaxonImageResult
 * @typedef {import('@/.js').TaxonImageListRequest} TaxonImageListRequest
 * @typedef {import('@/.js').TaxonImageBatchRequest} TaxonImageBatchRequest
 */

const API_BASE_ROUTE = 'api/admin/catalog/taxons';
const httpClient = configureHttpClient();

export const TaxonImageService = {
  /**
   * Fully synchronizes taxon images (add/update/delete) in one request.
   * Expects a FormData object containing 'data' which is an array of TaxonImageParameter.
   * Files should be appended to FormData directly.
   * @param {string} taxonId - The ID of the taxon.
   * @param {FormData} formData - FormData containing image data and files.
   * @returns {Promise<ApiResponse<TaxonImageResult[]>>}
   */
  async manageBatch(taxonId, formData) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/${taxonId}/images`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },

  /**
   * Retrieves the images for an existing taxon.
   * @param {string} taxonId - The ID of the taxon.
   * @param {TaxonImageListRequest} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<TaxonImageResult>>>}
   */
  async getList(taxonId, params = {}) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${taxonId}/images`, { params });
    return response.data;
  },
};
