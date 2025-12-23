<script setup>
import { onMounted } from 'vue';
import { useOptionTypeStore } from '@/stores';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import Toolbar from 'primevue/toolbar';

const optionTypeStore = useOptionTypeStore();

onMounted(async () => {
    await optionTypeStore.fetchPagedOptionTypes();
});

const onRefresh = async () => {
    await optionTypeStore.fetchPagedOptionTypes();
};
</script>

<template>
    <div class="card">
        <Toolbar class="mb-4">
            <template #start>
                <Button label="New Option Type" icon="pi pi-plus" class="mr-2" />
                <Button label="Refresh" icon="pi pi-refresh" severity="secondary" @click="onRefresh" :loading="optionTypeStore.loading" />
            </template>
        </Toolbar>

        <DataTable
            :value="optionTypeStore.optionTypes"
            :loading="optionTypeStore.loading"
            :paginator="true"
            :rows="10"
            dataKey="id"
            responsiveLayout="scroll"
        >
            <Column field="name" header="Name" sortable></Column>
            <Column field="presentation" header="Presentation" sortable></Column>
            <Column field="filterable" header="Filterable" sortable>
                <template #body="slotProps">
                    <i class="pi" :class="{ 'pi-check-circle text-green-500': slotProps.data.filterable, 'pi-times-circle text-red-500': !slotProps.data.filterable }"></i>
                </template>
            </Column>
            <Column field="productCount" header="Products" sortable></Column>
            <Column field="position" header="Position" sortable></Column>
            <Column header="Actions">
                <template #body="slotProps">
                    <Button icon="pi pi-pencil" outlined rounded class="mr-2" />
                    <Button icon="pi pi-trash" outlined rounded severity="danger" />
                </template>
            </Column>
        </DataTable>
    </div>
</template>
