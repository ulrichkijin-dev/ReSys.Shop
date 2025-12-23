<script setup>
import { onMounted } from 'vue';
import { useProductPropertyStore } from '@/stores';
import { usePropertyTypeStore } from '@/stores';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import Select from 'primevue/select';
import InputText from 'primevue/inputtext';

const props = defineProps({
    productId: {
        type: String,
        required: true
    }
});

const productPropertyStore = useProductPropertyStore();
const propertyTypeStore = usePropertyTypeStore();

onMounted(async () => {
    await productPropertyStore.fetchProductProperties({ productId: [props.productId] });
    await propertyTypeStore.fetchSelectPropertyTypes();
});
</script>

<template>
    <div class="pt-4">
        <div class="p-4 bg-surface-50 dark:bg-surface-950 rounded mb-6">
            <div class="font-bold mb-4 text-lg">Add New Property</div>
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4 items-end">
                <div class="flex flex-col gap-2">
                    <label>Property Type</label>
                    <Select :options="propertyTypeStore.selectPropertyTypes?.items" optionLabel="presentation" placeholder="Select Type" disabled />
                </div>
                <div class="flex flex-col gap-2">
                    <label>Value</label>
                    <InputText placeholder="Property value" disabled />
                </div>
                <Button label="Add Property" icon="pi pi-plus" disabled />
            </div>
        </div>

        <DataTable :value="productPropertyStore.productProperties" responsiveLayout="scroll">
            <Column field="propertyTypeName" header="Property"></Column>
            <Column field="propertyValue" header="Value"></Column>
            <Column field="position" header="Order"></Column>
            <Column header="Actions">
                <template #body="slotProps">
                    <Button icon="pi pi-trash" severity="danger" outlined rounded />
                </template>
            </Column>
        </DataTable>
    </div>
</template>
