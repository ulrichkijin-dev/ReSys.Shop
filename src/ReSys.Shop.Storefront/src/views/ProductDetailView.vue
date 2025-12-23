<template>
  <div v-if="loading" class="flex justify-center py-20">
    <ProgressSpinner />
  </div>

  <div v-else-if="product">
    <HeroSection 
      :title="product.name" 
      :subtitle="product.metaTitle || 'Premium Quality'"
      :breadcrumbs="[{ label: 'Shop', to: '/products' }, { label: product.name }]"
    />
    
    <div class="container mx-auto px-4 grid grid-cols-1 md:grid-cols-2 gap-12">
      <!-- Image Gallery -->
      <div class="space-y-4">
        <div class="aspect-square bg-surface-100 dark:bg-surface-800 rounded-xl overflow-hidden flex items-center justify-center border border-surface-200 dark:border-surface-700">
          <img v-if="product.imageUrl" :src="product.imageUrl" :alt="product.name" class="w-full h-full object-cover" />
          <i v-else class="pi pi-image text-6xl text-surface-400"></i>
        </div>
      </div>

      <!-- Product Info -->
      <div class="flex flex-col">
        <h1 class="text-4xl font-bold mb-2">{{ product.name }}</h1>
        <p class="text-xl text-primary font-bold mb-6">{{ formatPrice(product.price, product.currency) }}</p>
        
        <div class="prose dark:prose-invert mb-8">
          <p>{{ product.description }}</p>
        </div>

        <div class="mb-8">
          <h3 class="font-bold mb-3">Specifications</h3>
          <ul class="space-y-2">
            <li v-for="prop in product.properties" :key="prop.name" class="flex border-b border-surface-200 dark:border-surface-700 py-2">
              <span class="font-medium w-1/3">{{ prop.presentation }}</span>
              <span class="text-surface-600 dark:text-surface-400">{{ prop.value }}</span>
            </li>
          </ul>
        </div>

        <div class="mt-auto flex gap-4">
          <InputNumber v-model="quantity" showButtons :min="1" class="w-32" />
          <Button label="Add to Cart" icon="pi pi-shopping-cart" class="flex-grow" @click="addToCart" :loading="cartLoading" />
        </div>
      </div>

      <!-- Recommendations Section -->
      <div v-if="recommendations.length > 0" class="col-span-full mt-16">
        <h2 class="text-2xl font-bold mb-8">Recommended for You</h2>
        <div class="grid grid-cols-2 md:grid-cols-4 gap-6">
          <router-link 
            v-for="rec in recommendations" 
            :key="rec.product_id" 
            :to="'/product/' + rec.product_slug"
            class="group"
          >
            <div class="border border-surface-200 dark:border-surface-700 rounded-lg overflow-hidden flex flex-col h-full">
              <div class="aspect-square relative overflow-hidden bg-surface-100 dark:bg-surface-800">
                <img :src="rec.image_url" class="w-full h-full object-cover transition-transform group-hover:scale-105" />
              </div>
              <div class="p-4 flex-grow bg-surface-0 dark:bg-surface-900">
                <h3 class="font-bold text-sm truncate mb-1">{{ rec.product_name }}</h3>
                <p class="text-primary font-bold text-sm">{{ rec.price }} {{ rec.currency }}</p>
              </div>
            </div>
          </router-link>
        </div>
      </div>

      <!-- Reviews Section -->
      <div class="col-span-full">
        <ProductReviews :productId="product.id" />
      </div>
    </div>

    <div class="mt-20">
      <StoreIncentives />
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue';
import { useRoute } from 'vue-router';
import { productService } from '@/services/product.service';
import { useCartStore } from '@/stores/cart';
import { useImageSearchStore } from '@/stores/imageSearch';
import ProductReviews from '@/components/ProductReviews.vue';
import StoreIncentives from '@/components/StoreIncentives.vue';
import Button from 'primevue/button';
import InputNumber from 'primevue/inputnumber';
import ProgressSpinner from 'primevue/progressspinner';
import HeroSection from '@/components/HeroSection.vue';

const route = useRoute();
const cartStore = useCartStore();
const imageSearchStore = useImageSearchStore();

const product = ref(null);
const loading = ref(true);
const cartLoading = ref(false);
const quantity = ref(1);

const fetchProduct = async () => {
  loading.value = true;
  try {
    const response = await productService.getProductBySlug(route.params.slug);
    product.value = response.data;
    
    // Fetch recommendations based on this product's visual features
    if (product.value?.id) {
      imageSearchStore.fetchRecommendations(product.value.id);
    }
  } catch (err) {
    console.error(err);
  } finally {
    loading.value = false;
  }
};

const recommendations = computed(() => {
  return product.value ? imageSearchStore.recommendations[product.value.id] || [] : [];
});

const formatPrice = (value, currency) => {
  if (!value) return '';
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: currency || 'USD' }).format(value);
};

const addToCart = async () => {
  cartLoading.value = true;
  try {
    // Using product ID as variant ID for now (simplification)
    await cartStore.addToCart(product.value.id, quantity.value);
  } finally {
    cartLoading.value = false;
  }
};

onMounted(fetchProduct);
</script>