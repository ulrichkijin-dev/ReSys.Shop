import axios from 'axios';

const httpClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/',
  headers: {
    'Content-Type': 'application/json',
  },
});

httpClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('auth_token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    // This token is used by the backend to identify guest carts (AdhocCustomerId)
    const cartToken = localStorage.getItem('cart_token');
    if (cartToken) {
      config.headers['X-Cart-Token'] = cartToken;
    }

    return config;
  },
  (error) => Promise.reject(error)
);

export default httpClient;