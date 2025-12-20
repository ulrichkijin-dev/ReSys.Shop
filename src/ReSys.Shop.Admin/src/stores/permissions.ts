import { defineStore } from "pinia"
import { ref, computed } from "vue"
import { getAxiosInstance } from "@/api/axios"
import type { Permission } from "@/types"
import { useAuthStore } from "./auth"

export const usePermissionsStore = defineStore("permissions", () => {
  const permissions = ref<Map<string, Permission>>(new Map())
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  const allPermissions = computed(() => Array.from(permissions.value.values()))

  async function fetchAllPermissions() {
    isLoading.value = true
    error.value = null

    try {
      const axios = getAxiosInstance()
      const response = await axios.get("/permissions")
      permissions.value.clear()
      response.data.data.forEach((perm: Permission) => {
        permissions.value.set(perm.code, perm)
      })
    } catch (err: any) {
      error.value = err.message
      throw err
    } finally {
      isLoading.value = false
    }
  }

  function hasPermission(permissionCode: string): boolean {
    const authStore = useAuthStore()
    return authStore.userPermissions.includes(permissionCode)
  }

  function hasAnyPermission(permissionCodes: string[]): boolean {
    const authStore = useAuthStore()
    return permissionCodes.some((code) => authStore.userPermissions.includes(code))
  }

  function hasAllPermissions(permissionCodes: string[]): boolean {
    const authStore = useAuthStore()
    return permissionCodes.every((code) => authStore.userPermissions.includes(code))
  }

  function canAccess(action: string, resource: string): boolean {
    const permissionCode = `${resource}.${action}`
    return hasPermission(permissionCode)
  }

  return {
    permissions: allPermissions,
    isLoading,
    error,
    fetchAllPermissions,
    hasPermission,
    hasAnyPermission,
    hasAllPermissions,
    canAccess,
  }
})
