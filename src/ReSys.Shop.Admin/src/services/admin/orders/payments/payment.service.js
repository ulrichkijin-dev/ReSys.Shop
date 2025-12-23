// src/ReSys.Shop.Admin/src/service/admin/orders/payments/payment.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').PaymentCreateParameter} PaymentCreateParameter
 * @typedef {import('@/.js').PaymentAuthorizeParameter} PaymentAuthorizeParameter
 * @typedef {import('@/.js').PaymentCaptureParameter} PaymentCaptureParameter
 * @typedef {import('@/.js').PaymentRefundParameter} PaymentRefundParameter
 * @typedef {import('@/.js').PaymentListItem} PaymentListItem
 */

const API_BASE_ROUTE = 'api/admin/orders';
const httpClient = configureHttpClient();

export const paymentService = {
  /**
   * Retrieves a list of payments for a specific order.
   * @param {string} orderId - The ID of the order.
   * @param {QueryableParams} [params={}] - Query parameters for filtering.
   * @returns {Promise<ApiResponse<PaginationList<PaymentListItem>>>}
   */
  async getList(orderId, params = {}) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${orderId}/payments`, { params });
    return response.data;
  },

  /**
   * Adds a new payment record to the order.
   * @param {string} orderId - The ID of the order.
   * @param {PaymentCreateParameter} payload
   * @returns {Promise<ApiResponse<PaymentListItem>>}
   */
  async create(orderId, payload) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/${orderId}/payments`, payload);
    return response.data;
  },

  /**
   * Marks a payment as authorized with a transaction ID.
   * @param {string} orderId - The ID of the order.
   * @param {string} paymentId - The ID of the payment.
   * @param {PaymentAuthorizeParameter} payload
   * @returns {Promise<ApiResponse<void>>}
   */
  async authorize(orderId, paymentId, payload) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/${orderId}/payments/${paymentId}/authorize`, payload);
    return response.data;
  },

  /**
   * Marks a payment as captured/completed.
   * @param {string} orderId - The ID of the order.
   * @param {string} paymentId - The ID of the payment.
   * @param {PaymentCaptureParameter} [payload] - Optional transaction ID.
   * @returns {Promise<ApiResponse<void>>}
   */
  async capture(orderId, paymentId, payload = {}) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/${orderId}/payments/${paymentId}/capture`, payload);
    return response.data;
  },

  /**
   * Records a refund for a captured payment.
   * @param {string} orderId - The ID of the order.
   * @param {string} paymentId - The ID of the payment.
   * @param {PaymentRefundParameter} payload
   * @returns {Promise<ApiResponse<void>>}
   */
  async refund(orderId, paymentId, payload) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/${orderId}/payments/${paymentId}/refund`, payload);
    return response.data;
  },

  /**
   * Voids an authorized but not yet captured payment.
   * @param {string} orderId - The ID of the order.
   * @param {string} paymentId - The ID of the payment.
   * @returns {Promise<ApiResponse<void>>}
   */
  async voidPayment(orderId, paymentId) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/${orderId}/payments/${paymentId}/void`);
    return response.data;
  },
};
