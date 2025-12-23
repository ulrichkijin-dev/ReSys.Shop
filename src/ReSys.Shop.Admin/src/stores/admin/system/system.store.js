import { defineStore } from 'pinia';
import { systemService } from '@/services';

/**
 * @typedef {import('@/models/common/common.model').QueryableParams} QueryableParams
 * @typedef {import('@/models/common/common.model').PaginationList} PaginationList
 * @typedef {import('@/models/admin/system/system.model').AuditLogItem} AuditLogItem
 */

export const useSystemStore = defineStore('admin-system', {
  state: () => ({
    /** @type {PaginationList<AuditLogItem> | null} */
    auditLogs: null,
    /** @type {boolean} */
    loading: false,
    /** @type {boolean} */
    error: false,
    /** @type {string | null} */
    errorMessage: null,
  }),
  actions: {
    /**
     * Fetches audit logs with pagination and filters.
     * @param {QueryableParams} params
     */
    async fetchAuditLogs(params) {
      this.loading = true;
      this.error = false;
      try {
        const response = await systemService.getAuditLogs(params);
        if (response.succeeded) {
          this.auditLogs = response.data;
        } else {
          this.error = true;
          this.errorMessage = response.message || 'Failed to fetch audit logs.';
        }
      } catch (err) {
        this.error = true;
        this.errorMessage = err.message || 'An unexpected error occurred.';
      } finally {
        this.loading = false;
      }
    },
  },
});
