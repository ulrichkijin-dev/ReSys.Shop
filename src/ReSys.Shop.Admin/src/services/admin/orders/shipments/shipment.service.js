// src/ReSys.Shop.Admin/src/service/admin/orders/shipments/shipment.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/.js').ShipmentParameter} ShipmentParameter
 * @typedef {import('@/.js').ShipmentUpdateParameter} ShipmentUpdateParameter
 * @typedef {import('@/.js').ShipmentFulfillmentItemRequest} ShipmentFulfillmentItemRequest
 * @typedef {import('@/.js').ShipmentAddItemParameter} ShipmentAddItemParameter
 * @typedef {import('@/.js').ShipmentRemoveItemParameter} ShipmentRemoveItemParameter
 * @typedef {import('@/.js').ShipmentAutoPlanParameter} ShipmentAutoPlanParameter
 * @typedef {import('@/.js').ShipmentTransferToShipmentParameter} ShipmentTransferToShipmentParameter
 * @typedef {import('@/.js').ShipmentTransferToLocationParameter} ShipmentTransferToLocationParameter
 * @typedef {import('@/.js').ShipmentListItem} ShipmentListItem
 * @typedef {import('@/.js').ShipmentDetail} ShipmentDetail
 */

const API_BASE_ROUTE = 'api/admin/orders';
const httpClient = configureHttpClient();

