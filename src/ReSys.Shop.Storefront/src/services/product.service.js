import httpClient from '@/api/httpClient';

const PRODUCTS_ROUTE = 'api/storefront/products';
const TAXONOMIES_ROUTE = 'api/storefront/taxonomies';
const TAXONS_ROUTE = 'api/storefront/taxons';

export const productService = {
  async getProducts(params = {}) {
    const response = await httpClient.get(PRODUCTS_ROUTE, { params });
    return response.data;
  },

  async getProductBySlug(slug) {
    const response = await httpClient.get(`${PRODUCTS_ROUTE}/${slug}`);
    return response.data;
  },

  async getTaxonomies(params = {}) {
    const response = await httpClient.get(TAXONOMIES_ROUTE, { params });
    return response.data;
  },

  async getTaxonById(id) {
    const response = await httpClient.get(`${TAXONS_ROUTE}/${id}`);
    return response.data;
  }
};
