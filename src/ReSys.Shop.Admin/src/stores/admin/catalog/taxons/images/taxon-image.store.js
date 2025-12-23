// src/ReSys.Shop.Admin/src/stores/admin/catalog/taxons/images/taxon-image.store.js

import { defineStore } from 'pinia';
import { TaxonImageService } from '@/services';

/**
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').TaxonImageResult} TaxonImageResult
 * @typedef {import('@/.js').TaxonImageListRequest} TaxonImageListRequest
 * @typedef {import('@/.js').TaxonImageBatchRequest} TaxonImageBatchRequest
 */

export const useTaxonImageStore = defineStore('admin-taxon-image', {
  state: () => ({
    /** @type {TaxonImageResult[]} */
    taxonImages: [],
    /** @type {boolean} */
    loading: false,
    /** @type {boolean} */
    error: false,
    /** @type {string | null} */
    errorMessage: null,
    /** @type {PaginationList<TaxonImageResult> | null} */
    pagedTaxonImages: null,
  }),
  actions: {
    /**
     * Fetches a paginated list of taxon images.
     * @param {string} taxonId
     * @param {TaxonImageListRequest} [params={}]
     */
    async fetchTaxonImages(taxonId, params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await TaxonImageService.getList(taxonId, params);
        if (response.succeeded) {
          this.pagedTaxonImages = response.data;
          this.taxonImages = response.data?.items || [];
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch taxon images.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching taxon images:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Manages a batch of taxon images (add/update/delete).
     * @param {string} taxonId
     * @param {FormData} formData
     * @returns {Promise<boolean>}
     */
    async manageTaxonImages(taxonId, formData) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await TaxonImageService.manageBatch(taxonId, formData);
        if (response.succeeded) {
          this.fetchTaxonImages(taxonId); // Refresh images for this taxon
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to manage taxon images.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error managing taxon images:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },
  },
});
