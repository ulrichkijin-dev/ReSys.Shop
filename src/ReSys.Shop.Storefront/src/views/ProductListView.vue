<template>
  <HeroSection 
    :title="activeTaxonName || 'Our Products'" 
    subtitle="Explore our curated collection of high-quality fashion items."
    :breadcrumbs="[{ label: 'Shop', to: '/products' }, { label: activeTaxonName || 'All' }]"
  />
  
  <div class="container mx-auto px-4">
    <div class="grid grid-cols-1 md:grid-cols-4 gap-8">
      <!-- Sidebar Filters -->
      <aside class="hidden md:block space-y-8">
        <div>
          <h3 class="font-bold text-lg mb-4 uppercase tracking-wider text-surface-500">Categories</h3>
          <div class="flex flex-col space-y-2">
            <router-link to="/products" class="hover:text-primary transition-colors">All Products</router-link>
            <div v-for="taxonomy in taxonomies" :key="taxonomy.id">
              <p class="font-bold mt-4 mb-2">{{ taxonomy.name }}</p>
              <div class="flex flex-col space-y-1 ml-2 border-l-2 border-surface-100 dark:border-surface-800 pl-4">
                 <button v-for="taxon in taxonomy.root?.children" :key="taxon.id" 
                   @click="filterByTaxon(taxon.id)"
                   class="text-left text-sm hover:text-primary py-1 transition-colors"
                   :class="{'text-primary font-bold': activeTaxonId === taxon.id}"
                 >
                   {{ taxon.name }}
                 </button>
              </div>
            </div>
          </div>
        </div>

        <div>
          <h3 class="font-bold text-lg mb-4 uppercase tracking-wider text-surface-500">Price Range</h3>
          <Slider v-model="priceRange" range :min="0" :max="500" class="w-full mt-6" />
          <div class="flex justify-between mt-4 text-sm text-surface-600">
            <span>$0</span>
            <span>$500+</span>
          </div>
        </div>
      </aside>

      <!-- Product Grid -->
      <div class="md:col-span-3">
        <div class="flex justify-between items-center mb-8">
          <h1 class="text-3xl font-bold">Our Products</h1>
          <div class="flex items-center gap-4">
            <span class="text-sm text-surface-500">{{ totalCount }} items found</span>
            <Select v-model="sortBy" :options="sortOptions" optionLabel="label" placeholder="Sort By" class="w-48" />
          </div>
        </div>
        
        <div v-if="loading" class="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-3 gap-6">
          <div v-for="i in 6" :key="i" class="border border-surface-200 dark:border-surface-700 rounded-lg p-4">
            <Skeleton width="100%" height="250px" class="mb-4" />
            <Skeleton width="60%" height="1.5rem" class="mb-2" />
            <Skeleton width="40%" height="1rem" />
          </div>
        </div>

        <div v-else class="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-3 gap-6">
          <div v-for="product in products" :key="product.id" class="group border border-surface-200 dark:border-surface-700 rounded-xl overflow-hidden flex flex-col transition-all hover:shadow-2xl">
            <div class="h-80 relative overflow-hidden bg-surface-100 dark:bg-surface-800">
               <img v-if="product.imageUrl" :src="product.imageUrl" :alt="product.name" class="object-cover w-full h-full transition-transform group-hover:scale-110" />
               <i v-else class="pi pi-image text-4xl text-surface-400"></i>
               
               <!-- Hover Actions -->
               <div class="absolute inset-0 bg-black/20 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center gap-2">
                  <Button icon="pi pi-eye" rounded severity="secondary" @click="quickView(product)" />
                  <Button icon="pi pi-shopping-cart" rounded @click="addToCart(product)" />
               </div>
            </div>
            <div class="p-5 flex-grow">
              <router-link :to="'/product/' + product.slug">
                <h3 class="font-bold text-lg mb-1 group-hover:text-primary transition-colors">{{ product.name }}</h3>
              </router-link>
              <p class="text-surface-500 text-sm mb-4 line-clamp-1">{{ product.description }}</p>
              <div class="flex items-center justify-between mt-auto">
                <span class="text-xl font-extrabold">{{ formatPrice(product.price, product.currency) }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Pagination -->
        <div class="mt-12" v-if="totalCount > pageSize">
          <Paginator 
            :rows="pageSize" 
            :totalRecords="totalCount" 
            :rowsPerPageOptions="[12, 24, 48]"
            @page="onPageChange"
          />
        </div>
      </div>

      <!-- QuickView Dialog -->
      <Dialog v-model:visible="showQuickView" :style="{width: '800px'}" modal pt:root:class="border-none" pt:mask:class="backdrop-blur-sm">
         <div v-if="selectedProduct" class="grid grid-cols-1 md:grid-cols-2 gap-8 p-4">
            <div class="aspect-square bg-surface-100 rounded-xl overflow-hidden">
               <img :src="selectedProduct.imageUrl" class="w-full h-full object-cover" />
            </div>
            <div class="flex flex-col">
               <h2 class="text-3xl font-bold mb-2">{{ selectedProduct.name }}</h2>
               <p class="text-2xl text-primary font-bold mb-6">{{ formatPrice(selectedProduct.price, selectedProduct.currency) }}</p>
               <p class="text-surface-600 mb-8">{{ selectedProduct.description }}</p>
               <div class="mt-auto flex gap-4">
                  <Button label="Add to Cart" icon="pi pi-shopping-cart" class="flex-grow" @click="addToCart(selectedProduct)" />
                  <Button label="View Details" severity="secondary" @click="$router.push('/product/' + selectedProduct.slug)" />
               </div>
            </div>
         </div>
      </Dialog>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, watch } from 'vue';
