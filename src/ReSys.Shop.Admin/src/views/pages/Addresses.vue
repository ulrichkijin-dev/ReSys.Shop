<script setup>
import { useAddressStore } from '@/stores';
import { onMounted } from 'vue';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';

const addressStore = useAddressStore();

onMounted(async () => {
    await addressStore.fetchPagedAddresses();
});
</script>

<template>
    <div class="card">
        <div class="font-semibold text-xl mb-4">My Addresses</div>
        <DataTable :value="addressStore.addresses" :loading="addressStore.loading" responsiveLayout="scroll">
            <Column field="label" header="Label"></Column>
            <Column field="firstName" header="First Name"></Column>
            <Column field="lastName" header="Last Name"></Column>
            <Column field="address1" header="Address"></Column>
            <Column field="city" header="City"></Column>
            <Column field="stateName" header="State"></Column>
            <Column field="zipcode" header="Zip Code"></Column>
            <Column field="type" header="Type">
                <template #body="slotProps">
                    {{ slotProps.data.type === 0 ? 'Shipping' : 'Billing' }}
                </template>
            </Column>
            <Column header="Default">
                <template #body="slotProps">
                    <i class="pi" :class="{ 'pi-check-circle text-green-500': slotProps.data.isDefault, 'pi-times-circle text-red-500': !slotProps.data.isDefault }"></i>
                </template>
            </Column>
        </DataTable>
    </div>
</template>
