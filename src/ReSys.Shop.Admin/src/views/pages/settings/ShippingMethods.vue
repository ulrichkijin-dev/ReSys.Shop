<script setup>
import { ref, onMounted } from 'vue';
import { useShippingMethodStore } from '@/stores';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import Toolbar from 'primevue/toolbar';
import Dialog from 'primevue/dialog';
import InputText from 'primevue/inputtext';
import InputNumber from 'primevue/inputnumber';
import Select from 'primevue/select';
import ToggleSwitch from 'primevue/toggleswitch';
import { useToast } from 'primevue/usetoast';
import MetadataEditor from '@/components/MetadataEditor.vue';

const store = useShippingMethodStore();
const toast = useToast();

const showDialog = ref(false);
const submitted = ref(false);
const method = ref({
    name: '',
    presentation: '',
    type: 'FlatRate',
    baseCost: 0,
    active: true,
    estimatedDaysMin: null,
    estimatedDaysMax: null,
    displayOn: 'Both',
    publicMetadata: {},
    privateMetadata: {}
});

const typeOptions = ['FlatRate', 'PerItem', 'WeightBased'];

onMounted(async () => {
    await store.fetchPagedShippingMethods();
});

function openNew() {
    method.value = { 
        type: 'FlatRate', 
        baseCost: 0, 
        active: true, 
        displayOn: 'Both',
        publicMetadata: {},
        privateMetadata: {}
    };
    submitted.value = false;
    showDialog.value = true;
}

function editMethod(data) {
    method.value = { ...data };
    showDialog.value = true;
}

async function saveMethod() {
    submitted.value = true;
    if (method.value.name && method.value.presentation) {
        let success = false;
        if (method.value.id) {
            success = await store.updateShippingMethod(method.value.id, method.value);
        } else {
            success = await store.createShippingMethod(method.value);
        }

        if (success) {
            toast.add({ severity: 'success', summary: 'Success', detail: 'Shipping method saved', life: 3000 });
            showDialog.value = false;
        }
    }
}
</script>

<template>
    <div class="card">
        <Toolbar class="mb-4">
            <template #start>
                <Button label="New Method" icon="pi pi-plus" class="mr-2" @click="openNew" />
                <Button label="Refresh" icon="pi pi-refresh" severity="secondary" @click="store.fetchPagedShippingMethods" :loading="store.loading" />
            </template>
        </Toolbar>

        <DataTable :value="store.shippingMethods" :loading="store.loading" paginator :rows="10">
            <Column field="presentation" header="Name" sortable></Column>
            <Column field="type" header="Type" sortable></Column>
            <Column field="baseCost" header="Base Cost" sortable>
                <template #body="slotProps">
                    {{ new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(slotProps.data.baseCost) }}
                </template>
            </Column>
            <Column header="Delivery">
                <template #body="slotProps">
                    {{ slotProps.data.estimatedDaysMin }}-{{ slotProps.data.estimatedDaysMax }} Days
                </template>
            </Column>
            <Column field="active" header="Status">
                <template #body="slotProps">
                    <i class="pi" :class="{ 'pi-check-circle text-green-500': slotProps.data.active, 'pi-times-circle text-red-500': !slotProps.data.active }"></i>
                </template>
            </Column>
            <Column header="Actions">
                <template #body="slotProps">
                    <Button icon="pi pi-pencil" outlined rounded class="mr-2" @click="editMethod(slotProps.data)" />
                    <Button icon="pi pi-trash" outlined rounded severity="danger" />
                </template>
            </Column>
        </DataTable>

        <Dialog v-model:visible="showDialog" :header="method.id ? 'Edit Shipping Method' : 'New Shipping Method'" modal class="p-fluid w-[35rem]">
            <div class="flex flex-col gap-4">
                <div class="flex flex-col gap-2">
                    <label class="font-bold">Internal Name</label>
                    <InputText v-model.trim="method.name" required :class="{ 'p-invalid': submitted && !method.name }" />
                </div>
                <div class="flex flex-col gap-2">
                    <label class="font-bold">Display Name</label>
                    <InputText v-model.trim="method.presentation" required :class="{ 'p-invalid': submitted && !method.presentation }" />
                </div>
                <div class="grid grid-cols-2 gap-4">
                    <div class="flex flex-col gap-2">
                        <label class="font-bold">Type</label>
                        <Select v-model="method.type" :options="typeOptions" />
                    </div>
                    <div class="flex flex-col gap-2">
                        <label class="font-bold">Base Cost</label>
                        <InputNumber v-model="method.baseCost" mode="decimal" :minFractionDigits="2" />
                    </div>
                </div>
                <div class="grid grid-cols-2 gap-4">
                    <div class="flex flex-col gap-2">
                        <label class="font-bold">Est. Days Min</label>
                        <InputNumber v-model="method.estimatedDaysMin" />
                    </div>
                    <div class="flex flex-col gap-2">
                        <label class="font-bold">Est. Days Max</label>
                        <InputNumber v-model="method.estimatedDaysMax" />
                    </div>
                </div>
                <div class="flex items-center gap-2 mt-2">
                    <ToggleSwitch v-model="method.active" />
                    <label>Active and Available</label>
                </div>

                <div class="flex flex-col gap-6 mt-4">
                    <MetadataEditor 
                        v-model="method.publicMetadata" 
                        title="Public Metadata" 
                        helpText="Visible to storefront APIs (e.g. tracking_url_template, handling_fee)." 
                    />
                    
                    <MetadataEditor 
                        v-model="method.privateMetadata" 
                        title="Private Metadata" 
                        helpText="Internal carrier details (e.g. account_number, api_secret)." 
                    />
                </div>
            </div>
            <template #footer>
                <Button label="Cancel" text @click="showDialog = false" />
                <Button label="Save" icon="pi pi-check" @click="saveMethod" :loading="store.loading" />
            </template>
        </Dialog>
    </div>
</template>
