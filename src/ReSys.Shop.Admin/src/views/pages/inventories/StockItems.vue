<script setup>
import { ref, onMounted } from 'vue';
import { useStockItemStore } from '@/stores';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import Toolbar from 'primevue/toolbar';
import Dialog from 'primevue/dialog';
import InputNumber from 'primevue/inputnumber';
import InputText from 'primevue/inputtext';
import { useToast } from 'primevue/usetoast';

const store = useStockItemStore();
const toast = useToast();

const showAdjustDialog = ref(false);
const showHistoryDialog = ref(false);
const selectedItem = ref(null);
const adjustment = ref({ quantity: 0, reason: '' });

onMounted(async () => {
    await store.fetchPagedStockItems();
});

function openAdjust(item) {
    selectedItem.value = item;
    adjustment.value = { quantity: 0, reason: 'Manual Adjustment' };
    showAdjustDialog.value = true;
}

async function openHistory(item) {
    selectedItem.value = item;
    await store.fetchStockMovements(item.id);
    showHistoryDialog.value = true;
}

async function saveAdjustment() {
    if (adjustment.value.quantity !== 0) {
        const success = await store.adjustStock(selectedItem.value.id, adjustment.value);
        if (success) {
            toast.add({ severity: 'success', summary: 'Successful', detail: 'Stock Adjusted', life: 3000 });
            showAdjustDialog.value = false;
            await store.fetchPagedStockItems();
        }
    }
}
</script>

<template>
    <div class="card">
        <Toolbar class="mb-4">
            <template #start>
                <Button label="Refresh" icon="pi pi-refresh" severity="secondary" @click="store.fetchPagedStockItems" :loading="store.loading" />
            </template>
        </Toolbar>

        <DataTable :value="store.stockItems" :loading="store.loading" paginator :rows="10">
            <Column field="sku" header="SKU" sortable></Column>
            <Column field="productName" header="Product" sortable></Column>
            <Column field="locationName" header="Location" sortable></Column>
            <Column field="quantityOnHand" header="On Hand" sortable>
                <template #body="slotProps">
                    <span :class="{'text-red-500 font-bold': slotProps.data.quantityOnHand <= 0}">
                        {{ slotProps.data.quantityOnHand }}
                    </span>
                </template>
            </Column>
            <Column field="quantityReserved" header="Reserved" sortable></Column>
            <Column field="countAvailable" header="Available" sortable></Column>
            <Column header="Actions" style="min-width: 12rem">
                <template #body="slotProps">
                    <Button icon="pi pi-plus-minus" outlined rounded class="mr-2" @click="openAdjust(slotProps.data)" title="Adjust Stock" />
                    <Button icon="pi pi-history" severity="info" outlined rounded @click="openHistory(slotProps.data)" title="View Movements" />
                </template>
            </Column>
        </DataTable>

        <!-- Adjustment Dialog -->
        <Dialog v-model:visible="showAdjustDialog" header="Adjust Stock Level" modal class="w-120">
            <div class="flex flex-col gap-4">
                <div v-if="selectedItem" class="p-3 bg-surface-100 dark:bg-surface-800 rounded">
                    <div class="text-sm text-muted-color">Product</div>
                    <div class="font-bold">{{ selectedItem.productName }}</div>
                    <div class="text-xs mt-1">Current On Hand: {{ selectedItem.quantityOnHand }}</div>
                </div>
                <div class="flex flex-col gap-2">
                    <label>Adjustment Amount (+ or -)</label>
                    <InputNumber v-model="adjustment.quantity" showButtons buttonLayout="horizontal" incrementIcon="pi pi-plus" decrementIcon="pi pi-minus" />
                </div>
                <div class="flex flex-col gap-2">
                    <label>Reason</label>
                    <InputText v-model="adjustment.reason" placeholder="e.g. Received shipment, Damaged goods" />
                </div>
            </div>
            <template #footer>
                <Button label="Cancel" text @click="showAdjustDialog = false" />
                <Button label="Apply Adjustment" icon="pi pi-check" @click="saveAdjustment" :loading="store.loading" />
            </template>
        </Dialog>

        <!-- History Dialog -->
        <Dialog v-model:visible="showHistoryDialog" header="Stock Movement History" modal class="w-[50rem]">
            <DataTable :value="store.stockMovements" size="small" paginator :rows="5">
                <Column field="createdAt" header="Date">
                    <template #body="slotProps">
                        {{ new Date(slotProps.data.createdAt).toLocaleString() }}
                    </template>
                </Column>
                <Column field="action" header="Action"></Column>
                <Column field="quantity" header="Qty">
                    <template #body="slotProps">
                        <span :class="slotProps.data.isIncrease ? 'text-green-500' : 'text-red-500'">
                            {{ slotProps.data.isIncrease ? '+' : '' }}{{ slotProps.data.quantity }}
                        </span>
                    </template>
                </Column>
                <Column field="originator" header="Source"></Column>
                <Column field="reason" header="Reason"></Column>
            </DataTable>
        </Dialog>
    </div>
</template>
