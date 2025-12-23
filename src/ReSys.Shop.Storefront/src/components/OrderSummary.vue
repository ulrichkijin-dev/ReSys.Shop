<template>
  <div class="space-y-4 mb-6">
    <div v-for="item in cart?.lineItems" :key="item.id" class="flex gap-4 p-2 border-b border-surface-100 dark:border-surface-800">
      <div class="w-16 h-16 bg-surface-100 rounded shrink-0 flex items-center justify-center">
        <i class="pi pi-image text-surface-300"></i>
      </div>
      <div class="grow">
        <p class="text-sm font-bold line-clamp-1">{{ item.name }}</p>
        <p class="text-xs text-surface-500">Quantity: {{ item.quantity }}</p>
      </div>
      <div class="text-right">
        <p class="text-sm font-bold">{{ formatPrice(item.total) }}</p>
        <p class="text-xs text-surface-400">@ {{ formatPrice(item.price) }}</p>
      </div>
    </div>
  </div>

  <div class="space-y-3 py-4 text-sm">
    <!-- Coupon Input -->
    <div class="pb-4 border-b border-surface-100 dark:border-surface-800">
      <label class="text-xs font-bold uppercase text-surface-500 mb-2 block">Promo Code</label>
      <div v-if="!cart?.promoCode" class="flex gap-2">
        <InputText v-model="couponCode" placeholder="ENTER CODE" class="grow text-sm" />
        <Button label="Apply" size="small" @click="applyCoupon" :loading="processing" />
      </div>
      <div v-else class="flex justify-between items-center bg-green-50 dark:bg-green-900/20 p-2 rounded border border-green-200">
        <span class="text-green-700 font-bold text-xs">{{ cart.promoCode }} APPLIED</span>
        <Button icon="pi pi-times" severity="danger" text size="small" @click="removeCoupon" />
      </div>
    </div>

    <div class="flex justify-between">
      <span class="text-surface-500">Items ({{ itemCount }})</span>
      <span>{{ formatPrice(cart?.itemTotal) }}</span>
    </div>
    <div class="flex justify-between" v-if="cart?.shipmentTotal > 0">
      <span class="text-surface-500">Shipping</span>
      <span>{{ formatPrice(cart?.shipmentTotal) }}</span>
    </div>
    <div class="flex justify-between text-green-600 font-medium" v-if="cart?.adjustmentTotal < 0">
      <span class="text-surface-500">Discount</span>
      <span>{{ formatPrice(cart?.adjustmentTotal) }}</span>
    </div>
    <Divider />
    <div class="flex justify-between text-xl font-bold pt-2">
      <span>Total</span>
      <span class="text-primary">{{ formatPrice(cart?.total) }}</span>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue';
import { useCartStore } from '@/stores/cart';
import Divider from 'primevue/divider';
import InputText from 'primevue/inputtext';
import Button from 'primevue/button';

const props = defineProps({
  cart: { type: Object, required: true }
});

const cartStore = useCartStore();
const couponCode = ref('');
const processing = ref(false);

const applyCoupon = async () => {
  if (!couponCode.value) return;
  processing.value = true;
  try {
    await cartStore.applyCoupon(couponCode.value);
    couponCode.value = '';
  } finally {
    processing.value = false;
  }
};

const removeCoupon = async () => {
  processing.value = true;
  try {
    // In backend CartModule.Actions.RemoveCoupon takes an optional CouponCode
    await cartStore.applyCoupon(''); // Or dedicated remove action
    await cartStore.fetchCart();
  } finally {
    processing.value = false;
  }
};

const itemCount = computed(() => props.cart?.lineItems?.reduce((acc, i) => acc + i.quantity, 0) || 0);

const formatPrice = (value) => {
  return new Intl.NumberFormat('en-US', { 
    style: 'currency', 
    currency: props.cart?.currency || 'USD' 
  }).format(value || 0);
};
</script>
