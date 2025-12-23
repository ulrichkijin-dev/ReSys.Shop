import httpClient from '@/api/httpClient';

export const emailService = {
  async confirm(payload) {
    const response = await httpClient.post('api/account/email/confirm', payload);
    return response.data;
  }
};
