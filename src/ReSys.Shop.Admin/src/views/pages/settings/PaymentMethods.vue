<script setup>
import { ref, onMounted } from 'vue';
import { usePaymentMethodStore } from '@/stores';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import Toolbar from 'primevue/toolbar';
import Dialog from 'primevue/dialog';
import InputText from 'primevue/inputtext';
import Textarea from 'primevue/textarea';
import Select from 'primevue/select';
import ToggleSwitch from 'primevue/toggleswitch';
import { useToast } from 'primevue/usetoast';
import MetadataEditor from '@/components/MetadataEditor.vue';

const store = usePaymentMethodStore();
const toast = useToast();

const showDialog = ref(false);
const submitted = ref(false);
const method = ref({
    name: '',
    presentation: '',
    description: '',
    type: 'CreditCard',
    active: true,
    autoCapture: false,
    displayOn: 'Both',
    publicMetadata: {},
    privateMetadata: {}
});

const typeOptions = ['CreditCard', 'Cash', 'BankTransfer'];
const displayOptions = ['Both', 'Storefront', 'Admin'];

onMounted(async () => {
    await store.fetchPagedPaymentMethods();
});

function openNew() {
    method.value = { 
        type: 'CreditCard', 
        active: true, 
        autoCapture: false, 
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
            success = await store.updatePaymentMethod(method.value.id, method.value);
        } else {
            success = await store.createPaymentMethod(method.value);
        }

        if (success) {
            toast.add({ severity: 'success', summary: 'Success', detail: 'Payment method saved', life: 3000 });
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
                <Button label="Refresh" icon="pi pi-refresh" severity="secondary" @click="store.fetchPagedPaymentMethods" :loading="store.loading" />
            </template>
        </Toolbar>

        <DataTable :value="store.paymentMethods" :loading="store.loading" paginator :rows="10">
            <Column field="presentation" header="Display Name" sortable></Column>
            <Column field="type" header="Type" sortable></Column>
            <Column field="displayOn" header="Visibility"></Column>
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

        <Dialog v-model:visible="showDialog" :header="method.id ? 'Edit Payment Method' : 'New Payment Method'" modal class="p-fluid w-[35rem]">
            <div class="flex flex-col gap-4">
                <div class="flex flex-col gap-2">
                    <label for="name" class="font-bold">Internal Name</label>
                    <InputText id="name" v-model.trim="method.name" required :class="{ 'p-invalid': submitted && !method.name }" />
                </div>
                <div class="flex flex-col gap-2">
                    <label for="presentation" class="font-bold">Display Name</label>
                    <InputText id="presentation" v-model.trim="method.presentation" required :class="{ 'p-invalid': submitted && !method.presentation }" />
                </div>
                <div class="flex flex-col gap-2">
                    <label class="font-bold">Provider Type</label>
                    <Select v-model="method.type" :options="typeOptions" placeholder="Select Type" />
                </div>
                <div class="flex flex-col gap-2">
                    <label class="font-bold">Description</label>
                    <Textarea v-model="method.description" rows="2" />
                </div>
                <div class="flex items-center gap-6 mt-2">
                    <div class="flex items-center gap-2">
                        <ToggleSwitch v-model="method.active" />
                        <label>Active</label>
                    </div>
                    <div class="flex items-center gap-2">
                        <ToggleSwitch v-model="method.autoCapture" />
                        <label>Auto Capture</label>
                    </div>
                </div>

                <div class="flex flex-col gap-6 mt-4">
                    <MetadataEditor 
                        v-model="method.publicMetadata" 
                        title="Public Metadata" 
                        helpText="Configuration visible to customers or storefront APIs (e.g. icon_url, display_fee)." 
                    />
                    
                    <MetadataEditor 
                        v-model="method.privateMetadata" 
                        title="Private Metadata" 
                        helpText="Sensitive internal configuration (e.g. gateway_id, region_code)." 
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
