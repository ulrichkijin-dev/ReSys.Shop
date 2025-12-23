<script setup>
import { onMounted, ref } from 'vue';
import { useVariantStore } from '@/stores';
import { useProductOptionTypeStore } from '@/stores';
import { useOptionValueStore } from '@/stores';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import Tag from 'primevue/tag';
import Dialog from 'primevue/dialog';
import MultiSelect from 'primevue/multiselect';

const props = defineProps({
    productId: { type: String, required: true }
});

const variantStore = useVariantStore();
const potStore = useProductOptionTypeStore();
const ovStore = useOptionValueStore();

const showOptionDialog = ref(false);
const editingVariant = ref(null);
const selectedOptions = ref([]);

onMounted(async () => {
    await variantStore.fetchPagedVariants({ productId: props.productId });
    await potStore.fetchProductOptionTypes({ productId: [props.productId] });
});

async function openOptions(variant) {
    editingVariant.value = variant;
    // Load ALL option values for the types assigned to this product
    const optionTypeIds = potStore.productOptionTypes.map(pot => pot.optionTypeId);
    await ovStore.fetchSelectOptionValues({ optionTypeId: optionTypeIds });
    
    selectedOptions.value = variant.optionValueNames || [];
    showOptionDialog.value = true;
}

const getStockSeverity = (inStock) => {
    return inStock ? 'success' : 'danger';
};
</script>

<template>
    <div class="pt-4">
        <div class="flex justify-between items-center mb-4">
            <h5 class="m-0 font-bold">Product Variants</h5>
            <Button label="Generate Variants" icon="pi pi-cog" severity="secondary" outlined disabled title="Coming soon: Matrix generator" />
        </div>

        <DataTable :value="variantStore.variants" responsiveLayout="scroll">
            <Column field="sku" header="SKU" sortable></Column>
            <Column field="optionsText" header="Options"></Column>
            <Column field="purchasable" header="Purchasable">
                <template #body="slotProps">
                    <i class="pi" :class="{ 'pi-check text-green-500': slotProps.data.purchasable, 'pi-times text-red-500': !slotProps.data.purchasable }"></i>
                </template>
            </Column>
            <Column field="inStock" header="Stock">
                <template #body="slotProps">
                    <Tag :value="slotProps.data.inStock ? 'In Stock' : 'Out of Stock'" :severity="getStockSeverity(slotProps.data.inStock)" />
                </template>
            </Column>
            <Column header="Actions">
                <template #body="slotProps">
                    <div class="flex gap-2">
                        <Button icon="pi pi-pencil" outlined rounded />
                        <Button icon="pi pi-tags" severity="info" outlined rounded @click="openOptions(slotProps.data)" title="Manage Options" />
                        <Button icon="pi pi-money-bill" severity="success" outlined rounded title="Manage Pricing" />
                    </div>
                </template>
            </Column>
        </DataTable>

        <Dialog v-model:visible="showOptionDialog" header="Manage Variant Options" modal :style="{ width: '35rem' }">
            <div class="flex flex-col gap-4">
                <p class="text-muted-color">Selected options determine how this variant is distinguished (e.g. Size: XL, Color: Blue).</p>
                <div class="flex flex-col gap-2">
                    <label class="font-bold">Option Values</label>
                    <MultiSelect 
                        v-model="selectedOptions" 
                        :options="ovStore.selectOptionValues?.items" 
                        optionLabel="presentation" 
                        optionValue="name"
                        placeholder="Select Option Values" 
                        class="w-full"
                        display="chip"
                    />
                </div>
                <div class="flex justify-end gap-2 mt-4">
                    <Button label="Cancel" severity="secondary" text @click="showDialog = false" />
                    <Button label="Save Options" icon="pi pi-check" />
                </div>
            </div>
        </Dialog>
    </div>
</template>
