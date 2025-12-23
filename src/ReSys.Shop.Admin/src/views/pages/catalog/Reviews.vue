<script setup>
import { onMounted, ref } from 'vue';
import { useReviewStore } from '@/stores';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import Toolbar from 'primevue/toolbar';
import Rating from 'primevue/rating';
import Tag from 'primevue/tag';

const reviewStore = useReviewStore();

onMounted(async () => {
    await reviewStore.fetchPagedReviews();
});

const getStatusSeverity = (status) => {
    switch (status.toLowerCase()) {
        case 'approved':
            return 'success';
        case 'pending':
            return 'warn';
        case 'rejected':
            return 'danger';
        default:
            return 'info';
    }
};

const onRefresh = async () => {
    await reviewStore.fetchPagedReviews();
};
</script>

<template>
    <div class="card">
        <Toolbar class="mb-4">
            <template #start>
                <Button label="Refresh" icon="pi pi-refresh" severity="secondary" @click="onRefresh" :loading="reviewStore.loading" />
            </template>
        </Toolbar>

        <DataTable
            :value="reviewStore.reviews"
            :loading="reviewStore.loading"
            :paginator="true"
            :rows="10"
            dataKey="id"
            responsiveLayout="scroll"
        >
            <Column field="productName" header="Product" sortable></Column>
            <Column field="userName" header="Customer" sortable></Column>
            <Column field="rating" header="Rating" sortable>
                <template #body="slotProps">
                    <Rating :modelValue="slotProps.data.rating" readonly :cancel="false" />
                </template>
            </Column>
            <Column field="title" header="Title"></Column>
            <Column field="status" header="Status" sortable>
                <template #body="slotProps">
                    <Tag :value="slotProps.data.status" :severity="getStatusSeverity(slotProps.data.status)" />
                </template>
            </Column>
            <Column field="createdAt" header="Date" sortable>
                <template #body="slotProps">
                    {{ new Date(slotProps.data.createdAt).toLocaleDateString() }}
                </template>
            </Column>
            <Column header="Actions">
                <template #body="slotProps">
                    <Button icon="pi pi-check" v-if="slotProps.data.status.toLowerCase() === 'pending'" outlined rounded severity="success" class="mr-2" />
                    <Button icon="pi pi-times" v-if="slotProps.data.status.toLowerCase() === 'pending'" outlined rounded severity="danger" class="mr-2" />
                    <Button icon="pi pi-eye" outlined rounded />
                </template>
            </Column>
        </DataTable>
    </div>
</template>
