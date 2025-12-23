<template>
  <div class="min-h-screen flex flex-col">
    <Toast />
    <header class="bg-surface-0 dark:bg-surface-900 border-b border-surface-200 dark:border-surface-700 sticky top-0 z-50">
      <nav class="container mx-auto px-4 py-4 flex items-center justify-between gap-8">
        <router-link to="/" class="text-2xl font-bold text-primary shrink-0">ReSys.Shop</router-link>
        
        <!-- Search Bar -->
        <div class="hidden md:flex grow max-w-xl relative">
          <IconField class="w-full">
            <InputIcon class="pi pi-search" />
            <InputText v-model="searchQuery" placeholder="Search for fashion..." class="w-full rounded-full" @keyup.enter="handleSearch" />
          </IconField>
        </div>

        <div class="hidden md:flex items-center space-x-6 shrink-0">
          <router-link to="/products" class="hover:text-primary">Products</router-link>
          <div v-for="taxonomy in taxonomies" :key="taxonomy.id">
             <router-link :to="'/t/' + taxonomy.root.permalink" class="hover:text-primary">{{ taxonomy.name }}</router-link>
          </div>
        </div>

        <div class="flex items-center space-x-4">
          <router-link to="/visual-search" class="p-2 hover:bg-surface-100 dark:hover:bg-surface-800 rounded-full" title="Visual Search">
            <i class="pi pi-camera text-xl"></i>
          </router-link>

          <router-link to="/cart" class="relative p-2 hover:bg-surface-100 dark:hover:bg-surface-800 rounded-full">
            <i class="pi pi-shopping-cart text-xl"></i>
            <span v-if="itemCount > 0" class="absolute top-0 right-0 bg-primary text-primary-contrast text-xs rounded-full w-5 h-5 flex items-center justify-center">
              {{ itemCount }}
            </span>
          </router-link>
          
          <template v-if="isAuthenticated">
            <router-link to="/account" class="p-2 hover:bg-surface-100 dark:hover:bg-surface-800 rounded-full">
              <i class="pi pi-user text-xl"></i>
            </router-link>
          </template>
          <template v-else>
            <router-link to="/login" class="text-sm font-medium hover:text-primary">Login</router-link>
          </template>
        </div>
      </nav>
    </header>

    <main class="grow container mx-auto px-4 py-8">
      <router-view></router-view>
    </main>

    <footer class="bg-surface-50 dark:bg-surface-950 border-t border-surface-200 dark:border-surface-700 py-12">
      <div class="container mx-auto px-4 grid grid-cols-1 md:grid-cols-4 gap-8">
        <div>
          <h3 class="text-lg font-bold mb-4">ReSys.Shop</h3>
          <p class="text-sm text-surface-600 dark:text-surface-400">Your one-stop shop for modern e-commerce.</p>
        </div>
        <div>
          <h4 class="font-bold mb-4">Shop</h4>
          <ul class="space-y-2 text-sm">
            <li><router-link to="/products">All Products</router-link></li>
            <li><router-link to="/featured">Featured</router-link></li>
          </ul>
        </div>
        <div>
          <h4 class="font-bold mb-4">Account</h4>
          <ul class="space-y-2 text-sm">
            <li><router-link to="/account">My Profile</router-link></li>
            <li><router-link to="/orders">Order History</router-link></li>
          </ul>
        </div>
        <div>
          <h4 class="font-bold mb-4">Support</h4>
          <ul class="space-y-2 text-sm">
            <li><router-link to="/contact">Contact Us</router-link></li>
            <li><router-link to="/faq">FAQ</router-link></li>
          </ul>
        </div>
      </div>
    </footer>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useCartStore } from '@/stores/cart';
import { useAuthStore } from '@/stores/auth';
import { useProductStore } from '@/stores/product';
import Toast from 'primevue/toast';
import IconField from 'primevue/iconfield';
import InputIcon from 'primevue/inputicon';
import InputText from 'primevue/inputtext';

const router = useRouter();
const cartStore = useCartStore();
const authStore = useAuthStore();
const productStore = useProductStore();

const searchQuery = ref('');
const itemCount = computed(() => cartStore.itemCount);
const isAuthenticated = computed(() => authStore.isAuthenticated);
const taxonomies = computed(() => productStore.taxonomies);

const handleSearch = () => {
  if (!searchQuery.value.trim()) return;
  router.push({ name: 'products', query: { q: searchQuery.value } });
};

onMounted(() => {
  productStore.fetchTaxonomies();
  if (authStore.isAuthenticated) {
    authStore.fetchSession();
  }
  cartStore.fetchCart();
});
</script>
