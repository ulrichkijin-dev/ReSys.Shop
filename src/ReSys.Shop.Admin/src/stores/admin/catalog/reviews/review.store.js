// src/ReSys.Shop.Admin/src/stores/admin/catalog/reviews/review.store.js

import { defineStore } from 'pinia';
import { reviewService } from '@/services';

/**
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').ReviewItem} ReviewItem
 * @typedef {import('@/.js').ApproveReviewParameter} ApproveReviewParameter
 * @typedef {import('@/.js').RejectReviewParameter} RejectReviewParameter
 */

export const useReviewStore = defineStore('admin-review', {
  state: () => ({
    /** @type {ReviewItem[]} */
    reviews: [],
    /** @type {ReviewItem | null} */
    selectedReview: null,
    /** @type {boolean} */
    loading: false,
    /** @type {boolean} */
    error: false,
    /** @type {string | null} */
    errorMessage: null,
    /** @type {PaginationList<ReviewItem> | null} */
    pagedReviews: null,
  }),
  actions: {
    /**
     * Fetches a paginated list of reviews.
     * @param {QueryableParams} [params={}]
     */
    async fetchPagedReviews(params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await reviewService.getPagedList(params);
        if (response.succeeded) {
          this.pagedReviews = response.data;
          this.reviews = response.data?.items || [];
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch reviews.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching paged reviews:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches details for a single review by ID.
     * @param {string} id
     */
    async fetchReviewById(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await reviewService.getById(id);
        if (response.succeeded) {
          this.selectedReview = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch review details.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching review by ID:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Approves a review.
     * @param {string} id
     * @param {ApproveReviewParameter} [payload]
     * @returns {Promise<boolean>}
     */
    async approveReview(id, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await reviewService.approve(id, payload);
        if (response.succeeded) {
          this.fetchReviewById(id); // Refresh details
          this.fetchPagedReviews(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to approve review.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error approving review:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Rejects a review.
     * @param {string} id
     * @param {RejectReviewParameter} payload
     * @returns {Promise<boolean>}
     */
    async rejectReview(id, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await reviewService.reject(id, payload);
        if (response.succeeded) {
          this.fetchReviewById(id); // Refresh details
          this.fetchPagedReviews(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to reject review.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error rejecting review:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Deletes a review.
     * @param {string} id
     * @returns {Promise<boolean>}
     */
    async deleteReview(id) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await reviewService.delete(id);
        if (response.succeeded) {
          this.fetchPagedReviews(); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to delete review.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error deleting review:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },
  },
});
