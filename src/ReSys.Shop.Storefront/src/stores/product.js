import { defineStore } from 'pinia';
import { productService } from '@/services/product.service';

export const useProductStore = defineStore('product', {
  state: () => ({
    products: [],
    taxonomies: [],
    loading: false,
    error: null,
    totalCount: 0,
  }),

  actions: {
    async fetchProducts(params = {}) {
      this.loading = true;
      try {
        const response = await productService.getProducts(params);
        this.products = response.data.items;
        this.totalCount = response.data.totalCount;
      } catch (err) {
        this.error = err.message;
      } finally {
        this.loading = false;
      }
    },

    async fetchTaxonomies() {
      try {
        const response = await productService.getTaxonomies();
        this.taxonomies = response.data.items;
      } catch (err) {
        this.error = err.message;
      }
    }
  }
});
