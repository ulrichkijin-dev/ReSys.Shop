<template>
  <div class="max-w-md mx-auto py-20 text-center">
    <Card class="border-none shadow-2xl">
      <template #content>
        <div v-if="processing" class="py-12">
          <ProgressSpinner />
          <h2 class="text-xl font-bold mt-6">Confirming Payment...</h2>
          <p class="text-surface-500">Please do not refresh the page.</p>
        </div>

        <div v-else-if="success" class="py-12">
          <div class="w-20 h-20 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-6">
            <i class="pi pi-check text-4xl text-green-600"></i>
          </div>
          <h1 class="text-3xl font-bold mb-4">Order Confirmed!</h1>
          <p class="text-surface-600 mb-8">
            Thank you for your purchase. Your order <strong>#{{ orderNumber }}</strong> is being processed.
          </p>
          <div class="flex flex-col gap-3">
            <Button label="View Order Status" @click="$router.push('/account/orders/' + orderNumber)" />
            <Button label="Continue Shopping" severity="secondary" text @click="$router.push('/products')" />
          </div>
        </div>

        <div v-else class="py-12">
          <div class="w-20 h-20 bg-red-100 rounded-full flex items-center justify-center mx-auto mb-6">
            <i class="pi pi-times text-4xl text-red-600"></i>
          </div>
          <h1 class="text-3xl font-bold mb-4">Payment Failed</h1>
          <p class="text-surface-600 mb-8">We couldn't process your payment. Please try again or use a different method.</p>
          <Button label="Return to Checkout" @click="$router.push('/checkout')" />
        </div>
      </template>
    </Card>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { cartService } from '@/services/cart.service';
import Card from 'primevue/card';
import Button from 'primevue/button';
import ProgressSpinner from 'primevue/progressspinner';

const processing = ref(true);
const success = ref(false);
const orderNumber = ref('');

onMounted(async () => {
  try {
    // 1. Finalize the order on the backend
    // The backend will check the payment status with Stripe/PayPal
    const res = await cartService.complete();
    orderNumber.value = res.data.number;
    success.value = true;
  } catch (err) {
    success.value = false;
    console.error("Order completion failed:", err);
  } finally {
    processing.value = false;
  }
});
</script>
