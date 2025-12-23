// src/ReSys.Shop.Admin/src/stores/admin/dashboard/dashboard.store.js

import { defineStore } from 'pinia';
import { dashboardService } from '@/services';

/**
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/.js').DashboardSummary} DashboardSummary
 * @typedef {import('@/.js').RecentActivity} RecentActivity
 * @typedef {import('@/.js').InventoryAlert} InventoryAlert
 * @typedef {import('@/.js').SalesAnalysis} SalesAnalysis
 * @typedef {import('@/.js').OrderTrend} OrderTrend
 */

export const useDashboardStore = defineStore('admin-dashboard', {
  state: () => ({
    /** @type {DashboardSummary | null} */
    summary: null,
    /** @type {RecentActivity | null} */
    recentActivity: null,
    /** @type {InventoryAlert[] | null} */
    inventoryAlerts: null,
    /** @type {SalesAnalysis | null} */
    salesAnalysis: null,
    /** @type {OrderTrend | null} */
    orderTrends: null,
    /** @type {boolean} */
    loading: false,
    /** @type {boolean} */
    error: false,
    /** @type {string | null} */
    errorMessage: null,
  }),
  actions: {
    /**
     * Fetches dashboard summary data.
     */
    async fetchSummary() {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await dashboardService.getSummary();
        if (response.succeeded) {
          this.summary = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch dashboard summary.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching dashboard summary:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches recent activity data.
     */
    async fetchRecentActivity() {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await dashboardService.getRecentActivity();
        if (response.succeeded) {
          this.recentActivity = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch recent activity.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching recent activity:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches inventory alerts.
     */
    async fetchInventoryAlerts() {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await dashboardService.getInventoryAlerts();
        if (response.succeeded) {
          this.inventoryAlerts = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch inventory alerts.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching inventory alerts:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches sales analysis data.
     * @param {number} [days=30]
     */
    async fetchSalesAnalysis(days = 30) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await dashboardService.getSalesAnalysis(days);
        if (response.succeeded) {
          this.salesAnalysis = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch sales analysis.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching sales analysis:', err);
      } finally {
        this.loading = false;
      }
    },

    /**
     * Fetches order trends data.
     * @param {number} [days=30]
     */
    async fetchOrderTrends(days = 30) {
      this.loading = true;
      this.error = false;
      this.errorMessage = null;
      try {
        const response = await dashboardService.getOrderTrends(days);
        if (response.succeeded) {
          this.orderTrends = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch order trends.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
        console.error('Error fetching order trends:', err);
      } finally {
        this.loading = false;
      }
    },
  },
});
