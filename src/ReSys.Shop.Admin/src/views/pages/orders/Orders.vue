<script setup>
import { ref, onMounted } from 'vue';
import { useOrderStore } from '@/stores';
import { useRouter } from 'vue-router';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import Toolbar from 'primevue/toolbar';
import Tag from 'primevue/tag';
import InputText from 'primevue/inputtext';
import Select from 'primevue/select';

const store = useOrderStore();
const router = useRouter();

const filters = ref({
    global: { value: null, matchMode: 'contains' }
});

const statusOptions = ['Cart', 'Address', 'Delivery', 'Payment', 'Confirm', 'Complete', 'Canceled'];

onMounted(async () => {
    await store.fetchPagedOrders();
});

const getStatusSeverity = (status) => {
    switch (status) {
        case 'Complete': return 'success';
        case 'Confirm': return 'info';
        case 'Payment': return 'warn';
        case 'Canceled': return 'danger';
        default: return 'secondary';
    }
};

const formatCurrency = (value, currency) => {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: currency || 'USD' }).format(value);
};

const viewOrder = (id) => {
    router.push({ name: 'admin-order-detail', params: { id } });
};
</script>

<template>
    <div class="card">
        <Toolbar class="mb-4">
            <template #start>
                <Button label="Refresh" icon="pi pi-refresh" severity="secondary" @click="store.fetchPagedOrders" :loading="store.loading" />
            </template>
            <template #end>
                <IconField iconPosition="left">
                    <InputIcon>
                        <i class="pi pi-search" />
                    </InputIcon>
                    <InputText v-model="filters['global'].value" placeholder="Search Number/Email..." />
                </IconField>
            </template>
        </Toolbar>

        <DataTable :value="store.orders" :loading="store.loading" paginator :rows="10" v-model:filters="filters" dataKey="id">
            <Column field="number" header="Number" sortable>
                <template #body="slotProps">
                    <span class="font-bold">#{{ slotProps.data.number }}</span>
                </template>
            </Column>
            <Column field="createdAt" header="Date" sortable>
                <template #body="slotProps">
                    {{ new Date(slotProps.data.createdAt).toLocaleString() }}
                </template>
            </Column>
            <Column field="userName" header="Customer" sortable></Column>
            <Column field="email" header="Email"></Column>
            <Column field="total" header="Total" sortable>
                <template #body="slotProps">
                    {{ formatCurrency(slotProps.data.total, slotProps.data.currency) }}
                </template>
            </Column>
            <Column field="state" header="Status" sortable>
                <template #body="slotProps">
                    <Tag :value="slotProps.data.state" :severity="getStatusSeverity(slotProps.data.state)" />
                </template>
            </Column>
            <Column header="Actions">
                <template #body="slotProps">
                    <Button icon="pi pi-search" outlined rounded @click="viewOrder(slotProps.data.id)" />
                </template>
            </Column>
        </DataTable>
    </div>
</template>
