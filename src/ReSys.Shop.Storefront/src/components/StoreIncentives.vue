<template>
  <div class="bg-surface-0 dark:bg-surface-950 py-12 border-y border-surface-200 dark:border-surface-800">
    <div class="container mx-auto px-4">
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
        <div v-for="item in incentives" :key="item.title" class="flex items-center gap-4 p-4 rounded-xl hover:bg-surface-50 dark:hover:bg-surface-900 transition-colors">
          <div class="w-12 h-12 rounded-full bg-primary/10 flex items-center justify-center shrink-0">
            <i :class="[item.icon, 'text-xl text-primary']"></i>
          </div>
          <div>
            <h4 class="font-bold text-surface-900 dark:text-surface-0">{{ item.title }}</h4>
            <p class="text-sm text-surface-500">{{ item.description }}</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
const incentives = [
  { title: 'Free Shipping', description: 'On all orders over $100', icon: 'pi pi-truck' },
  { title: 'Secure Payment', description: '100% secure payment processing', icon: 'pi pi-lock' },
  { title: 'Easy Returns', description: '30-day money back guarantee', icon: 'pi pi-refresh' },
  { title: '24/7 Support', description: 'Dedicated support team', icon: 'pi pi-phone' }
];
</script>
