<script setup>
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { usePropertyTypeStore } from '@/stores';
import { useToast } from 'primevue/usetoast';

import Button from 'primevue/button';
import InputText from 'primevue/inputtext';
import Select from 'primevue/select';
import ToggleSwitch from 'primevue/toggleswitch';

const route = useRoute();
const router = useRouter();
const propertyTypeStore = usePropertyTypeStore();
const toast = useToast();

const isNew = computed(() => route.params.id === 'new');
const propertyTypeId = ref(route.params.id);

const propertyType = ref({
    name: '',
    presentation: '',
    kind: 'Text',
    displayOn: 'Both',
    filterable: true,
    filterParam: '',
    position: 0
});

const kindOptions = ['Text', 'Number', 'Boolean', 'Date', 'Select'];
const displayOptions = ['Both', 'Storefront', 'Admin'];

onMounted(async () => {
    if (!isNew.value) {
        await propertyTypeStore.fetchPropertyTypeById(propertyTypeId.value);
        if (propertyTypeStore.selectedPropertyType) {
            propertyType.value = { ...propertyTypeStore.selectedPropertyType };
        }
    }
});

async function onSave() {
    let success = false;
    if (isNew.value) {
        success = await propertyTypeStore.createPropertyType(propertyType.value);
    } else {
        success = await propertyTypeStore.updatePropertyType(propertyTypeId.value, propertyType.value);
    }

    if (success) {
        toast.add({ severity: 'success', summary: 'Success', detail: 'Property Type saved', life: 3000 });
        if (isNew.value) router.push('/catalog/property-types');
    }
}
</script>

<template>
    <div class="card">
        <div class="flex items-center justify-between mb-6">
            <div>
                <div class="text-3xl font-medium mb-2">{{ isNew ? 'Create Property Type' : 'Edit Property Type' }}</div>
                <div class="text-muted-color">Define custom fields for products (e.g. Material, Manufacturer)</div>
            </div>
            <div class="flex gap-2">
                <Button label="Cancel" severity="secondary" outlined @click="router.push('/catalog/property-types')" />
                <Button label="Save" icon="pi pi-check" @click="onSave" :loading="propertyTypeStore.loading" />
            </div>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-6 pt-4">
            <div class="flex flex-col gap-2">
                <label class="font-bold">Internal Name</label>
                <InputText v-model="propertyType.name" />
            </div>
            <div class="flex flex-col gap-2">
                <label class="font-bold">Display Name</label>
                <InputText v-model="propertyType.presentation" />
            </div>
            <div class="flex flex-col gap-2">
                <label class="font-bold">Data Kind</label>
                <Select v-model="propertyType.kind" :options="kindOptions" />
            </div>
            <div class="flex flex-col gap-2">
                <label class="font-bold">Visibility</label>
                <Select v-model="propertyType.displayOn" :options="displayOptions" />
            </div>
            <div class="flex flex-col gap-2">
                <label class="font-bold">Filter Parameter (Slug)</label>
                <InputText v-model="propertyType.filterParam" placeholder="e.g. material-slug" />
            </div>
            <div class="flex items-center gap-2 mt-4">
                <ToggleSwitch v-model="propertyType.filterable" />
                <label class="font-bold">Use in Filters</label>
            </div>
        </div>
    </div>
</template>
