<script setup>
import { ref, onMounted } from 'vue';
import { usePermissionStore } from '@/stores';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import Toolbar from 'primevue/toolbar';
import InputText from 'primevue/inputtext';

const store = usePermissionStore();
const filters = ref({ global: { value: null, matchMode: 'contains' } });

onMounted(async () => {
    await store.fetchPagedPermissions();
});
</script>

<template>
    <div class="card">
        <Toolbar class="mb-4">
            <template #start>
                <Button label="Refresh" icon="pi pi-refresh" severity="secondary" @click="store.fetchPagedPermissions" :loading="store.loading" />
            </template>
            <template #end>
                <IconField iconPosition="left">
                    <InputIcon>
                        <i class="pi pi-search" />
                    </InputIcon>
                    <InputText v-model="filters['global'].value" placeholder="Search Permissions..." />
                </IconField>
            </template>
        </Toolbar>

        <DataTable :value="store.permissions" :loading="store.loading" paginator :rows="20" v-model:filters="filters">
            <Column field="name" header="Name" sortable></Column>
            <Column field="displayName" header="Display Name"></Column>
            <Column field="category" header="Category" sortable></Column>
            <Column field="value" header="Value">
                <template #body="slotProps">
                    <code class="text-primary">{{ slotProps.data.value }}</code>
                </template>
            </Column>
        </DataTable>
    </div>
</template>
