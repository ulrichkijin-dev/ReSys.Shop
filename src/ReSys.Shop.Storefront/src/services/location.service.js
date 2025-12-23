import httpClient from '@/api/httpClient';

export const locationService = {
  async getCountries(params = {}) {
    const response = await httpClient.get('api/storefront/countries', { params });
    return response.data;
  },
  async getCountryById(id) {
    const response = await httpClient.get(`api/storefront/countries/${id}`);
    return response.data;
  },
  async getDefaultCountry() {
    const response = await httpClient.get('api/storefront/countries/default');
    return response.data;
  },
  async getStates(params = {}) {
    const response = await httpClient.get('api/storefront/states', { params });
    return response.data;
  },
  async getStateById(id) {
    const response = await httpClient.get(`api/storefront/states/${id}`);
    return response.data;
  }
};