import { useRoute } from 'vue-router';
import { useProductStore } from '@/stores/product';
import { useCartStore } from '@/stores/cart';
import { useToast } from 'primevue/usetoast';
import Button from 'primevue/button';
import Skeleton from 'primevue/skeleton';
import Slider from 'primevue/slider';
import Select from 'primevue/select';
import Dialog from 'primevue/dialog';
import Paginator from 'primevue/paginator';
import HeroSection from '@/components/HeroSection.vue';

const route = useRoute();
const toast = useToast();
const productStore = useProductStore();
const cartStore = useCartStore();

const products = computed(() => productStore.products);
const loading = computed(() => productStore.loading);
const totalCount = computed(() => productStore.totalCount);
const taxonomies = computed(() => productStore.taxonomies);

const activeTaxonId = ref(null);
const activeTaxonName = computed(() => {
  if (!activeTaxonId.value) return null;
  for (const tax of taxonomies.value) {
    const found = tax.root?.children?.find(c => c.id === activeTaxonId.value);
    if (found) return found.name;
  }
  return null;
});

const priceRange = ref([0, 500]);
const sortBy = ref({ label: 'Newest', value: 'createdAt desc' });
const currentPage = ref(1);
const pageSize = ref(12);
const showQuickView = ref(false);
const selectedProduct = ref(null);

const sortOptions = [
  { label: 'Newest', value: 'createdAt desc' },
  { label: 'Price: Low to High', value: 'price asc' },
  { label: 'Price: High to Low', value: 'price desc' },
  { label: 'Name: A-Z', value: 'name asc' },
];

const fetchFilteredProducts = () => {
  productStore.fetchProducts({
    taxonId: activeTaxonId.value,
    sortBy: sortBy.value.value,
    searchTerm: route.query.q || '',
    page: currentPage.value,
    pageSize: pageSize.value
  });
};

onMounted(() => {
  productStore.fetchTaxonomies();
  fetchFilteredProducts();
});

watch([activeTaxonId, sortBy, () => route.query.q], () => {
  currentPage.value = 1; 
  fetchFilteredProducts();
});

const onPageChange = (event) => {
  currentPage.value = event.page + 1;
  pageSize.value = event.rows;
  fetchFilteredProducts();
  window.scrollTo({ top: 0, behavior: 'smooth' });
};

const addToCart = async (product) => {
  try {
    await cartStore.addToCart(product.id, 1);
    toast.add({ severity: 'success', summary: 'Added to Cart', detail: product.name, life: 3000 });
  } catch (err) {
    toast.add({ severity: 'error', summary: 'Error', detail: 'Could not add to cart', life: 3000 });
  }
};

const filterByTaxon = (id) => {
  activeTaxonId.value = id;
};

const quickView = (product) => {
  selectedProduct.value = product;
  showQuickView.value = true;
};

const formatPrice = (value, currency) => {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: currency || 'USD' }).format(value);
};
</script>