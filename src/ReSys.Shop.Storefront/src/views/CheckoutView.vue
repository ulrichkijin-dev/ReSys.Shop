<template>
  <HeroSection 
    title="Secure Checkout" 
    subtitle="Please complete your information below to finalize your order."
    :breadcrumbs="[{ label: 'Bag', to: '/cart' }, { label: 'Checkout' }]"
  />
  
  <div class="container mx-auto px-4 py-12">
    <div v-if="loading && !cart" class="text-center py-20">
      <ProgressSpinner />
    </div>

    <div v-else class="grid grid-cols-1 lg:grid-cols-12 gap-12">
      <!-- Main Checkout Flow (Steps) -->
      <div class="lg:col-span-8">
        <Stepper v-model:value="activeStep" linear>
          <StepList>
            <Step v-if="!isAuthenticated" value="0">Identity</Step>
            <Step :value="isAuthenticated ? '0' : '1'">Shipping</Step>
            <Step :value="isAuthenticated ? '1' : '2'">Delivery</Step>
            <Step :value="isAuthenticated ? '2' : '3'">Payment</Step>
          </StepList>

          <StepPanels>
            <!-- 1. Contact Info (Only for guests) -->
            <StepPanel v-if="!isAuthenticated" value="0">
              <template #content="{ activateCallback }">
                <div class="p-8 bg-surface-0 dark:bg-surface-900 rounded-2xl border border-surface-200 dark:border-surface-700 shadow-sm">
                  <h3 class="text-xl font-bold mb-6">Contact Information</h3>
                  <div class="flex flex-col gap-2 mb-8">
                    <label for="email">Email Address</label>
                    <InputText id="email" v-model="guestEmail" placeholder="Enter your email for order updates" required />
                  </div>
                  <div class="flex justify-end">
                    <Button label="Continue" icon="pi pi-arrow-right" iconPos="right" @click="saveEmail(() => activateCallback('1'))" :loading="processing" />
                  </div>
                </div>
              </template>
            </StepPanel>

            <!-- 2. Shipping Address -->
            <StepPanel :value="isAuthenticated ? '0' : '1'">
              <template #content="{ activateCallback }">
                <div class="p-8 bg-surface-0 dark:bg-surface-900 rounded-2xl border border-surface-200 dark:border-surface-700 shadow-sm">
                  <div class="flex justify-between items-center mb-6">
                    <h3 class="text-xl font-bold">Shipping Address</h3>
                    <Button v-if="savedAddresses.length && !isAddingNew" label="New Address" text size="small" @click="isAddingNew = true" />
                  </div>

                  <!-- Saved Addresses Selection -->
                  <div v-if="savedAddresses.length && !isAddingNew" class="grid grid-cols-1 md:grid-cols-2 gap-4 mb-8">
                    <div v-for="addr in savedAddresses" :key="addr.id" 
                      @click="selectedAddress = addr"
                      class="p-4 border-2 rounded-xl cursor-pointer transition-all hover:border-primary"
                      :class="selectedAddress?.id === addr.id ? 'border-primary bg-primary/5' : 'border-surface-200'"
                    >
                      <p class="font-bold">{{ addr.firstName }} {{ addr.lastName }}</p>
                      <p class="text-sm text-surface-500 mt-1">{{ addr.address1 }}, {{ addr.city }}</p>
                      <p class="text-sm text-surface-500">{{ addr.phone }}</p>
                    </div>
                  </div>

                  <!-- New Address Form -->
                  <AddressForm v-else @submit="(data) => saveAddress(data, () => activateCallback(isAuthenticated ? '1' : '2'))">
                    <template #actions>
                      <Button v-if="savedAddresses.length" label="Cancel" severity="secondary" text @click="isAddingNew = false" />
                      <Button type="submit" label="Deliver to this Address" icon="pi pi-check" :loading="processing" />
                    </template>
                  </AddressForm>

                  <div v-if="savedAddresses.length && !isAddingNew" class="flex justify-end mt-6 border-t pt-6">
                    <Button label="Continue" icon="pi pi-arrow-right" iconPos="right" @click="proceedWithSavedAddress(() => activateCallback(isAuthenticated ? '1' : '2'))" :disabled="!selectedAddress" :loading="processing" />
                  </div>
                </div>
              </template>
            </StepPanel>

            <!-- 3. Delivery Method -->
            <StepPanel :value="isAuthenticated ? '1' : '2'">
              <template #content="{ activateCallback }">
                <div class="p-8 bg-surface-0 dark:bg-surface-900 rounded-2xl border border-surface-200 dark:border-surface-700 shadow-sm">
                  <h3 class="text-xl font-bold mb-6">Select Shipping Method</h3>
                  <div class="space-y-4 mb-8">
                    <div v-for="method in shippingMethods" :key="method.id" 
                      @click="selectedShippingId = method.id"
                      class="flex items-center gap-4 p-5 border-2 rounded-xl cursor-pointer transition-all hover:border-primary"
                      :class="selectedShippingId === method.id ? 'border-primary bg-primary/5' : 'border-surface-200 dark:border-surface-800'"
                    >
                      <RadioButton v-model="selectedShippingId" :value="method.id" />
                      <div class="flex-grow">
                        <p class="font-bold text-lg">{{ method.presentation || method.name }}</p>
                        <p class="text-sm text-surface-500">{{ method.description }}</p>
                      </div>
                      <span class="font-bold text-lg text-primary">{{ formatPrice(method.amount || (method.amountCents / 100)) }}</span>
                    </div>
                  </div>
                  <div class="flex justify-between">
                    <Button label="Back" severity="secondary" text @click="activateCallback(isAuthenticated ? '0' : '1')" />
                    <Button label="Continue to Payment" icon="pi pi-arrow-right" iconPos="right" @click="saveShipping(() => activateCallback(isAuthenticated ? '2' : '3'))" :disabled="!selectedShippingId" :loading="processing" />
                  </div>
                </div>
              </template>
            </StepPanel>

            <!-- 4. Payment -->
            <StepPanel :value="isAuthenticated ? '2' : '3'">
              <template #content="{ activateCallback }">
                <div class="p-8 bg-surface-0 dark:bg-surface-900 rounded-2xl border border-surface-200 dark:border-surface-700 shadow-sm">
                  <h3 class="text-xl font-bold mb-6">Payment Method</h3>
                  <div class="space-y-4 mb-8">
                    <div v-for="method in paymentMethods" :key="method.id" 
                      @click="selectedPaymentId = method.id"
                      class="flex items-center gap-4 p-5 border-2 rounded-xl cursor-pointer transition-all hover:border-primary"
                      :class="selectedPaymentId === method.id ? 'border-primary bg-primary/5' : 'border-surface-200 dark:border-surface-800'"
                    >
                      <RadioButton v-model="selectedPaymentId" :value="method.id" />
                      <div class="flex-grow">
                        <p class="font-bold">{{ method.name || method.presentation }}</p>
                        <p class="text-xs text-surface-500">{{ method.description }}</p>
                      </div>
                      <i v-if="method.type === 'Stripe'" class="pi pi-credit-card text-2xl"></i>
                      <i v-else-if="method.type === 'PayPal'" class="pi pi-paypal text-2xl text-blue-600"></i>
                    </div>
                  </div>
                  <div class="flex justify-between">
                    <Button label="Back" severity="secondary" text @click="activateCallback(isAuthenticated ? '1' : '2')" />
                    <Button label="Place Order" icon="pi pi-lock" severity="success" size="large" @click="finalizeOrder" :disabled="!selectedPaymentId" :loading="processing" />
                  </div>
                </div>
              </template>
            </StepPanel>
          </StepPanels>
        </Stepper>
      </div>

      <!-- Sidebar: Order Summary -->
      <div class="lg:col-span-4">
        <div class="sticky top-24 bg-surface-0 dark:bg-surface-900 p-8 rounded-2xl border border-surface-200 dark:border-surface-700 shadow-sm">
          <h2 class="text-2xl font-bold mb-8">Order Summary</h2>
          <OrderSummary :cart="cart" />
          
          <div class="mt-8 p-4 bg-surface-50 dark:bg-surface-800 rounded-xl flex items-start gap-3">
            <i class="pi pi-shield text-primary mt-1"></i>
            <p class="text-xs text-surface-600 dark:text-surface-400">
              Secure Checkout. Your data is protected using industrial-standard encryption.
            </p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useCartStore } from '@/stores/cart';
