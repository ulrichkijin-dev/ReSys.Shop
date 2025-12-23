<script setup>
import { ref, onMounted } from 'vue';
import { useProductClassificationStore } from '@/stores';
import { useTaxonomyStore } from '@/stores';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import MultiSelect from 'primevue/multiselect';

const props = defineProps({
    productId: {
        type: String,
        required: true
    }
});

const classificationStore = useProductClassificationStore();
const taxonomyStore = useTaxonomyStore();

onMounted(async () => {
    await classificationStore.fetchProductClassifications({ productId: [props.productId] });
    await taxonomyStore.fetchSelectTaxonomies();
});

async function removeClassification(id) {
    // Implement delete logic if store supports it or through manage
}
</script>

<template>
    <div class="pt-4">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
            <div class="flex flex-col gap-2">
                <label class="font-bold">Add to Taxonomies</label>
                <div class="flex gap-2">
                    <MultiSelect :options="taxonomyStore.selectTaxonomies?.items" optionLabel="presentation" placeholder="Select Categories" class="w-full" disabled />
                    <Button label="Apply" disabled />
                </div>
            </div>
        </div>

        <DataTable :value="classificationStore.productClassifications" responsiveLayout="scroll">
            <Column field="taxonName" header="Taxon Name"></Column>
            <Column field="taxonPrettyName" header="Full Path"></Column>
            <Column field="position" header="Position"></Column>
            <Column header="Actions">
                <template #body="slotProps">
                    <Button icon="pi pi-trash" severity="danger" outlined rounded />
                </template>
            </Column>
        </DataTable>
    </div>
</template>
