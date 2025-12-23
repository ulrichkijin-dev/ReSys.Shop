import httpClient from '@/api/httpClient';

const API_BASE_ROUTE = 'api/storefront/cart';
const CHECKOUT_BASE_ROUTE = 'api/storefront/checkout';

export const cartService = {
  async get() {
    const response = await httpClient.get(API_BASE_ROUTE);
    return response.data;
  },

  async create(storeId, currency = 'USD') {
    const response = await httpClient.post(API_BASE_ROUTE, { storeId, currency });
    return response.data;
  },

  async delete() {
    const response = await httpClient.delete(API_BASE_ROUTE);
    return response.data;
  },

  async addItem(variantId, quantity) {
    const response = await httpClient.post(`${API_BASE_ROUTE}/add_item`, { variantId, quantity });
    return response.data;
  },

  async setQuantity(lineItemId, quantity) {
    const response = await httpClient.patch(`${API_BASE_ROUTE}/set_quantity`, { lineItemId, quantity });
    return response.data;
  },

  async removeItem(lineItemId) {
    const response = await httpClient.delete(`${API_BASE_ROUTE}/remove_line_item/${lineItemId}`);
    return response.data;
  },

  async empty() {
    const response = await httpClient.patch(`${API_BASE_ROUTE}/empty`);
    return response.data;
  },

  async applyCoupon(couponCode) {
    const response = await httpClient.patch(`${API_BASE_ROUTE}/apply_coupon`, { couponCode });
    return response.data;
  },

  async removeCoupon(couponCode) {
    const response = await httpClient.patch(`${API_BASE_ROUTE}/remove_coupon`, { couponCode });
    return response.data;
  },

  async associate() {
    const response = await httpClient.patch(`${API_BASE_ROUTE}/associate`);
    return response.data;
  },

  async setShippingAddress(address) {
    const response = await httpClient.patch(`${API_BASE_ROUTE}/set_shipping_address`, address);
    return response.data;
  },

  // Checkout
  async getCheckoutSummary() {
    const response = await httpClient.get(`${CHECKOUT_BASE_ROUTE}/summary`);
    return response.data;
  },

  async updateCheckout(payload) {
    // Currently used for updating Email
    const response = await httpClient.patch(CHECKOUT_BASE_ROUTE, payload);
    return response.data;
  },

  async updateCheckoutAddress(payload) {
    const response = await httpClient.patch(`${CHECKOUT_BASE_ROUTE}/address`, payload);
    return response.data;
  },

  async nextStep() {
    const response = await httpClient.patch(`${CHECKOUT_BASE_ROUTE}/next`);
    return response.data;
  },

  async advanceCheckout() {
    const response = await httpClient.patch(`${CHECKOUT_BASE_ROUTE}/advance`);
    return response.data;
  },

  async complete() {
    const response = await httpClient.patch(`${CHECKOUT_BASE_ROUTE}/complete`);
    return response.data;
  },

  async selectShippingMethod(shippingMethodId) {
    const response = await httpClient.patch(`${CHECKOUT_BASE_ROUTE}/select_shipping_method`, { shippingMethodId });
    return response.data;
  },

  async getPaymentMethods() {
    const response = await httpClient.get(`${CHECKOUT_BASE_ROUTE}/payment_methods`);
    return response.data;
  },

  async addPayment(payload) {
    const response = await httpClient.post(`${CHECKOUT_BASE_ROUTE}/payments`, payload);
    return response.data;
  }
};