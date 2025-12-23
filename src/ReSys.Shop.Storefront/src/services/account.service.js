import httpClient from '@/api/httpClient';

const PROFILE_ROUTE = 'api/storefront/account'; // Updated to storefront specific route
const ADDRESS_ROUTE = 'api/account/addresses';
const ORDERS_ROUTE = 'api/storefront/account/orders';

export const accountService = {
  // Profile (Storefront AccountModule)
  async getProfile() {
    const response = await httpClient.get(PROFILE_ROUTE);
    return response.data;
  },
  async updateProfile(payload) {
    // Backend uses group.MapPatch(pattern: string.Empty, ...)
    const response = await httpClient.patch(PROFILE_ROUTE, payload);
    return response.data;
  },
  async deleteAccount() {
    const response = await httpClient.delete(PROFILE_ROUTE);
    return response.data;
  },

  // Addresses (Account AddressModule)
  async getAddresses(params = {}) {
    const response = await httpClient.get(ADDRESS_ROUTE, { params });
    return response.data;
  },
  async getAddressById(id) {
    const response = await httpClient.get(`${ADDRESS_ROUTE}/${id}`);
    return response.data;
  },
  async createAddress(payload) {
    const response = await httpClient.post(ADDRESS_ROUTE, payload);
    return response.data;
  },
  async updateAddress(id, payload) {
    const response = await httpClient.put(`${ADDRESS_ROUTE}/${id}`, payload);
    return response.data;
  },
  async deleteAddress(id) {
    const response = await httpClient.delete(`${ADDRESS_ROUTE}/${id}`);
    return response.data;
  },

  // Orders (Storefront OrderModule)
  async getOrders(params = {}) {
    const response = await httpClient.get(ORDERS_ROUTE, { params });
    return response.data;
  },
  async getOrderByNumber(number) {
    const response = await httpClient.get(`${ORDERS_ROUTE}/${number}`);
    return response.data;
  },
  async getOrderByToken(token) {
    // Guest order access
    const response = await httpClient.get(`api/storefront/orders/${token}`);
    return response.data;
  },
  async getOrderStatus(number) {
    const response = await httpClient.get(`${ORDERS_ROUTE}/${number}/status`);
    return response.data;
  }
};