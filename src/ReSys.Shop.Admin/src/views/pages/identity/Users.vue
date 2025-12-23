<script setup>
import { ref, onMounted } from 'vue';
import { useUserStore } from '@/stores';
import { useRouter } from 'vue-router';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import Toolbar from 'primevue/toolbar';
import Tag from 'primevue/tag';
import InputText from 'primevue/inputtext';

const store = useUserStore();
const router = useRouter();
const filters = ref({ global: { value: null, matchMode: 'contains' } });

onMounted(async () => {
    await store.fetchPagedUsers();
});

const viewUser = (id) => {
    router.push({ name: 'admin-user-detail', params: { id } });
};

const createUser = () => {
    router.push({ name: 'admin-user-detail', params: { id: 'new' } });
};
</script>

<template>
    <div class="card">
        <Toolbar class="mb-4">
            <template #start>
                <Button label="New User" icon="pi pi-plus" class="mr-2" @click="createUser" />
                <Button label="Refresh" icon="pi pi-refresh" severity="secondary" @click="store.fetchPagedUsers" :loading="store.loading" />
            </template>
            <template #end>
                <IconField iconPosition="left">
                    <InputIcon>
                        <i class="pi pi-search" />
                    </InputIcon>
                    <InputText v-model="filters['global'].value" placeholder="Search Email/User..." />
                </IconField>
            </template>
        </Toolbar>

        <DataTable :value="store.users" :loading="store.loading" paginator :rows="10" v-model:filters="filters">
            <Column field="userName" header="Username" sortable></Column>
            <Column field="email" header="Email" sortable></Column>
            <Column field="fullName" header="Full Name"></Column>
            <Column field="emailConfirmed" header="Status">
                <template #body="slotProps">
                    <Tag :value="slotProps.data.emailConfirmed ? 'Verified' : 'Unverified'" :severity="slotProps.data.emailConfirmed ? 'success' : 'warn'" />
                </template>
            </Column>
            <Column field="createdAt" header="Registered" sortable>
                <template #body="slotProps">
                    {{ new Date(slotProps.data.createdAt).toLocaleDateString() }}
                </template>
            </Column>
            <Column header="Actions">
                <template #body="slotProps">
                    <Button icon="pi pi-pencil" outlined rounded class="mr-2" @click="viewUser(slotProps.data.id)" />
                    <Button icon="pi pi-trash" outlined rounded severity="danger" />
                </template>
            </Column>
        </DataTable>
    </div>
</template>
