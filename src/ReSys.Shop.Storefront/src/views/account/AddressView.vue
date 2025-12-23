<template>
  <div>
    <div class="flex justify-between items-center mb-6">
      <h2 class="text-2xl font-bold">Manage Addresses</h2>
      <Button label="Add New Address" icon="pi pi-plus" @click="showAddressDialog = true" />
    </div>

    <div v-if="loading" class="grid grid-cols-1 md:grid-cols-2 gap-4">
      <Skeleton v-for="i in 2" :key="i" height="150px" />
    </div>

    <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-6">
      <div v-for="address in addresses" :key="address.id" class="p-6 border border-surface-200 dark:border-surface-700 rounded-xl relative hover:border-primary transition-colors">
        <div v-if="address.isDefault" class="absolute top-4 right-4 bg-primary/10 text-primary text-xs font-bold px-2 py-1 rounded">
          DEFAULT
        </div>
        <h3 class="font-bold text-lg mb-2">{{ address.firstName }} {{ address.lastName }}</h3>
        <div class="text-surface-600 dark:text-surface-400 space-y-1 text-sm mb-6">
          <p>{{ address.address1 }}</p>
          <p v-if="address.address2">{{ address.address2 }}</p>
          <p>{{ address.city }}, {{ address.stateName }} {{ address.zipcode }}</p>
          <p>{{ address.phone }}</p>
        </div>
        <div class="flex gap-2">
          <Button icon="pi pi-pencil" text @click="editAddress(address)" />
          <Button icon="pi pi-trash" text severity="danger" @click="confirmDelete(address.id)" />
        </div>
      </div>
    </div>

    <!-- Real Address Dialog -->
    <Dialog v-model:visible="showAddressDialog" :header="selectedAddress ? 'Edit Address' : 'New Address'" :style="{width: '600px'}" modal>
       <AddressForm :initialData="selectedAddress || {}" @submit="handleAddressSubmit">
          <template #actions>
             <Button label="Cancel" severity="secondary" text @click="showAddressDialog = false" />
             <Button type="submit" label="Save Address" :loading="saving" />
          </template>
       </AddressForm>
    </Dialog>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { accountService } from '@/services/account.service';
import AddressForm from '@/components/AddressForm.vue';
import Button from 'primevue/button';
import Skeleton from 'primevue/skeleton';
import Dialog from 'primevue/dialog';

const addresses = ref([]);
const loading = ref(true);
const saving = ref(false);
const showAddressDialog = ref(false);
const selectedAddress = ref(null);

onMounted(async () => {
  fetchAddresses();
});

const fetchAddresses = async () => {
  loading.value = true;
  try {
    const response = await accountService.getAddresses();
    addresses.value = response.data.items;
  } finally {
    loading.value = false;
  }
};

const editAddress = (address) => {
  selectedAddress.value = address;
  showAddressDialog.value = true;
};

const handleAddressSubmit = async (formData) => {
  saving.value = true;
  try {
    if (selectedAddress.value) {
      await accountService.updateAddress(selectedAddress.value.id, formData);
    } else {
      await accountService.createAddress(formData);
    }
    showAddressDialog.value = false;
    await fetchAddresses();
  } finally {
    saving.value = false;
  }
};

const confirmDelete = async (id) => {
  if (confirm('Are you sure?')) {
    await accountService.deleteAddress(id);
    fetchAddresses();
  }
};
</script>
