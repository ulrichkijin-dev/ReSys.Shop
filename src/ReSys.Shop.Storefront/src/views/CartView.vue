<template>
  <HeroSection 
    title="Shopping Bag" 
    :subtitle="itemCount > 0 ? `You have ${itemCount} items ready for checkout.` : 'Your bag is looking a bit light.'"
    :breadcrumbs="[{ label: 'Bag' }]"
  />
  
  <div class="container mx-auto px-4">
    <div v-if="loading && !cart" class="text-center py-12">
      <i class="pi pi-spin pi-spinner text-4xl"></i>
    </div>

    <div v-else-if="!cart || cart.lineItems.length === 0" class="text-center py-12">
      <p class="text-xl mb-6">Your cart is empty.</p>
      <router-link to="/products">
        <Button label="Go Shopping" />
      </router-link>
    </div>

    <div v-else class="grid grid-cols-1 lg:grid-cols-3 gap-8">
      <div class="lg:col-span-2 space-y-4">
        <div v-for="item in cart.lineItems" :key="item.id" class="flex items-center gap-4 p-4 border border-surface-200 dark:border-surface-700 rounded-lg">
          <div class="w-20 h-20 bg-surface-100 dark:bg-surface-800 rounded flex items-center justify-center">
            <i class="pi pi-image text-2xl text-surface-400"></i>
          </div>
          <div class="grow">
            <h3 class="font-bold">{{ item.name }}</h3>
            <p class="text-sm text-surface-600 dark:text-surface-400">{{ item.sku }}</p>
          </div>
          <div class="flex items-center gap-2">
            <Button icon="pi pi-minus" severity="secondary" rounded text @click="updateQuantity(item, item.quantity - 1)" :disabled="item.quantity <= 1" />
            <span class="w-8 text-center">{{ item.quantity }}</span>
            <Button icon="pi pi-plus" severity="secondary" rounded text @click="updateQuantity(item, item.quantity + 1)" />
          </div>
          <div class="text-right min-w-25">
            <p class="font-bold">{{ formatPrice(item.total, cart.currency) }}</p>
            <Button icon="pi pi-trash" severity="danger" text @click="removeItem(item.id)" />
          </div>
        </div>
      </div>

      <div class="bg-surface-50 dark:bg-surface-900 p-6 rounded-lg h-fit">
        <h2 class="text-xl font-bold mb-4">Order Summary</h2>
        <div class="space-y-2 mb-4">
          <div class="flex justify-between">
            <span>Subtotal</span>
            <span>{{ formatPrice(cart.itemTotal, cart.currency) }}</span>
          </div>
          <div class="flex justify-between">
            <span>Shipping</span>
            <span>{{ formatPrice(cart.shipmentTotal, cart.currency) }}</span>
          </div>
          <div v-if="cart.adjustmentTotal !== 0" class="flex justify-between">
            <span>Adjustments</span>
            <span>{{ formatPrice(cart.adjustmentTotal, cart.currency) }}</span>
          </div>
          <Divider />
          <div class="flex justify-between text-xl font-bold">
            <span>Total</span>
            <span>{{ formatPrice(cart.total, cart.currency) }}</span>
          </div>
        </div>
        <router-link to="/checkout">
          <Button label="Proceed to Checkout" class="w-full" size="large" />
        </router-link>
      </div>
    </div>
  </div>

  <div class="mt-20">
    <StoreIncentives />
  </div>
</template>

<script setup>
import { computed } from 'vue';
import { useCartStore } from '@/stores/cart';
import Button from 'primevue/button';
import Divider from 'primevue/divider';
import HeroSection from '@/components/HeroSection.vue';
import StoreIncentives from '@/components/StoreIncentives.vue';

const cartStore = useCartStore();

const cart = computed(() => cartStore.cart);
const loading = computed(() => cartStore.loading);

const formatPrice = (value, currency) => {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: currency || 'USD' }).format(value);
};

const updateQuantity = (item, quantity) => {
  cartStore.updateQuantity(item.id, quantity);
};

const removeItem = (id) => {
  cartStore.removeFromCart(id);
};
</script>
