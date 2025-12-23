import httpClient from '@/api/httpClient';

const INTERNAL_BASE_ROUTE = 'api/account/auth/internal';
const SESSION_BASE_ROUTE = 'api/account/auth/session';

export const authService = {
  async login(credentials) {
    const response = await httpClient.post(`${INTERNAL_BASE_ROUTE}/login`, credentials);
    if (response.data?.data?.accessToken) {
      localStorage.setItem('auth_token', response.data.data.accessToken);
      localStorage.setItem('refresh_token', response.data.data.refreshToken);
    }
    return response.data;
  },

  async register(payload) {
    const response = await httpClient.post('api/storefront/account', payload);
    return response.data;
  },

  async logout() {
    const refreshToken = localStorage.getItem('refresh_token');
    await httpClient.post(`${SESSION_BASE_ROUTE}/logout/me`, { refreshToken });
    localStorage.removeItem('auth_token');
    localStorage.removeItem('refresh_token');
  },

  async getSession() {
    const response = await httpClient.get(SESSION_BASE_ROUTE);
    return response.data;
  }
};
