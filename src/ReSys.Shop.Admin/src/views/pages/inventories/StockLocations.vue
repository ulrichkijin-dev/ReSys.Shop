<script setup>
import { ref, onMounted } from 'vue';
import { useStockLocationStore } from '@/stores';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import Toolbar from 'primevue/toolbar';
import Dialog from 'primevue/dialog';
import InputText from 'primevue/inputtext';
import ToggleSwitch from 'primevue/toggleswitch';
import { useToast } from 'primevue/usetoast';

const store = useStockLocationStore();
const toast = useToast();

const showDialog = ref(false);
const submitted = ref(false);
const location = ref({
    name: '',
    presentation: '',
    active: true,
    default: false,
    address1: '',
    city: '',
    zipCode: '',
    countryId: null
});

onMounted(async () => {
    await store.fetchPagedStockLocations();
});

function openNew() {
    location.value = { active: true, default: false };
    submitted.value = false;
    showDialog.value = true;
}

function editLocation(data) {
    location.value = { ...data };
    showDialog.value = true;
}

async function saveLocation() {
    submitted.value = true;
    if (location.value.name && location.value.presentation) {
        let success = false;
        if (location.value.id) {
            success = await store.updateStockLocation(location.value.id, location.value);
        } else {
            success = await store.createStockLocation(location.value);
        }

        if (success) {
            toast.add({ severity: 'success', summary: 'Successful', detail: 'Location Saved', life: 3000 });
            showDialog.value = false;
        }
    }
}
</script>

<template>
    <div class="card">
        <Toolbar class="mb-4">
            <template #start>
                <Button label="New Location" icon="pi pi-plus" class="mr-2" @click="openNew" />
                <Button label="Refresh" icon="pi pi-refresh" severity="secondary" @click="store.fetchPagedStockLocations" :loading="store.loading" />
            </template>
        </Toolbar>

        <DataTable :value="store.stockLocations" :loading="store.loading" paginator :rows="10">
            <Column field="name" header="Name" sortable></Column>
            <Column field="presentation" header="Display Name" sortable></Column>
            <Column field="city" header="City" sortable></Column>
            <Column field="active" header="Active">
                <template #body="slotProps">
                    <i class="pi" :class="{ 'pi-check-circle text-green-500': slotProps.data.active, 'pi-times-circle text-red-500': !slotProps.data.active }"></i>
                </template>
            </Column>
            <Column field="default" header="Default">
                <template #body="slotProps">
                    <i class="pi pi-star-fill text-yellow-500" v-if="slotProps.data.default"></i>
                </template>
            </Column>
            <Column field="stockItemCount" header="Stock Items"></Column>
            <Column header="Actions">
                <template #body="slotProps">
                    <Button icon="pi pi-pencil" outlined rounded class="mr-2" @click="editLocation(slotProps.data)" />
                    <Button icon="pi pi-trash" outlined rounded severity="danger" v-if="!slotProps.data.default" />
                </template>
            </Column>
        </DataTable>

        <Dialog v-model:visible="showDialog" :header="location.id ? 'Edit Location' : 'New Location'" modal class="p-fluid w-[40rem]">
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div class="flex flex-col gap-2">
                    <label for="name">Internal Name</label>
                    <InputText id="name" v-model.trim="location.name" required autofocus :class="{ 'p-invalid': submitted && !location.name }" />
                </div>
                <div class="flex flex-col gap-2">
                    <label for="presentation">Display Name</label>
                    <InputText id="presentation" v-model.trim="location.presentation" required :class="{ 'p-invalid': submitted && !location.presentation }" />
                </div>
                <div class="flex flex-col gap-2 md:col-span-2">
                    <label for="address1">Address</label>
                    <InputText id="address1" v-model="location.address1" />
                </div>
                <div class="flex flex-col gap-2">
                    <label for="city">City</label>
                    <InputText id="city" v-model="location.city" />
                </div>
                <div class="flex flex-col gap-2">
                    <label for="zipCode">Zip Code</label>
                    <InputText id="zipCode" v-model="location.zipCode" />
                </div>
                <div class="flex items-center gap-4 mt-4">
                    <div class="flex items-center gap-2">
                        <ToggleSwitch v-model="location.active" />
                        <label>Active</label>
                    </div>
                    <div class="flex items-center gap-2">
                        <ToggleSwitch v-model="location.default" />
                        <label>Default</label>
                    </div>
                </div>
            </div>
            <template #footer>
                <Button label="Cancel" icon="pi pi-times" text @click="showDialog = false" />
                <Button label="Save" icon="pi pi-check" @click="saveLocation" :loading="store.loading" />
            </template>
        </Dialog>
    </div>
</template>
