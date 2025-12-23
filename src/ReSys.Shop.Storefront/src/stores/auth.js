import { defineStore } from 'pinia';
import { authService } from '@/services/auth.service';
import { cartService } from '@/services/cart.service';

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null,
    isAuthenticated: !!localStorage.getItem('auth_token'),
    loading: false,
    error: null,
  }),

  actions: {
    async login(credentials) {
      this.loading = true;
      try {
        const response = await authService.login(credentials);
        this.user = response.data.userProfile;
        this.isAuthenticated = true;
        
        // IMPORTANT: Associate guest cart with user account after login
        await cartService.associate().catch(e => console.warn("Cart association failed:", e));
        
        return response;
      } catch (err) {
        this.error = err.message;
        throw err;
      } finally {
        this.loading = false;
      }
    },

    async logout() {
      try {
        await authService.logout();
      } finally {
        this.user = null;
        this.isAuthenticated = false;
      }
    },

    async fetchSession() {
      if (!this.isAuthenticated) return;
      try {
        const response = await authService.getSession();
        this.user = response.data;
      } catch (err) {
        this.logout();
      }
    }
  }
});
