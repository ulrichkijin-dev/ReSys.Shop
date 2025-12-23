import axios from 'axios';

const imageSearchClient = axios.create({
  baseURL: import.meta.env.VITE_IMAGE_SEARCH_API_URL || 'http://localhost:8000/',
  headers: {
    'X-API-Key': import.meta.env.VITE_IMAGE_SEARCH_API_KEY || 'your-default-api-key',
  },
});

export const imageSearchService = {
  async searchByUpload(file, model = 'fashion_clip', limit = 12) {
    const formData = new FormData();
    formData.append('file', file);
    
    const response = await imageSearchClient.post(`api/search/by-upload?model=${model}&limit=${limit}`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },

  async getRecommendations(productId, model = 'efficientnet_b0', limit = 4) {
    const response = await imageSearchClient.get(`api/recommendations/by-product-id/${productId}`, {
      params: { model, limit }
    });
    return response.data;
  }
};
