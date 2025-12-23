<script setup>
import { onMounted, ref } from 'vue';
import { useProductOptionTypeStore } from '@/stores';
import { useOptionTypeStore } from '@/stores';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import Select from 'primevue/select';

const props = defineProps({
    productId: { type: String, required: true }
});

const store = useProductOptionTypeStore();
const optionTypeStore = useOptionTypeStore();
const selectedOptionType = ref(null);

onMounted(async () => {
    await store.fetchProductOptionTypes({ productId: [props.productId] });
    await optionTypeStore.fetchSelectOptionTypes();
});

async function addOptionType() {
    if (!selectedOptionType.value) return;
    const newData = store.productOptionTypes.map(o => ({ optionTypeId: o.optionTypeId, position: o.position }));
    newData.push({ optionTypeId: selectedOptionType.value.id, position: newData.length });
    await store.manageProductOptionTypes(props.productId, { data: newData });
    selectedOptionType.value = null;
    await store.fetchProductOptionTypes({ productId: [props.productId] });
}

async function removeOptionType(id) {
    const newData = store.productOptionTypes
        .filter(o => o.optionTypeId !== id)
        .map((o, idx) => ({ optionTypeId: o.optionTypeId, position: idx }));
    await store.manageProductOptionTypes(props.productId, { data: newData });
    await store.fetchProductOptionTypes({ productId: [props.productId] });
}
</script>

<template>
    <div class="pt-4">
        <div class="p-4 bg-surface-50 dark:bg-surface-950 rounded mb-6">
            <div class="font-bold mb-4 text-lg">Assign Option Type (Variant Generator)</div>
            <div class="flex gap-4 items-end">
                <div class="flex flex-col gap-2 grow">
                    <label>Select Option Type (e.g. Size, Color)</label>
                    <Select v-model="selectedOptionType" :options="optionTypeStore.selectOptionTypes?.items" optionLabel="presentation" placeholder="Choose one..." class="w-full" />
                </div>
                <Button label="Add to Product" icon="pi pi-plus" @click="addOptionType" :disabled="!selectedOptionType" />
            </div>
        </div>

        <DataTable :value="store.productOptionTypes">
            <Column field="optionTypeName" header="Internal Name"></Column>
            <Column field="optionTypePresentation" header="Display Name"></Column>
            <Column field="position" header="Order"></Column>
            <Column header="Actions">
                <template #body="slotProps">
                    <Button icon="pi pi-trash" severity="danger" text rounded @click="removeOptionType(slotProps.data.optionTypeId)" />
                </template>
            </Column>
        </DataTable>
    </div>
</template>
