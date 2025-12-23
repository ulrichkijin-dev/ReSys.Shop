<script setup>
import { ref, onMounted } from 'vue';
import { useRoleStore } from '@/stores';
import { useRouter } from 'vue-router';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import Toolbar from 'primevue/toolbar';
import Tag from 'primevue/tag';

const store = useRoleStore();
const router = useRouter();

onMounted(async () => {
    await store.fetchPagedRoles();
});

const viewRole = (id) => {
    router.push({ name: 'admin-role-detail', params: { id } });
};

const createRole = () => {
    router.push({ name: 'admin-role-detail', params: { id: 'new' } });
};
</script>

<template>
    <div class="card">
        <Toolbar class="mb-4">
            <template #start>
                <Button label="New Role" icon="pi pi-plus" class="mr-2" @click="createRole" />
                <Button label="Refresh" icon="pi pi-refresh" severity="secondary" @click="store.fetchPagedRoles" :loading="store.loading" />
            </template>
        </Toolbar>

        <DataTable :value="store.roles" :loading="store.loading" paginator :rows="10">
            <Column field="name" header="Name" sortable></Column>
            <Column field="displayName" header="Display Name" sortable></Column>
            <Column field="userCount" header="Users" sortable></Column>
            <Column field="permissionCount" header="Permissions" sortable></Column>
            <Column field="isSystemRole" header="System">
                <template #body="slotProps">
                    <i class="pi" :class="{ 'pi-check-circle text-blue-500': slotProps.data.isSystemRole, 'pi-circle text-muted-color': !slotProps.data.isSystemRole }"></i>
                </template>
            </Column>
            <Column header="Actions">
                <template #body="slotProps">
                    <Button icon="pi pi-pencil" outlined rounded class="mr-2" @click="viewRole(slotProps.data.id)" />
                    <Button icon="pi pi-trash" outlined rounded severity="danger" v-if="!slotProps.data.isSystemRole" />
                </template>
            </Column>
        </DataTable>
    </div>
</template>