import { useAuthStore } from '@/stores/auth';
import { useToast } from 'primevue/usetoast';
import { cartService } from '@/services/cart.service';
import { accountService } from '@/services/account.service';
import AddressForm from '@/components/AddressForm.vue';
import OrderSummary from '@/components/OrderSummary.vue';
import HeroSection from '@/components/HeroSection.vue';
import Stepper from 'primevue/stepper';
import StepList from 'primevue/steplist';
import StepPanels from 'primevue/steppanels';
import StepPanel from 'primevue/steppanel';
import StepItem from 'primevue/stepitem';
import Step from 'primevue/step';
import Button from 'primevue/button';
import InputText from 'primevue/inputtext';
import RadioButton from 'primevue/radiobutton';
import ProgressSpinner from 'primevue/progressspinner';

const router = useRouter();
const toast = useToast();
const cartStore = useCartStore();
const authStore = useAuthStore();

const cart = computed(() => cartStore.cart);
const isAuthenticated = computed(() => authStore.isAuthenticated);
const loading = ref(true);
const processing = ref(false);
const activeStep = ref('0');

// Data
const guestEmail = ref('');
const savedAddresses = ref([]);
const isAddingNew = ref(false);
const selectedAddress = ref(null);
const shippingMethods = ref([]);
const selectedShippingId = ref(null);
const paymentMethods = ref([]);
const selectedPaymentId = ref(null);

