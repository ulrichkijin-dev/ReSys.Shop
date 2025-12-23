<script setup>
import { useOrderStore } from '@/stores';
import { paymentService } from '@/services';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import Tag from 'primevue/tag';

const props = defineProps({
    orderId: { type: String, required: true }
});

const store = useOrderStore();

const getPaymentStatusSeverity = (status) => {
    switch (status) {
        case 'Captured': return 'success';
        case 'Authorized': return 'info';
        case 'Pending': return 'warn';
        case 'Failed':
        case 'Void': return 'danger';
        default: return 'secondary';
    }
};

const formatCurrency = (value) => {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: store.selectedOrder?.currency || 'USD' }).format(value);
};

async function capturePayment(paymentId) {
    await paymentService.capture(props.orderId, paymentId);
    await store.fetchOrderById(props.orderId);
}

async function refundPayment(paymentId) {
    // Logic for refund dialog
}
</script>

<template>
    <div class="pt-4">
        <div class="flex justify-between items-center mb-4">
            <h5 class="m-0 font-bold">Payment Transactions</h5>
            <Button label="Add Manual Payment" icon="pi pi-plus" severity="secondary" outlined size="small" />
        </div>

        <DataTable :value="store.selectedOrder?.payments" responsiveLayout="scroll">
            <Column field="createdAt" header="Date">
                <template #body="slotProps">
                    {{ new Date(slotProps.data.createdAt).toLocaleString() }}
                </template>
            </Column>
            <Column field="paymentMethodType" header="Method"></Column>
            <Column field="amount" header="Amount">
                <template #body="slotProps">
                    {{ formatCurrency(slotProps.data.amount) }}
                </template>
            </Column>
            <Column field="state" header="Status">
                <template #body="slotProps">
                    <Tag :value="slotProps.data.state" :severity="getPaymentStatusSeverity(slotProps.data.state)" />
                </template>
            </Column>
            <Column field="referenceTransactionId" header="Ref ID">
                <template #body="slotProps">
                    <small class="font-mono">{{ slotProps.data.referenceTransactionId || 'N/A' }}</small>
                </template>
            </Column>
            <Column header="Actions">
                <template #body="slotProps">
                    <div class="flex gap-2">
                        <Button v-if="slotProps.data.state === 'Authorized'" icon="pi pi-check-circle" severity="success" outlined rounded @click="capturePayment(slotProps.data.id)" title="Capture" />
                        <Button v-if="slotProps.data.state === 'Captured'" icon="pi pi-replay" severity="warning" outlined rounded title="Refund" />
                        <Button v-if="slotProps.data.state === 'Authorized'" icon="pi pi-ban" severity="danger" outlined rounded title="Void" />
                    </div>
                </template>
            </Column>
        </DataTable>
    </div>
</template>
