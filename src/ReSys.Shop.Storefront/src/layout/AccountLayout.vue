<template>
  <div class="grid grid-cols-1 md:grid-cols-4 gap-8">
    <aside class="md:col-span-1">
      <div class="bg-surface-50 dark:bg-surface-900 rounded-xl p-4 border border-surface-200 dark:border-surface-700">
        <h2 class="text-xl font-bold mb-6 px-4">My Account</h2>
        <nav class="flex flex-col space-y-1">
          <router-link 
            v-for="item in menuItems" 
            :key="item.to" 
            :to="item.to"
            class="flex items-center space-x-3 px-4 py-3 rounded-lg transition-colors hover:bg-surface-200 dark:hover:bg-surface-800"
            active-class="bg-primary text-primary-contrast hover:bg-primary"
          >
            <i :class="item.icon"></i>
            <span>{{ item.label }}</span>
          </router-link>
          <button @click="logout" class="flex items-center space-x-3 px-4 py-3 rounded-lg text-red-500 hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors text-left w-full mt-4">
            <i class="pi pi-sign-out"></i>
            <span>Logout</span>
          </button>
        </nav>
      </div>
    </aside>

    <main class="md:col-span-3">
      <router-view></router-view>
    </main>
  </div>
</template>

<script setup>
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth';

const router = useRouter();
const authStore = useAuthStore();

const menuItems = [
  { label: 'Profile', to: '/account/profile', icon: 'pi pi-user' },
  { label: 'Addresses', to: '/account/addresses', icon: 'pi pi-map-marker' },
  { label: 'Orders', to: '/account/orders', icon: 'pi pi-shopping-bag' },
];

const logout = async () => {
  await authStore.logout();
  router.push('/login');
};
</script>