export const shipmentService = {
  /**
   * Retrieves a list of shipments for a specific order.
   * @param {string} orderId - The ID of the order.
   * @param {QueryableParams} [params={}] - Query parameters for filtering.
   * @returns {Promise<ApiResponse<PaginationList<ShipmentListItem>>>}
   */
  async getList(orderId, params = {}) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${orderId}/shipments`, { params });
    return response.data;
  },

  /**
   * Retrieves details of a specific shipment.
   * @param {string} orderId - The ID of the order.
   * @param {string} shipmentId - The ID of the shipment.
   * @returns {Promise<ApiResponse<ShipmentDetail>>}
   */
  async getById(orderId, shipmentId) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/${orderId}/shipments/${shipmentId}`);
    return response.data;
  },

  /**
   * Creates a new shipment for the order.
   * @param {string} orderId - The ID of the order.
   * @param {ShipmentParameter} payload
   * @returns {Promise<ApiResponse<ShipmentListItem>>}
   */
  async create(orderId, payload) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/${orderId}/shipments`, payload);
    return response.data;
  },

  /**
   * Updates shipment details like tracking number.
   * @param {string} orderId - The ID of the order.
   * @param {string} shipmentId - The ID of the shipment.
   * @param {ShipmentUpdateParameter} payload
   * @returns {Promise<ApiResponse<void>>}
   */
  async update(orderId, shipmentId, payload) {
    const response = await httpClient.put(`${API_BASE_ROUTE}/${orderId}/shipments/${shipmentId}`, payload);
    return response.data;
  },

  /**
   * Cancels and deletes a shipment.
   * @param {string} orderId - The ID of the order.
   * @param {string} shipmentId - The ID of the shipment.
   * @returns {Promise<ApiResponse<void>>}
   */
  async delete(orderId, shipmentId) {
    const response = await httpClient.delete(`${API_BASE_ROUTE}/${orderId}/shipments/${shipmentId}`);
    return response.data;
  },

  /**
   * Adds a product variant to an existing shipment.
   * @param {string} orderId - The ID of the order.
   * @param {string} shipmentId - The ID of the shipment.
   * @param {ShipmentAddItemParameter} payload
   * @returns {Promise<ApiResponse<void>>}
   */
  async addItem(orderId, shipmentId, payload) {
    const response = await httpClient.patch(`${API_BASE_ROUTE}/${orderId}/shipments/${shipmentId}/add_item`, payload);
    return response.data;
  },

  /**
   * Removes a product variant from a shipment.
   * @param {string} orderId - The ID of the order.
   * @param {string} shipmentId - The ID of the shipment.
   * @param {ShipmentRemoveItemParameter} payload
   * @returns {Promise<ApiResponse<void>>}
   */
  async removeItem(orderId, shipmentId, payload) {
    const response = await httpClient.patch(`${API_BASE_ROUTE}/${orderId}/shipments/${shipmentId}/remove_item`, payload);
    return response.data;
  },

  /**
   * Automatically determines and creates shipments using a fulfillment strategy.
   * @param {string} orderId - The ID of the order.
   * @param {ShipmentAutoPlanParameter} payload
   * @returns {Promise<ApiResponse<ShipmentListItem[]>>}
   */
  async autoPlan(orderId, payload) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/${orderId}/shipments/auto_plan`, payload);
    return response.data;
  },

  /**
   * Transitions shipment state to Ready.
   * @param {string} orderId - The ID of the order.
   * @param {string} shipmentId - The ID of the shipment.
   * @returns {Promise<ApiResponse<void>>}
   */
  async ready(orderId, shipmentId) {
    const response = await httpClient.patch(`${API_BASE_ROUTE}/${orderId}/shipments/${shipmentId}/ready`);
    return response.data;
  },

  /**
   * Marks a shipment as shipped and records tracking information.
   * @param {string} orderId - The ID of the order.
   * @param {string} shipmentId - The ID of the shipment.
   * @param {ShipmentUpdateParameter} payload
   * @returns {Promise<ApiResponse<void>>}
   */
  async ship(orderId, shipmentId, payload) {
    const response = await httpClient.patch(`${API_BASE_ROUTE}/${orderId}/shipments/${shipmentId}/ship`, payload);
    return response.data;
  },

  /**
   * Cancels a shipment.
   * @param {string} orderId - The ID of the order.
   * @param {string} shipmentId - The ID of the shipment.
   * @returns {Promise<ApiResponse<void>>}
   */
  async cancelAction(orderId, shipmentId) {
    const response = await httpClient.patch(`${API_BASE_ROUTE}/${orderId}/shipments/${shipmentId}/cancel`);
    return response.data;
  },

  /**
   * Marks a shipment as delivered to the customer.
   * @param {string} orderId - The ID of the order.
   * @param {string} shipmentId - The ID of the shipment.
   * @returns {Promise<ApiResponse<void>>}
   */
  async deliver(orderId, shipmentId) {
    const response = await httpClient.patch(`${API_BASE_ROUTE}/${orderId}/shipments/${shipmentId}/deliver`);
    return response.data;
  },

  /**
   * Resumes a canceled shipment.
   * @param {string} orderId - The ID of the order.
   * @param {string} shipmentId - The ID of the shipment.
   * @returns {Promise<ApiResponse<void>>}
   */
  async resume(orderId, shipmentId) {
    const response = await httpClient.patch(`${API_BASE_ROUTE}/${orderId}/shipments/${shipmentId}/resume`);
    return response.data;
  },

  /**
   * Moves shipment back to pending state.
   * @param {string} orderId - The ID of the order.
   * @param {string} shipmentId - The ID of the shipment.
   * @returns {Promise<ApiResponse<void>>}
   */
  async toPending(orderId, shipmentId) {
    const response = await httpClient.patch(`${API_BASE_ROUTE}/${orderId}/shipments/${shipmentId}/pending`);
    return response.data;
  },

  /**
   * Transfers inventory units from one shipment to another.
   * @param {string} orderId - The ID of the order.
   * @param {string} shipmentId - The ID of the source shipment.
   * @param {ShipmentTransferToShipmentParameter} payload
   * @returns {Promise<ApiResponse<void>>}
   */
  async transferToShipment(orderId, shipmentId, payload) {
    const response = await httpClient.patch(`${API_BASE_ROUTE}/${orderId}/shipments/${shipmentId}/transfer_to_shipment`, payload);
    return response.data;
  },

  /**
   * Transfers inventory units from one shipment to a new shipment at a different location.
   * @param {string} orderId - The ID of the order.
   * @param {string} shipmentId - The ID of the source shipment.
   * @param {ShipmentTransferToLocationParameter} payload
   * @returns {Promise<ApiResponse<void>>}
   */
  async transferToLocation(orderId, shipmentId, payload) {
    const response = await httpClient.patch(`${API_BASE_ROUTE}/${orderId}/shipments/${shipmentId}/transfer_to_location`, payload);
    return response.data;
  },
};
