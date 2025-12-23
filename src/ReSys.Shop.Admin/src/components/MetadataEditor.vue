<script setup>
import { ref, watch, onMounted } from 'vue';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import InputText from 'primevue/inputtext';
import InputNumber from 'primevue/inputnumber';
import Checkbox from 'primevue/checkbox';
import Select from 'primevue/select';

const props = defineProps({
    modelValue: {
        type: Object,
        default: () => ({})
    },
    title: {
        type: String,
        default: 'Metadata'
    },
    helpText: {
        type: String,
        default: ''
    }
});

const emit = defineEmits(['update:modelValue']);

// Internal state as an array for easier DataTable binding
const items = ref([]);
const types = [
    { label: 'String', value: 'string' },
    { label: 'Number', value: 'number' },
    { label: 'Boolean', value: 'boolean' }
];

// New item state
const newItem = ref({
    key: '',
    value: '',
    type: 'string'
});

// Initialize items from modelValue object
const initItems = () => {
    if (!props.modelValue) {
        items.value = [];
        return;
    }
    
    items.value = Object.entries(props.modelValue).map(([key, value]) => {
        let type = 'string';
        if (typeof value === 'number') type = 'number';
        if (typeof value === 'boolean') type = 'boolean';
        
        return { key, value, type };
    });
};

onMounted(initItems);

// Watch for external changes to modelValue
watch(() => props.modelValue, initItems, { deep: true });

// Sync back to parent
const syncModel = () => {
    const obj = {};
    items.value.forEach(item => {
        if (item.key) {
            obj[item.key] = item.value;
        }
    });
    emit('update:modelValue', obj);
};

const addItem = () => {
    if (!newItem.value.key) return;
    
    // Check for duplicates
    if (items.value.some(i => i.key === newItem.value.key)) {
        return;
    }

    let val = newItem.value.value;
    if (newItem.value.type === 'number') val = Number(val) || 0;
    if (newItem.value.type === 'boolean') val = val === true || val === 'true';

    items.value.push({
        key: newItem.value.key,
        value: val,
        type: newItem.value.type
    });

    newItem.value = { key: '', value: '', type: 'string' };
    syncModel();
};

const removeItem = (key) => {
    items.value = items.value.filter(i => i.key !== key);
    syncModel();
};

const onValueChange = () => {
    syncModel();
};
</script>

<template>
    <div class="metadata-editor border rounded-lg p-4 bg-surface-0 dark:bg-surface-900">
        <div class="flex flex-col mb-4">
            <span class="font-bold text-lg text-surface-900 dark:text-surface-0">{{ title }}</span>
            <small v-if="helpText" class="text-muted-color">{{ helpText }}</small>
        </div>

        <DataTable :value="items" class="p-datatable-sm mb-4" responsiveLayout="scroll">
            <Column field="key" header="Key">
                <template #body="slotProps">
                    <code class="text-primary font-bold">{{ slotProps.data.key }}</code>
                </template>
            </Column>
            <Column header="Value">
                <template #body="slotProps">
                    <div class="flex items-center">
                        <InputNumber v-if="slotProps.data.type === 'number'" v-model="slotProps.data.value" @update:modelValue="onValueChange" class="w-full" size="small" />
                        <Checkbox v-else-if="slotProps.data.type === 'boolean'" v-model="slotProps.data.value" :binary="true" @change="onValueChange" />
                        <InputText v-else v-model="slotProps.data.value" @input="onValueChange" class="w-full" size="small" />
                    </div>
                </template>
            </Column>
            <Column header="Actions" class="w-24">
                <template #body="slotProps">
                    <Button icon="pi pi-trash" severity="danger" text rounded @click="removeItem(slotProps.data.key)" />
                </template>
            </Column>
        </DataTable>

        <div class="grid grid-cols-1 md:grid-cols-4 gap-2 items-end p-3 bg-surface-50 dark:bg-surface-950 rounded-lg">
            <div class="flex flex-col gap-1">
                <label class="text-xs font-bold">New Key</label>
                <InputText v-model="newItem.key" placeholder="key_name" size="small" />
            </div>
            <div class="flex flex-col gap-1">
                <label class="text-xs font-bold">Type</label>
                <Select v-model="newItem.type" :options="types" optionLabel="label" optionValue="value" size="small" />
            </div>
            <div class="flex flex-col gap-1">
                <label class="text-xs font-bold">Value</label>
                <InputNumber v-if="newItem.type === 'number'" v-model="newItem.value" class="w-full" size="small" />
                <div v-else-if="newItem.type === 'boolean'" class="flex items-center h-10">
                    <Checkbox v-model="newItem.value" :binary="true" />
                </div>
                <InputText v-else v-model="newItem.value" placeholder="value" class="w-full" size="small" />
            </div>
            <Button label="Add" icon="pi pi-plus" size="small" @click="addItem" :disabled="!newItem.key" />
        </div>
    </div>
</template>

<style scoped>
.metadata-editor :deep(.p-datatable-thead > tr > th) {
    background: transparent;
}
</style>
