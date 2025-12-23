<script setup>
import { ref, onMounted } from 'vue';
import { usePromotionStore } from '@/stores';
import { useRouter } from 'vue-router';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import Toolbar from 'primevue/toolbar';
import Tag from 'primevue/tag';
import InputText from 'primevue/inputtext';

const store = usePromotionStore();
const router = useRouter();
const filters = ref({ global: { value: null, matchMode: 'contains' } });

onMounted(async () => {
    await store.fetchPagedPromotions();
});

const getStatusSeverity = (promo) => {
    if (promo.isExpired) return 'danger';
    return promo.active ? 'success' : 'warn';
};

const getStatusLabel = (promo) => {
    if (promo.isExpired) return 'Expired';
    return promo.active ? 'Active' : 'Inactive';
};

const viewPromotion = (id) => {
    router.push({ name: 'admin-promotion-detail', params: { id } });
};

const createPromotion = () => {
    router.push({ name: 'admin-promotion-detail', params: { id: 'new' } });
};
</script>

<template>
    <div class="card">
        <Toolbar class="mb-4">
            <template #start>
                <Button label="New Promotion" icon="pi pi-plus" class="mr-2" @click="createPromotion" />
                <Button label="Refresh" icon="pi pi-refresh" severity="secondary" @click="store.fetchPagedPromotions" :loading="store.loading" />
            </template>
            <template #end>
                <IconField iconPosition="left">
                    <InputIcon>
                        <i class="pi pi-search" />
                    </InputIcon>
                    <InputText v-model="filters['global'].value" placeholder="Search..." />
                </IconField>
            </template>
        </Toolbar>

        <DataTable :value="store.promotions" :loading="store.loading" paginator :rows="10" v-model:filters="filters">
            <Column field="name" header="Name" sortable></Column>
            <Column field="promotionCode" header="Code" sortable>
                <template #body="slotProps">
                    <code class="p-1 bg-surface-100 dark:bg-surface-800 rounded">{{ slotProps.data.promotionCode || 'AUTO' }}</code>
                </template>
            </Column>
            <Column field="type" header="Type" sortable></Column>
            <Column field="usageCount" header="Usage">
                <template #body="slotProps">
                    {{ slotProps.data.usageCount }} / {{ slotProps.data.usageLimit || '∞' }}
                </template>
            </Column>
            <Column field="status" header="Status">
                <template #body="slotProps">
                    <Tag :value="getStatusLabel(slotProps.data)" :severity="getStatusSeverity(slotProps.data)" />
                </template>
            </Column>
            <Column field="expiresAt" header="Expiry">
                <template #body="slotProps">
                    {{ slotProps.data.expiresAt ? new Date(slotProps.data.expiresAt).toLocaleDateString() : 'Never' }}
                </template>
            </Column>
            <Column header="Actions">
                <template #body="slotProps">
                    <Button icon="pi pi-pencil" outlined rounded class="mr-2" @click="viewPromotion(slotProps.data.id)" />
                    <Button icon="pi pi-trash" outlined rounded severity="danger" />
                </template>
            </Column>
        </DataTable>
    </div>
</template>
