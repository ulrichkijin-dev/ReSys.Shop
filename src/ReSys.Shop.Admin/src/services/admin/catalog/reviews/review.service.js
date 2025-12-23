// src/ReSys.Shop.Admin/src/service/admin/catalog/reviews/review.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').ReviewItem} ReviewItem
 * @typedef {import('@/.js').ApproveReviewParameter} ApproveReviewParameter
 * @typedef {import('@/.js').RejectReviewParameter} RejectReviewParameter
 */

const API_BASE_ROUTE = 'api/admin/catalog/reviews';
const httpClient = configureHttpClient();

export const reviewService = {
  /**
   * Retrieves a paginated list of product reviews.
   * @param {QueryableParams} [params={}] - Query parameters for pagination and filtering.
   * @returns {Promise<ApiResponse<PaginationList<ReviewItem>>>}
   */
  async getPagedList(params = {}) {
    const response = await httpClient.get(API_BASE_ROUTE, { params });
    return response.data;
  },

  /**
   * Retrieves detailed information about a specific review.
   * @param {string} id - The ID of the review.
   * @returns {Promise<ApiResponse<ReviewItem>>}
   */
  async getById(id) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },

  /**
   * Approves a review, making it visible on the storefront.
   * @param {string} id - The ID of the review to approve.
   * @param {ApproveReviewParameter} [payload] - Optional notes from the moderator.
   * @returns {Promise<ApiResponse<ReviewItem>>}
   */
  async approve(id, payload = {}) {
    const response = await httpClient.patch(`${API_BASE_ROUTE}/${id}/approve`, payload);
    return response.data;
  },

  /**
   * Rejects a review with a reason.
   * @param {string} id - The ID of the review to reject.
   * @param {RejectReviewParameter} payload - The reason for rejecting the review.
   * @returns {Promise<ApiResponse<ReviewItem>>}
   */
  async reject(id, payload) {
    const response = await httpClient.patch(`${API_BASE_ROUTE}/${id}/reject`, payload);
    return response.data;
  },

  /**
   * Permanently deletes a product review.
   * @param {string} id - The ID of the review to delete.
   * @returns {Promise<ApiResponse<void>>}
   */
  async delete(id) {
    const response = await httpClient.delete(`${API_BASE_ROUTE}/${id}`);
    return response.data;
  },
};
