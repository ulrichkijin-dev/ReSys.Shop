<script setup>
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useOrderStore } from '@/stores';
import { useToast } from 'primevue/usetoast';

import Tabs from 'primevue/tabs';
import TabList from 'primevue/tablist';
import Tab from 'primevue/tab';
import TabPanels from 'primevue/tabpanels';
import TabPanel from 'primevue/tabpanel';
import Button from 'primevue/button';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Tag from 'primevue/tag';

import OrderShipmentManager from './components/OrderShipmentManager.vue';
import OrderPaymentManager from './components/OrderPaymentManager.vue';

const route = useRoute();
const router = useRouter();
const store = useOrderStore();
const toast = useToast();

const orderId = route.params.id;

onMounted(async () => {
    await store.fetchOrderById(orderId);
});

const formatCurrency = (value) => {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: store.selectedOrder?.currency || 'USD' }).format(value);
};

const getStatusSeverity = (status) => {
    switch (status) {
        case 'Complete': return 'success';
        case 'Confirm': return 'info';
        case 'Payment': return 'warn';
        case 'Canceled': return 'danger';
        default: return 'secondary';
    }
};

async function advanceOrder() {
    const success = await store.advanceOrderState(orderId);
    if (success) {
        toast.add({ severity: 'success', summary: 'Success', detail: 'Order state advanced', life: 3000 });
    }
}

async function cancelOrder() {
    const success = await store.cancelOrder(orderId);
    if (success) {
        toast.add({ severity: 'warn', summary: 'Canceled', detail: 'Order has been canceled', life: 3000 });
    }
}
</script>

<template>
    <div class="card" v-if="store.selectedOrder">
        <div class="flex items-center justify-between mb-6">
            <div class="flex items-center gap-4">
                <Button icon="pi pi-arrow-left" severity="secondary" rounded outlined @click="router.push('/orders')" />
                <div>
                    <div class="text-3xl font-medium mb-1">Order #{{ store.selectedOrder.number }}</div>
                    <div class="flex items-center gap-2 text-muted-color">
                        <Tag :value="store.selectedOrder.state" :severity="getStatusSeverity(store.selectedOrder.state)" />
                        <span>Placed on {{ new Date(store.selectedOrder.createdAt).toLocaleString() }}</span>
                    </div>
                </div>
            </div>
            <div class="flex gap-2">
                <Button v-if="store.selectedOrder.state !== 'Complete' && store.selectedOrder.state !== 'Canceled'" 
                        label="Advance State" icon="pi pi-step-forward" severity="success" @click="advanceOrder" :loading="store.loading" />
                <Button v-if="store.selectedOrder.state !== 'Canceled'" 
                        label="Cancel Order" icon="pi pi-times" severity="danger" outlined @click="cancelOrder" :loading="store.loading" />
            </div>
        </div>

        <div class="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-6">
            <!-- Customer Info -->
            <div class="p-4 border rounded-lg bg-surface-50 dark:bg-surface-950">
                <div class="font-bold mb-3 flex items-center gap-2">
                    <i class="pi pi-user text-primary"></i> Customer
                </div>
                <div>{{ store.selectedOrder.userName || 'Guest User' }}</div>
                <div class="text-muted-color text-sm">{{ store.selectedOrder.email }}</div>
            </div>

            <!-- Financial Summary -->
            <div class="p-4 border rounded-lg bg-surface-50 dark:bg-surface-950 lg:col-span-2">
                <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
                    <div>
                        <div class="text-xs text-muted-color uppercase font-bold mb-1">Item Total</div>
                        <div class="text-xl">{{ formatCurrency(store.selectedOrder.itemTotal) }}</div>
                    </div>
                    <div>
                        <div class="text-xs text-muted-color uppercase font-bold mb-1">Shipping</div>
                        <div class="text-xl">{{ formatCurrency(store.selectedOrder.shipmentTotal) }}</div>
                    </div>
                    <div>
                        <div class="text-xs text-muted-color uppercase font-bold mb-1">Adjustments</div>
                        <div class="text-xl text-red-500">{{ formatCurrency(store.selectedOrder.adjustmentTotal) }}</div>
                    </div>
                    <div class="border-l pl-4">
                        <div class="text-xs text-primary uppercase font-bold mb-1">Grand Total</div>
                        <div class="text-2xl font-bold">{{ formatCurrency(store.selectedOrder.total) }}</div>
                    </div>
                </div>
            </div>
        </div>

        <Tabs value="0">
            <TabList>
                <Tab value="0">Items</Tab>
                <Tab value="1">Fulfillment</Tab>
                <Tab value="2">Payments</Tab>
                <Tab value="3">History</Tab>
            </TabList>
            <TabPanels>
                <!-- Items -->
                <TabPanel value="0">
                    <DataTable :value="store.selectedOrder.lineItems" responsiveLayout="scroll" class="pt-4">
                        <Column field="capturedName" header="Product"></Column>
                        <Column field="capturedSku" header="SKU"></Column>
                        <Column field="unitPrice" header="Price">
                            <template #body="slotProps">
                                {{ formatCurrency(slotProps.data.unitPrice) }}
                            </template>
                        </Column>
                        <Column field="quantity" header="Qty"></Column>
                        <Column field="total" header="Total">
                            <template #body="slotProps">
                                <span class="font-bold">{{ formatCurrency(slotProps.data.total) }}</span>
                            </template>
                        </Column>
                    </DataTable>
                </TabPanel>

                <!-- Fulfillment -->
                <TabPanel value="1">
                    <OrderShipmentManager :orderId="orderId" />
                </TabPanel>

                <!-- Payments -->
                <TabPanel value="2">
                    <OrderPaymentManager :orderId="orderId" />
                </TabPanel>

                <!-- History -->
                <TabPanel value="3">
                    <DataTable :value="store.selectedOrder.histories" responsiveLayout="scroll" size="small" class="pt-4">
                        <Column field="createdAt" header="Timestamp">
                            <template #body="slotProps">
                                {{ new Date(slotProps.data.createdAt).toLocaleString() }}
                            </template>
                        </Column>
                        <Column field="description" header="Event"></Column>
                        <Column field="fromState" header="From"></Column>
                        <Column field="toState" header="To"></Column>
                        <Column field="triggeredBy" header="User"></Column>
                    </DataTable>
                </TabPanel>
            </TabPanels>
        </Tabs>
    </div>
    <div v-else-if="store.loading" class="flex justify-center py-12">
        <ProgressSpinner />
    </div>
</template>
