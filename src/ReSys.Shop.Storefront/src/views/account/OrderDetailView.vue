<template>
  <div v-if="loading" class="text-center py-20">
    <ProgressSpinner />
  </div>

  <div v-else-if="order">
    <HeroSection 
      :title="'Order #' + order.number" 
      :subtitle="'Placed on ' + new Date(order.createdAt).toLocaleDateString()"
      :breadcrumbs="[{ label: 'Account', to: '/account' }, { label: 'Orders', to: '/account/orders' }, { label: order.number }]"
    />
    
    <div class="container mx-auto px-4 py-12 space-y-8">
      <div class="flex justify-between items-end border-b pb-6">
        <div>
          <router-link to="/account/orders" class="text-primary text-sm font-bold flex items-center gap-2 mb-4">
            <i class="pi pi-arrow-left"></i> Back to Orders
          </router-link>
          <h1 class="text-3xl font-bold">Order #{{ order.number }}</h1>
          <p class="text-surface-500">Placed on {{ new Date(order.createdAt).toLocaleDateString() }}</p>
        </div>
        <Tag :value="order.state" :severity="getStatusSeverity(order.state)" size="large" />
      </div>

      <div class="grid grid-cols-1 md:grid-cols-3 gap-8">
        <div class="md:col-span-2 space-y-6">
          <!-- Items -->
          <div class="bg-surface-0 dark:bg-surface-900 border rounded-2xl overflow-hidden">
            <div class="p-6 border-b font-bold bg-surface-50 dark:bg-surface-800">Items</div>
            <div class="p-0">
              <div v-for="item in order.lineItems" :key="item.id" class="flex items-center gap-4 p-6 border-b last:border-0">
                <div class="w-20 h-20 bg-surface-100 rounded flex items-center justify-center">
                  <i class="pi pi-image text-2xl text-surface-300"></i>
                </div>
                <div class="grow">
                  <p class="font-bold text-lg">{{ item.name }}</p>
                  <p class="text-sm text-surface-500">Quantity: {{ item.quantity }}</p>
                </div>
                <div class="text-right">
                  <p class="font-bold">{{ formatPrice(item.price * item.quantity) }}</p>
                  <p class="text-xs text-surface-400">{{ formatPrice(item.price) }} each</p>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="space-y-6">
          <!-- Summary -->
          <div class="bg-surface-0 dark:bg-surface-900 border rounded-2xl p-6 shadow-sm">
            <h3 class="font-bold mb-4">Order Totals</h3>
            <div class="space-y-3 text-sm">
              <div class="flex justify-between text-surface-600">
                <span>Subtotal</span>
                <span>{{ formatPrice(order.total) }}</span>
              </div>
              <div class="flex justify-between font-bold text-lg border-t pt-3 mt-3">
                <span>Total</span>
                <span class="text-primary">{{ formatPrice(order.total) }}</span>
              </div>
            </div>
          </div>

          <!-- Shipping Info -->
          <div class="bg-surface-50 dark:bg-surface-800 rounded-2xl p-6">
            <h3 class="font-bold mb-3">Shipping Status</h3>
            <p class="text-sm text-surface-600 mb-2">{{ order.shipmentState || 'Processing' }}</p>
            <div class="flex items-center gap-2 text-xs text-primary font-bold">
              <i class="pi pi-box"></i>
              <span>Tracking info will be available soon</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import { accountService } from '@/services/account.service';
import Tag from 'primevue/tag';
import ProgressSpinner from 'primevue/progressspinner';
import HeroSection from '@/components/HeroSection.vue';

const route = useRoute();
const order = ref(null);
const loading = ref(true);

onMounted(async () => {
  try {
    const res = await accountService.getOrderByNumber(route.params.number);
    order.value = res.data;
  } catch (err) {
    console.error(err);
  } finally {
    loading.value = false;
  }
});

const getStatusSeverity = (status) => {
  switch (status?.toLowerCase()) {
    case 'complete': return 'success';
    case 'canceled': return 'danger';
    default: return 'info';
  }
};

const formatPrice = (val) => {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: order.value?.currency || 'USD' }).format(val);
};
</script>
