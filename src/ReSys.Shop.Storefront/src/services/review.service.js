import httpClient from '@/api/httpClient';

export const reviewService = {
  async getProductReviews(productId, params = {}) {
    const response = await httpClient.get(`api/storefront/products/${productId}/reviews`, { params });
    return response.data;
  },
  async createReview(productId, payload) {
    const response = await httpClient.post(`api/storefront/products/${productId}/reviews`, payload);
    return response.data;
  },
  async voteReview(reviewId, helpful) {
    const response = await httpClient.post(`api/storefront/reviews/${reviewId}/vote`, { helpful });
    return response.data;
  }
};
