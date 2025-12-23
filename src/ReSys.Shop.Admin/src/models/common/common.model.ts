// src/ReSys.Shop.Admin/src/models/common/common.model.ts

export interface QueryableParams {
  pageIndex?: number;
  pageSize?: number;
  search?: string;
  orderBy?: string;
  filter?: string;
  // Add other common query parameters if found consistently in C# QueryableParams
}

export interface PaginationList<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

// ApiResponse structure (based on C# ReSys.Shop.Core.Common.Models.Wrappers.Responses.ApiResponse)
export interface ApiResponse<T = any> {
  succeeded: boolean;
  message?: string | null;
  errors?: { code: string; description: string }[] | null;
  data?: T;
  meta?: any; // For pagination metadata or other generic info
}
