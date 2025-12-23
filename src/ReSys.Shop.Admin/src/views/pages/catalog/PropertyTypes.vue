<script setup>
import { onMounted } from 'vue';
import { usePropertyTypeStore } from '@/stores';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import Toolbar from 'primevue/toolbar';
import Tag from 'primevue/tag';

const propertyTypeStore = usePropertyTypeStore();

onMounted(async () => {
    await propertyTypeStore.fetchPagedPropertyTypes();
});

const onRefresh = async () => {
    await propertyTypeStore.fetchPagedPropertyTypes();
};
</script>

<template>
    <div class="card">
        <Toolbar class="mb-4">
            <template #start>
                <Button label="New Property Type" icon="pi pi-plus" class="mr-2" />
                <Button label="Refresh" icon="pi pi-refresh" severity="secondary" @click="onRefresh" :loading="propertyTypeStore.loading" />
            </template>
        </Toolbar>

        <DataTable
            :value="propertyTypeStore.propertyTypes"
            :loading="propertyTypeStore.loading"
            :paginator="true"
            :rows="10"
            dataKey="id"
            responsiveLayout="scroll"
        >
            <Column field="name" header="Name" sortable></Column>
            <Column field="presentation" header="Presentation" sortable></Column>
            <Column field="kind" header="Kind" sortable>
                <template #body="slotProps">
                    <Tag :value="slotProps.data.kind" severity="info" />
                </template>
            </Column>
            <Column field="displayOn" header="Display On" sortable></Column>
            <Column field="filterable" header="Filterable" sortable>
                <template #body="slotProps">
                    <i class="pi" :class="{ 'pi-check-circle text-green-500': slotProps.data.filterable, 'pi-times-circle text-red-500': !slotProps.data.filterable }"></i>
                </template>
            </Column>
            <Column field="productPropertyCount" header="Products" sortable></Column>
            <Column header="Actions">
                <template #body="slotProps">
                    <Button icon="pi pi-pencil" outlined rounded class="mr-2" />
                    <Button icon="pi pi-trash" outlined rounded severity="danger" />
                </template>
            </Column>
        </DataTable>
    </div>
</template>
