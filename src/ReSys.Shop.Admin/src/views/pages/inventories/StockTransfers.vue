<script setup>
import { ref, onMounted } from 'vue';
import { useStockTransferStore } from '@/stores';
import { useStockLocationStore } from '@/stores';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import Toolbar from 'primevue/toolbar';
import Dialog from 'primevue/dialog';
import Select from 'primevue/select';
import InputText from 'primevue/inputtext';
import { useToast } from 'primevue/usetoast';

const store = useStockTransferStore();
const locationStore = useStockLocationStore();
const toast = useToast();

const showCreateDialog = ref(false);
const newTransfer = ref({
    sourceLocationId: null,
    destinationLocationId: null,
    reference: ''
});

onMounted(async () => {
    await store.fetchPagedStockTransfers();
    await locationStore.fetchSelectStockLocations();
});

async function createTransfer() {
    if (newTransfer.value.destinationLocationId) {
        const success = await store.createStockTransfer(newTransfer.value);
        if (success) {
            toast.add({ severity: 'success', summary: 'Success', detail: 'Transfer Record Created', life: 3000 });
            showCreateDialog.value = false;
            await store.fetchPagedStockTransfers();
        }
    }
}
</script>

<template>
    <div class="card">
        <Toolbar class="mb-4">
            <template #start>
                <Button label="New Transfer" icon="pi pi-plus" class="mr-2" @click="showCreateDialog = true" />
                <Button label="Refresh" icon="pi pi-refresh" severity="secondary" @click="store.fetchPagedStockTransfers" :loading="store.loading" />
            </template>
        </Toolbar>

        <DataTable :value="store.stockTransfers" :loading="store.loading" paginator :rows="10">
            <Column field="number" header="Number" sortable></Column>
            <Column field="sourceLocationName" header="Source">
                <template #body="slotProps">
                    {{ slotProps.data.sourceLocationName || 'External Supplier' }}
                </template>
            </Column>
            <Column field="destinationLocationName" header="Destination"></Column>
            <Column field="reference" header="Reference"></Column>
            <Column field="movementCount" header="Items"></Column>
            <Column field="createdAt" header="Date">
                <template #body="slotProps">
                    {{ new Date(slotProps.data.createdAt).toLocaleDateString() }}
                </template>
            </Column>
            <Column header="Actions">
                <template #body="slotProps">
                    <Button icon="pi pi-eye" outlined rounded class="mr-2" />
                    <Button icon="pi pi-sign-in" severity="success" outlined rounded title="Execute/Receive" />
                </template>
            </Column>
        </DataTable>

        <Dialog v-model:visible="showCreateDialog" header="Initiate Stock Transfer" modal class="w-[35rem]">
            <div class="flex flex-col gap-4">
                <div class="flex flex-col gap-2">
                    <label>Source Location</label>
                    <Select v-model="newTransfer.sourceLocationId" :options="locationStore.selectStockLocations?.items" optionLabel="presentation" optionValue="id" placeholder="Select Source (Leave empty for Receive)" showClear />
                </div>
                <div class="flex flex-col gap-2">
                    <label>Destination Location</label>
                    <Select v-model="newTransfer.destinationLocationId" :options="locationStore.selectStockLocations?.items" optionLabel="presentation" optionValue="id" placeholder="Select Destination" />
                </div>
                <div class="flex flex-col gap-2">
                    <label>Reference / Note</label>
                    <InputText v-model="newTransfer.reference" placeholder="e.g. PO-12345" />
                </div>
            </div>
            <template #footer>
                <Button label="Cancel" text @click="showCreateDialog = false" />
                <Button label="Create Transfer" icon="pi pi-check" @click="createTransfer" :loading="store.loading" />
            </template>
        </Dialog>
    </div>
</template>
