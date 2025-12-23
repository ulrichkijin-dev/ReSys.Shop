import { defineStore } from 'pinia';
import { cartService } from '@/services/cart.service';

export const useCartStore = defineStore('cart', {
  state: () => ({
    cart: null,
    loading: false,
    error: null,
  }),

  getters: {
    itemCount: (state) => state.cart?.lineItems?.reduce((acc, item) => acc + item.quantity, 0) || 0,
    total: (state) => state.cart?.total || 0,
    lineItems: (state) => state.cart?.lineItems || [],
    isCartEmpty: (state) => !state.cart || state.cart.lineItems.length === 0,
  },

  actions: {
    async fetchCart() {
      this.loading = true;
      try {
        const response = await cartService.get();
        this.cart = response.data;
        // In the backend, Token maps to src.AdhocCustomerId
        if (this.cart?.token) {
          localStorage.setItem('cart_token', this.cart.token);
        }
      } catch (err) {
        if (err.response?.status === 404) {
          this.cart = null;
        }
        this.error = err.message;
      } finally {
        this.loading = false;
      }
    },

    async addToCart(variantId, quantity = 1) {
      this.loading = true;
      try {
        if (!this.cart) {
          const storeId = import.meta.env.VITE_DEFAULT_STORE_ID; 
          const response = await cartService.create(storeId);
          this.cart = response.data;
          localStorage.setItem('cart_token', this.cart.token);
        }
        const response = await cartService.addItem(variantId, quantity);
        this.cart = response.data;
      } catch (err) {
        this.error = err.message;
        throw err;
      } finally {
        this.loading = false;
      }
    },

    async updateQuantity(lineItemId, quantity) {
      try {
        const response = await cartService.setQuantity(lineItemId, quantity);
        this.cart = response.data;
      } catch (err) {
        this.error = err.message;
      }
    },

    async removeFromCart(lineItemId) {
      try {
        const response = await cartService.removeItem(lineItemId);
        this.cart = response.data;
      } catch (err) {
        this.error = err.message;
      }
    },

    async applyCoupon(code) {
      try {
        const response = await cartService.applyCoupon(code);
        this.cart = response.data;
      } catch (err) {
        this.error = err.response?.data?.message || err.message;
        throw err;
      }
    }
  }
});