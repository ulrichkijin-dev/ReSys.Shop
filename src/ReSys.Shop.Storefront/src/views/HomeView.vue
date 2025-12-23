<template>
  <div class="space-y-20">
    <!-- Main Hero Block -->
    <section class="relative h-[70vh] flex items-center overflow-hidden bg-surface-900 text-white">
      <div class="absolute inset-0 z-0">
        <img src="https://images.unsplash.com/photo-1441986300917-64674bd600d8?auto=format&fit=crop&q=80" 
             class="w-full h-full object-cover opacity-40" alt="Hero Background">
      </div>
      <div class="container mx-auto px-4 relative z-10">
        <div class="max-w-2xl">
          <span class="inline-block bg-primary px-3 py-1 rounded text-xs font-bold uppercase tracking-widest mb-4">New Collection 2026</span>
          <h1 class="text-6xl md:text-8xl font-extrabold mb-6 leading-tight">Elevate Your <span class="text-primary">Style</span></h1>
          <p class="text-xl text-surface-300 mb-10 leading-relaxed">Discover the latest trends in high-end fashion. Curated for those who demand excellence in every stitch.</p>
          <div class="flex gap-4">
            <router-link to="/products">
              <Button label="Shop Collection" size="large" />
            </router-link>
            <router-link to="/visual-search">
              <Button label="Search by Image" icon="pi pi-camera" size="large" severity="secondary" outlined />
            </router-link>
          </div>
        </div>
      </div>
    </section>

    <!-- Incentives Block -->
    <StoreIncentives />

    <!-- Category Collections Block -->
    <section class="container mx-auto px-4">
      <div class="flex justify-between items-end mb-12">
        <div>
          <h2 class="text-4xl font-bold mb-2">Shop by Category</h2>
          <p class="text-surface-500">Explore our diverse range of fashion categories.</p>
        </div>
        <router-link to="/products" class="text-primary font-bold hover:underline">View All</router-link>
      </div>
      
      <div class="grid grid-cols-1 md:grid-cols-3 gap-8">
        <router-link v-for="tax in taxonomies.slice(0, 3)" :key="tax.id" 
          :to="'/products?taxonId=' + tax.root.id"
          class="group relative h-96 rounded-2xl overflow-hidden shadow-lg"
        >
          <div class="absolute inset-0 bg-linear-to-t from-black/80 via-transparent to-transparent z-10"></div>
          <img src="https://images.unsplash.com/photo-1445205170230-053b830c6050?auto=format&fit=crop&q=80" 
               class="w-full h-full object-cover transition-transform duration-700 group-hover:scale-110" alt="Category">
          <div class="absolute bottom-8 left-8 z-20">
            <h3 class="text-2xl font-bold text-white mb-2">{{ tax.name }}</h3>
            <span class="text-white/80 flex items-center gap-2 font-medium">
              Explore Collection <i class="pi pi-arrow-right transition-transform group-hover:translate-x-2"></i>
            </span>
          </div>
        </router-link>
      </div>
    </section>

    <!-- Featured Products Block -->
    <section class="container mx-auto px-4 pb-20">
      <h2 class="text-4xl font-bold mb-12">Featured Arrivals</h2>
      <div v-if="loading" class="grid grid-cols-1 md:grid-cols-4 gap-8">
        <Skeleton v-for="i in 4" :key="i" height="400px" />
      </div>
      <div v-else class="grid grid-cols-1 md:grid-cols-4 gap-8">
        <div v-for="product in products.slice(0, 4)" :key="product.id" class="group border border-surface-200 dark:border-surface-800 rounded-2xl overflow-hidden flex flex-col h-full bg-surface-0 dark:bg-surface-900 transition-all hover:shadow-2xl">
          <div class="h-80 relative overflow-hidden bg-surface-100 dark:bg-surface-800">
             <img :src="product.imageUrl" class="w-full h-full object-cover" />
             <div class="absolute inset-0 bg-black/20 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center">
                <Button label="View" icon="pi pi-eye" rounded @click="$router.push('/product/' + product.slug)" />
             </div>
          </div>
          <div class="p-6">
            <h3 class="font-bold text-lg mb-1">{{ product.name }}</h3>
            <p class="text-primary font-bold">{{ product.displayPrice }} {{ product.currency }}</p>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup>
import { onMounted, computed } from 'vue';
import { useProductStore } from '@/stores/product';
import StoreIncentives from '@/components/StoreIncentives.vue';
import Button from 'primevue/button';
import Skeleton from 'primevue/skeleton';

const productStore = useProductStore();
const taxonomies = computed(() => productStore.taxonomies);
const products = computed(() => productStore.products);
const loading = computed(() => productStore.loading);

onMounted(() => {
  productStore.fetchTaxonomies();
  productStore.fetchProducts({ pageSize: 4 });
});
</script>