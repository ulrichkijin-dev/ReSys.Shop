import { configureHttpClient } from '@/utils/http-client';

/**
 * @typedef {import('@/models/common/common.model').ApiResponse} ApiResponse
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/models/admin/system/system.model').AuditLogItem} AuditLogItem
 */

const API_BASE_ROUTE = 'api/admin/system';
const httpClient = configureHttpClient();

export const systemService = {
  /**
   * Retrieves a paginated list of audit logs.
   * @param {QueryableParams} params
   * @returns {Promise<ApiResponse<PaginationList<AuditLogItem>>>}
   */
  async getAuditLogs(params) {
    const response = await httpClient.get(`${API_BASE_ROUTE}/audit-logs`, { params });
    return response.data;
  },
};
