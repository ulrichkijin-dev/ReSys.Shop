// src/ReSys.Shop.Admin/src/stores/admin/orders/shipments/shipment.store.js

import { defineStore } from 'pinia';
import { shipmentService } from '@/services';

/**
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

export const useShipmentStore = defineStore('admin-shipment', {
  state: () => ({
    /** @type {ShipmentListItem[]} */
    shipments: [],
    /** @type {ShipmentDetail | null} */
    selectedShipment: null,
    /** @type {boolean} */
    loading: false,
    /** @type {boolean} */
    error: false,
    /** @type {string | null} */
    errorMessage: null,
    /** @type {PaginationList<ShipmentListItem> | null} */
    pagedShipments: null, // Although annotations suggest List, using PaginationList for consistency
  }),
  actions: {
    /**
     * Fetches a list of shipments for a specific order.
     * @param {string} orderId
     * @param {QueryableParams} [params={}]
     */
    async fetchShipments(orderId, params = {}) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await shipmentService.getList(orderId, params);
        if (response.succeeded) {
          // Adjusting to fit PaginationList structure if the backend actually returns it
          // Otherwise, directly assign response.data to shipments.
          this.pagedShipments = {
            items: response.data?.items || response.data || [],
            totalCount: response.data?.totalCount || (response.data ? response.data.length : 0),
            pageNumber: params.pageNumber || 0,
            pageSize: params.pageSize || 0,
          };
          this.shipments = response.data?.items || response.data || [];
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch shipments.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching shipments:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches details for a specific shipment.
     * @param {string} orderId
     * @param {string} shipmentId
     */
    async fetchShipmentById(orderId, shipmentId) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await shipmentService.getById(orderId, shipmentId);
        if (response.succeeded) {
          this.selectedShipment = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch shipment details.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching shipment by ID:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Creates a new shipment for the order.
     * @param {string} orderId
     * @param {ShipmentParameter} payload
     * @returns {Promise<boolean>}
     */
    async createShipment(orderId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await shipmentService.create(orderId, payload);
        if (response.succeeded) {
          this.fetchShipments(orderId); // Refresh the list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to create shipment.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error creating shipment:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Updates shipment details.
     * @param {string} orderId
     * @param {string} shipmentId
     * @param {ShipmentUpdateParameter} payload
     * @returns {Promise<boolean>}
     */
    async updateShipment(orderId, shipmentId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await shipmentService.update(orderId, shipmentId, payload);
        if (response.succeeded) {
          this.fetchShipmentById(orderId, shipmentId); // Refresh details
          this.fetchShipments(orderId); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to update shipment.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error updating shipment:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Deletes a shipment.
     * @param {string} orderId
     * @param {string} shipmentId
     * @returns {Promise<boolean>}
     */
    async deleteShipment(orderId, shipmentId) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await shipmentService.delete(orderId, shipmentId);
        if (response.succeeded) {
          this.fetchShipments(orderId); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to delete shipment.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error deleting shipment:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Adds a product variant to an existing shipment.
     * @param {string} orderId
     * @param {string} shipmentId
     * @param {ShipmentAddItemParameter} payload
     * @returns {Promise<boolean>}
     */
    async addItemToShipment(orderId, shipmentId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await shipmentService.addItem(orderId, shipmentId, payload);
        if (response.succeeded) {
          this.fetchShipmentById(orderId, shipmentId); // Refresh details
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to add item to shipment.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error adding item to shipment:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Removes a product variant from a shipment.
     * @param {string} orderId
     * @param {string} shipmentId
     * @param {ShipmentRemoveItemParameter} payload
     * @returns {Promise<boolean>}
     */
    async removeItemFromShipment(orderId, shipmentId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await shipmentService.removeItem(orderId, shipmentId, payload);
        if (response.succeeded) {
          this.fetchShipmentById(orderId, shipmentId); // Refresh details
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to remove item from shipment.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error removing item from shipment:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Automatically determines and creates shipments using a fulfillment strategy.
     * @param {string} orderId
     * @param {ShipmentAutoPlanParameter} payload
     * @returns {Promise<boolean>}
     */
    async autoPlanShipments(orderId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await shipmentService.autoPlan(orderId, payload);
        if (response.succeeded) {
          this.fetchShipments(orderId); // Refresh the list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to auto-plan shipments.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error auto-planning shipments:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Transitions shipment state to Ready.
     * @param {string} orderId
     * @param {string} shipmentId
     * @returns {Promise<boolean>}
     */
    async readyShipment(orderId, shipmentId) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await shipmentService.ready(orderId, shipmentId);
        if (response.succeeded) {
          this.fetchShipmentById(orderId, shipmentId); // Refresh details
          this.fetchShipments(orderId); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to mark shipment as ready.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error marking shipment as ready:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Marks a shipment as shipped.
     * @param {string} orderId
     * @param {string} shipmentId
     * @returns {Promise<boolean>}
     */
    async shipShipment(orderId, shipmentId) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await shipmentService.ship(orderId, shipmentId);
        if (response.succeeded) {
          this.fetchShipmentById(orderId, shipmentId); // Refresh details
          this.fetchShipments(orderId); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to mark shipment as shipped.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error marking shipment as shipped:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Cancels a shipment.
     * @param {string} orderId
     * @param {string} shipmentId
     * @returns {Promise<boolean>}
     */
    async cancelShipment(orderId, shipmentId) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await shipmentService.cancelAction(orderId, shipmentId);
        if (response.succeeded) {
          this.fetchShipmentById(orderId, shipmentId); // Refresh details
          this.fetchShipments(orderId); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to cancel shipment.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error canceling shipment:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Marks a shipment as delivered.
     * @param {string} orderId
     * @param {string} shipmentId
     * @returns {Promise<boolean>}
     */
    async deliverShipment(orderId, shipmentId) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await shipmentService.deliver(orderId, shipmentId);
        if (response.succeeded) {
          this.fetchShipmentById(orderId, shipmentId); // Refresh details
          this.fetchShipments(orderId); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to mark shipment as delivered.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error marking shipment as delivered:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Resumes a canceled shipment.
     * @param {string} orderId
     * @param {string} shipmentId
     * @returns {Promise<boolean>}
     */
    async resumeShipment(orderId, shipmentId) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await shipmentService.resume(orderId, shipmentId);
        if (response.succeeded) {
          this.fetchShipmentById(orderId, shipmentId); // Refresh details
          this.fetchShipments(orderId); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to resume shipment.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error resuming shipment:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Moves shipment back to pending state.
     * @param {string} orderId
     * @param {string} shipmentId
     * @returns {Promise<boolean>}
     */
    async toPendingShipment(orderId, shipmentId) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await shipmentService.toPending(orderId, shipmentId);
        if (response.succeeded) {
          this.fetchShipmentById(orderId, shipmentId); // Refresh details
          this.fetchShipments(orderId); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to move shipment to pending.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error moving shipment to pending:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Transfers inventory units from one shipment to another.
     * @param {string} orderId
     * @param {string} shipmentId
     * @param {ShipmentTransferToShipmentParameter} payload
     * @returns {Promise<boolean>}
     */
    async transferShipmentToShipment(orderId, shipmentId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await shipmentService.transferToShipment(orderId, shipmentId, payload);
        if (response.succeeded) {
          this.fetchShipmentById(orderId, shipmentId); // Refresh details
          this.fetchShipments(orderId); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to transfer shipment units to another shipment.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error transferring shipment units to another shipment:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Transfers inventory units from one shipment to a new shipment at a different location.
     * @param {string} orderId
     * @param {string} shipmentId
     * @param {ShipmentTransferToLocationParameter} payload
     * @returns {Promise<boolean>}
     */
    async transferShipmentToLocation(orderId, shipmentId, payload) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await shipmentService.transferToLocation(orderId, shipmentId, payload);
        if (response.succeeded) {
          this.fetchShipmentById(orderId, shipmentId); // Refresh details
          this.fetchShipments(orderId); // Refresh list
          return true;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to transfer shipment units to a new location.';
          return false;
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error transferring shipment units to a new location:', err);
        return false;
      } finally {
        this.loading = false;
      }
    },
  },
});
