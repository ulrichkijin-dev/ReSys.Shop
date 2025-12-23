import { defineStore } from 'pinia';
import { imageSearchService } from '@/services/imageSearch.service';

export const useImageSearchStore = defineStore('imageSearch', {
  state: () => ({
    searchResults: [],
    recommendations: {}, // Keyed by productId
    loading: false,
    error: null,
  }),

  actions: {
    async searchByImage(file) {
      this.loading = true;
      this.error = null;
      try {
        const response = await imageSearchService.searchByUpload(file, 'fashion_clip');
        this.searchResults = response.results;
      } catch (err) {
        this.error = err.response?.data?.detail || 'Image search failed';
        throw err;
      } finally {
        this.loading = false;
      }
    },

    async fetchRecommendations(productId) {
      if (this.recommendations[productId]) return;
      
      try {
        const response = await imageSearchService.getRecommendations(productId, 'efficientnet_b0');
        this.recommendations[productId] = response.results;
      } catch (err) {
        console.error('Failed to fetch recommendations:', err);
      }
    }
  }
});
