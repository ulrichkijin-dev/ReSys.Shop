<script setup>
import { ref, onMounted } from 'vue';
import { useProductStore } from '@/stores';
import { useRouter } from 'vue-router';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import InputText from 'primevue/inputtext';
import Tag from 'primevue/tag';
import Toolbar from 'primevue/toolbar';

const productStore = useProductStore();
const router = useRouter();

const filters = ref({
    global: { value: null, matchMode: 'contains' }
});

onMounted(async () => {
    await productStore.fetchPagedProducts();
});

const getStatusSeverity = (status) => {
    switch (status.toLowerCase()) {
        case 'active':
            return 'success';
        case 'draft':
            return 'warn';
        case 'archived':
            return 'danger';
        default:
            return 'info';
    }
};

const viewProduct = (id) => {
    router.push({ name: 'admin-product-detail', params: { id } });
};

const createProduct = () => {
    router.push({ name: 'admin-product-detail', params: { id: 'new' } });
};

const onRefresh = async () => {
    await productStore.fetchPagedProducts();
};
</script>

<template>
    <div class="card">
        <Toolbar class="mb-4">
            <template #start>
                <Button label="New Product" icon="pi pi-plus" class="mr-2" @click="createProduct" />
                <Button label="Refresh" icon="pi pi-refresh" severity="secondary" @click="onRefresh" :loading="productStore.loading" />
            </template>
            <template #end>
                <div class="flex flex-wrap gap-2 items-center justify-between">
                    <IconField iconPosition="left">
                        <InputIcon>
                            <i class="pi pi-search" />
                        </InputIcon>
                        <InputText v-model="filters['global'].value" placeholder="Search..." />
                    </IconField>
                </div>
            </template>
        </Toolbar>

        <DataTable
            :value="productStore.products"
            :loading="productStore.loading"
            :paginator="true"
            :rows="10"
            v-model:filters="filters"
            dataKey="id"
            responsiveLayout="scroll"
            removableSort
        >
            <template #empty> No products found. </template>
            <template #loading> Loading products data. Please wait. </template>

            <Column field="name" header="Name" sortable style="min-width: 16rem">
                <template #body="slotProps">
                    <div class="flex flex-col">
                        <span class="font-bold">{{ slotProps.data.name }}</span>
                        <small class="text-surface-500">{{ slotProps.data.slug }}</small>
                    </div>
                </template>
            </Column>
            <Column field="status" header="Status" sortable style="min-width: 8rem">
                <template #body="slotProps">
                    <Tag :value="slotProps.data.status" :severity="getStatusSeverity(slotProps.data.status)" />
                </template>
            </Column>
            <Column field="variantCount" header="Variants" sortable style="min-width: 8rem"></Column>
            <Column field="imageCount" header="Images" sortable style="min-width: 8rem"></Column>
            <Column field="available" header="Available" sortable style="min-width: 8rem">
                <template #body="slotProps">
                    <i class="pi" :class="{ 'pi-check-circle text-green-500': slotProps.data.available, 'pi-times-circle text-red-500': !slotProps.data.available }"></i>
                </template>
            </Column>
            <Column field="createdAt" header="Created At" sortable style="min-width: 12rem">
                <template #body="slotProps">
                    {{ new Date(slotProps.data.createdAt).toLocaleDateString() }}
                </template>
            </Column>
            <Column header="Actions" :exportable="false" style="min-width: 12rem">
                <template #body="slotProps">
                    <Button icon="pi pi-pencil" outlined rounded class="mr-2" @click="viewProduct(slotProps.data.id)" />
                    <Button icon="pi pi-trash" outlined rounded severity="danger" @click="confirmDeleteProduct(slotProps.data)" />
                </template>
            </Column>
        </DataTable>
    </div>
</template>
