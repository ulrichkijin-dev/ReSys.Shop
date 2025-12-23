<script setup>
import { onMounted, ref } from 'vue';
import { useOrderStore } from '@/stores';
import { shipmentService } from '@/services';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import Tag from 'primevue/tag';
import Dialog from 'primevue/dialog';
import InputText from 'primevue/inputtext';

const props = defineProps({
    orderId: { type: String, required: true }
});

const store = useOrderStore();
const showShipDialog = ref(false);
const shipmentToShip = ref(null);
const trackingNumber = ref('');
const shipping = ref(false);

onMounted(async () => {
    // Shipments are typically included in OrderDetail, but we can refresh them
});

const getShipmentStatusSeverity = (status) => {
    switch (status) {
        case 'Shipped': return 'success';
        case 'Ready': return 'info';
        case 'Pending': return 'warn';
        case 'Canceled': return 'danger';
        default: return 'secondary';
    }
};

async function readyShipment(shipmentId) {
    await shipmentService.ready(props.orderId, shipmentId);
    await store.fetchOrderById(props.orderId);
}

function openShipDialog(shipment) {
    shipmentToShip.value = shipment;
    trackingNumber.value = 'TRK-' + Math.random().toString(36).substring(7).toUpperCase();
    showShipDialog.value = true;
}

async function confirmShip() {
    if (!shipmentToShip.value) return;

    shipping.value = true;
    try {
        await shipmentService.ship(props.orderId, shipmentToShip.value.id, {
            trackingNumber: trackingNumber.value
        });
        await store.fetchOrderById(props.orderId);
        showShipDialog.value = false;
    } finally {
        shipping.value = false;
    }
}
</script>

<template>
    <div class="pt-4">
        <div class="flex justify-between items-center mb-4">
            <h5 class="m-0 font-bold">Fulfillment & Shipments</h5>
            <Button label="Auto Plan Shipments" icon="pi pi-bolt" severity="secondary" outlined />
        </div>

        <div v-for="shipment in store.selectedOrder?.shipments" :key="shipment.id" class="mb-6 border rounded-lg overflow-hidden">
            <div class="bg-surface-100 dark:bg-surface-800 p-3 flex justify-between items-center">
                <div class="flex items-center gap-3">
                    <span class="font-bold">#{{ shipment.number }}</span>
                    <Tag :value="shipment.state" :severity="getShipmentStatusSeverity(shipment.state)" />
                    <span class="text-sm text-muted-color ml-2"><i class="pi pi-map-marker text-xs mr-1"></i>{{ shipment.stockLocationName }}</span>
                </div>
                <div class="flex gap-2">
                    <Button v-if="shipment.state === 'Pending'" label="Ready" size="small" severity="info" @click="readyShipment(shipment.id)" />
                    <Button v-if="shipment.state === 'Ready'" label="Ship" size="small" severity="success" @click="openShipDialog(shipment)" />
                    <Button v-if="shipment.state !== 'Shipped' && shipment.state !== 'Canceled'" icon="pi pi-times" size="small" severity="danger" text />
                </div>
            </div>

            <div class="p-3">
                <div v-if="shipment.trackingNumber" class="mb-3">
                    <span class="font-medium mr-2">Tracking:</span>
                    <code class="text-primary">{{ shipment.trackingNumber }}</code>
                </div>

                <div class="text-sm italic text-muted-color" v-if="shipment.shippedAt">
                    Shipped on: {{ new Date(shipment.shippedAt).toLocaleString() }}
                </div>
            </div>
        </div>

        <div v-if="!store.selectedOrder?.shipments?.length" class="text-center py-8 bg-surface-50 dark:bg-surface-950 rounded border-2 border-dashed">
            <i class="pi pi-truck text-4xl text-muted-color mb-3"></i>
            <p>No shipments have been created for this order yet.</p>
            <Button label="Create Manual Shipment" icon="pi pi-plus" class="mt-2" size="small" />
        </div>

        <Dialog v-model:visible="showShipDialog" header="Ship Order" :style="{ width: '400px' }" modal>
            <div class="flex flex-col gap-4">
                <p>Please enter the tracking number for shipment <strong>#{{ shipmentToShip?.number }}</strong></p>
                <div class="flex flex-col gap-2">
                    <label for="tracking">Tracking Number</label>
                    <InputText id="tracking" v-model="trackingNumber" autofocus />
                </div>
            </div>
            <template #footer>
                <Button label="Cancel" severity="secondary" text @click="showShipDialog = false" />
                <Button label="Confirm Shipment" severity="success" @click="confirmShip" :loading="shipping" />
            </template>
        </Dialog>
    </div>
</template>