onMounted(async () => {
  await cartStore.fetchCart();
  if (isAuthenticated.value) {
    const addrResponse = await accountService.getAddresses();
    savedAddresses.value = addrResponse.data.items;
    if (savedAddresses.value.length) {
      selectedAddress.value = savedAddresses.value.find(a => a.isDefault) || savedAddresses.value[0];
    }
  }
  
  syncStepWithCart();
  await fetchMethods();
  loading.value = false;
});

const syncStepWithCart = () => {
  if (!cart.value) return;
  const state = cart.value.state;
  if (state === 'Address') activeStep.value = '0';
  else if (state === 'Delivery') activeStep.value = isAuthenticated.value ? '1' : '2';
  else if (state === 'Payment') activeStep.value = isAuthenticated.value ? '2' : '3';
};

const fetchMethods = async () => {
  const [pmRes, smRes] = await Promise.allSettled([
    cartService.getPaymentMethods(),
    cartService.getShippingMethods()
  ]);
  if (pmRes.status === 'fulfilled') paymentMethods.value = pmRes.value.data;
  if (smRes.status === 'fulfilled') shippingMethods.value = smRes.value.data;
};

const saveEmail = async (next) => {
  processing.value = true;
  try {
    await cartService.updateCheckout({ email: guestEmail.value });
    await cartStore.fetchCart();
    next();
  } finally {
    processing.value = false;
  }
};

const saveAddress = async (data, next) => {
  processing.value = true;
  try {
    await cartService.updateCheckoutAddress({ shipAddress: data });
    await cartService.nextStep();
    await cartStore.fetchCart();
    next();
  } finally {
    processing.value = false;
  }
};

const proceedWithSavedAddress = async (next) => {
  processing.value = true;
  try {
    // Map saved address to order
    await cartService.updateCheckoutAddress({ 
      shipAddress: selectedAddress.value 
    });
    await cartService.nextStep();
    await cartStore.fetchCart();
    next();
  } finally {
    processing.value = false;
  }
};

const saveShipping = async (next) => {
  processing.value = true;
  try {
    await cartService.selectShippingMethod(selectedShippingId.value);
    await cartService.nextStep();
    await cartStore.fetchCart();
    next();
  } finally {
    processing.value = false;
  }
};

const finalizeOrder = async () => {
  processing.value = true;
  try {
    const addPaymentRes = await cartService.addPayment({
      paymentMethodId: selectedPaymentId.value,
      amount: cart.value.total,
      returnUrl: `${window.location.origin}/checkout/complete`,
      cancelUrl: `${window.location.origin}/checkout`
    });

    if (addPaymentRes.data.paymentApprovalUrl) {
      window.location.href = addPaymentRes.data.paymentApprovalUrl;
      return;
    }

    const res = await cartService.complete();
    router.push(`/account/orders/${res.data.number}`);
  } catch (err) {
    toast.add({ severity: 'error', summary: 'Payment Failed', detail: err.response?.data?.message || 'Transaction could not be processed' });
  } finally {
    processing.value = false;
  }
};

const formatPrice = (value) => {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: cart.value?.currency || 'USD' }).format(value);
};
</script>
