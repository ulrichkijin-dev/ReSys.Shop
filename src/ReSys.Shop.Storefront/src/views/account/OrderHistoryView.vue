<template>
  <HeroSection 
    title="Order History" 
    subtitle="View and track all your past purchases."
    :breadcrumbs="[{ label: 'Account', to: '/account' }, { label: 'Orders' }]"
  />
  
  <div class="container mx-auto px-4">
    <DataTable :value="orders" :loading="loading" responsiveLayout="scroll">
      <Column field="number" header="Order #"></Column>
      <Column field="createdAt" header="Date">
        <template #body="slotProps">
          {{ new Date(slotProps.data.createdAt).toLocaleDateString() }}
        </template>
      </Column>
      <Column field="state" header="Status">
        <template #body="slotProps">
          <Tag :value="slotProps.data.state" :severity="getStatusSeverity(slotProps.data.state)" />
        </template>
      </Column>
      <Column field="total" header="Total">
        <template #body="slotProps">
          {{ formatPrice(slotProps.data.total, slotProps.data.currency) }}
        </template>
      </Column>
      <Column header="Actions">
        <template #body="slotProps">
          <Button icon="pi pi-eye" text @click="viewOrder(slotProps.data.number)" />
        </template>
      </Column>
    </DataTable>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { accountService } from '@/services/account.service';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Tag from 'primevue/tag';
import Button from 'primevue/button';
import HeroSection from '@/components/HeroSection.vue';

const orders = ref([]);
const loading = ref(true);

onMounted(async () => {
  try {
    const response = await accountService.getOrders();
    orders.value = response.data.items;
  } finally {
    loading.value = false;
  }
});

const getStatusSeverity = (status) => {
  switch (status.toLowerCase()) {
    case 'complete': return 'success';
    case 'pending': return 'warning';
    case 'canceled': return 'danger';
    default: return 'info';
  }
};

const formatPrice = (value, currency) => {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: currency || 'USD' }).format(value);
};

const viewOrder = (number) => {
  // Navigate to order detail
};
</script>
