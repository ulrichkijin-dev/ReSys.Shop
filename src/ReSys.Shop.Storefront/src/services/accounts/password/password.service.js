import httpClient from '@/api/httpClient';

export const passwordService = {
  async reset(payload) {
    const response = await httpClient.post('api/account/password/reset', payload);
    return response.data;
  },
  async forgot(email) {
    const response = await httpClient.post('api/account/password/forgot', { email });
    return response.data;
  }
};
