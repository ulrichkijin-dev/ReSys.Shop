// src/ReSys.Shop.Admin/src/service/admin/dashboard/dashboard.service.js

import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/.js').DashboardSummary} DashboardSummary
 * @typedef {import('@/.js').RecentActivity} RecentActivity
 * @typedef {import('@/.js').InventoryAlert} InventoryAlert
 * @typedef {import('@/.js').SalesAnalysis} SalesAnalysis
 * @typedef {import('@/.js').OrderTrend} OrderTrend
 */

const API_BASE_ROUTE = 'api/admin/dashboard';
const httpClient = configureHttpClient();

export const dashboardService = {
  /**
   * Retrieves dashboard summary data.
   * @returns {Promise<ApiResponse<DashboardSummary>>}
   */
  async getSummary() {
    const response = await httpClient.get(`${API_BASE_ROUTE}/summary`);
    return response.data;
  },

  /**
   * Retrieves recent activity data.
   * @returns {Promise<ApiResponse<RecentActivity>>}
   */
  async getRecentActivity() {
    const response = await httpClient.get(`${API_BASE_ROUTE}/recent-activity`);
    return response.data;
  },

  /**
   * Retrieves inventory alerts.
   * @returns {Promise<ApiResponse<InventoryAlert[]>>}
   */
  async getInventoryAlerts() {
    const response = await httpClient.get(`${API_BASE_ROUTE}/inventory-alerts`);
    return response.data;
  },

  /**
   * Retrieves sales analysis data.
   * @param {number} [days=30] - Number of days for the analysis.
   * @returns {Promise<ApiResponse<SalesAnalysis>>}
   */
  async getSalesAnalysis(days = 30) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/sales-analysis`, { params: { days } });
    return response.data;
  },

  /**
   * Retrieves order trends data.
   * @param {number} [days=30] - Number of days for the analysis.
   * @returns {Promise<ApiResponse<OrderTrend>>}
   */
  async getOrderTrends(days = 30) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/order-trends`, { params: { days } });
    return response.data;
  },
};
