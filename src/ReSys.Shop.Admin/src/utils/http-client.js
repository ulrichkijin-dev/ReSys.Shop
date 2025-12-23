// src/ReSys.Shop.Admin/src/utils/http-client.js

import axios from 'axios'
import { useAuthStore } from '@/stores/accounts/auth/auth.store.js'

/**
 * @typedef {import('../services/notification/notification.interface').INotificationService} INotificationService
 */

// Use var to avoid Temporal Dead Zone errors during circular dependency evaluation
var httpClientInstance = null
var notificationService = null
var isRefreshing = false
var failedQueue = []

const processQueue = (error, token = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error)
    } else {
      prom.resolve(token)
    }
  })

  failedQueue = []
}

/**
 * Configures and returns the Axios HTTP client instance.
 * @param {INotificationService} [toastNotificationService] - The notification service to use for toasts.
 * @returns {import('axios').AxiosInstance}
 */
export function configureHttpClient(toastNotificationService) {
  const API_BASE_URL = import.meta.env?.VITE_API_BASE_URL || '/api'


  if (httpClientInstance) {
    if (toastNotificationService && !notificationService) {
      notificationService = toastNotificationService
    }
    return httpClientInstance
  }

  notificationService = toastNotificationService

  httpClientInstance = axios.create({
    baseURL: API_BASE_URL,
    headers: {
      'Content-Type': 'application/json',
    },
  })

  // Request interceptor
  httpClientInstance.interceptors.request.use(
    (config) => {
      const authStore = useAuthStore()
      const token = authStore.getAccessToken || localStorage.getItem('accessToken')

      if (token) {
        config.headers.Authorization = `Bearer ${token}`
      }
      return config
    },
    (error) => Promise.reject(error),
  )

  // Response interceptor
  httpClientInstance.interceptors.response.use(
    (response) => {
      const apiResponse = response.data
      // Standard success notification for mutating requests
      if (
        apiResponse.succeeded &&
        apiResponse.message &&
        !['get', 'head'].includes(response.config.method?.toLowerCase())
      ) {
        notificationService?.success(apiResponse.message)
      }
      return response
    },
    async (error) => {
      const originalRequest = error.config
      const authStore = useAuthStore()

      if (!error.response) {
        notificationService?.error(
          'Network error: Could not connect to the server.',
          'Connection Failed',
        )
        return Promise.reject(error)
      }

      const { status, data: apiResponse } = error.response

      // 401 Unauthorized - Token Refresh Logic
      if (status === 401 && !originalRequest._retry && !originalRequest.url.includes('/login')) {
        if (isRefreshing) {
          return new Promise((resolve, reject) => {
            failedQueue.push({ resolve, reject })
          })
            .then((token) => {
              originalRequest.headers.Authorization = `Bearer ${token}`
              return httpClientInstance(originalRequest)
            })
            .catch((err) => Promise.reject(err))
        }

        originalRequest._retry = true
        isRefreshing = true

        try {
          const success = await authStore.refreshToken()
          if (success) {
            const newToken = authStore.getAccessToken
            processQueue(null, newToken)
            originalRequest.headers.Authorization = `Bearer ${newToken}`
            return httpClientInstance(originalRequest)
          }
        } catch (refreshError) {
          processQueue(refreshError, null)
          authStore.logout()
          return Promise.reject(refreshError)
        } finally {
          isRefreshing = false
        }
      }

      // Centralized Error Reporting
      handleApiError(status, apiResponse, originalRequest)

      return Promise.reject(error)
    },
  )

  return httpClientInstance
}

/**
 * Handles standardized error reporting based on HTTP status and API response structure.
 */
function handleApiError(status, apiResponse, originalRequest) {
  if (status === 403) {
    notificationService?.error(
      'You do not have permission to perform this action.',
      'Access Denied',
    )
  } else if (status === 404) {
    if (!originalRequest.url.includes('/session')) {
      notificationService?.warn(apiResponse?.message || 'Resource not found.', 'Not Found')
    }
  } else if (status === 400 || status === 422) {
    if (apiResponse?.errors && apiResponse.errors.length > 0) {
      const errorMessages = apiResponse.errors
        .map((err) => err.description || err.message)
        .join('\n')
      notificationService?.error(errorMessages, 'Validation Error')
    } else {
      notificationService?.error(apiResponse?.message || 'Invalid request.', 'Error')
    }
  } else if (status >= 500) {
    notificationService?.error('A server-side error occurred.', 'Server Error')
  }
}
