<script setup>
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useOptionTypeStore } from '@/stores';
import { useOptionValueStore } from '@/stores';
import { useToast } from 'primevue/usetoast';

import Tabs from 'primevue/tabs';
import TabList from 'primevue/tablist';
import Tab from 'primevue/tab';
import TabPanels from 'primevue/tabpanels';
import TabPanel from 'primevue/tabpanel';
import Button from 'primevue/button';
import InputText from 'primevue/inputtext';
import ToggleSwitch from 'primevue/toggleswitch';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';

const route = useRoute();
const router = useRouter();
const optionTypeStore = useOptionTypeStore();
const optionValueStore = useOptionValueStore();
const toast = useToast();

const isNew = computed(() => route.params.id === 'new');
const optionTypeId = ref(route.params.id);

const optionType = ref({
    name: '',
    presentation: '',
    filterable: true,
    position: 0
});

onMounted(async () => {
    if (!isNew.value) {
        await optionTypeStore.fetchOptionTypeById(optionTypeId.value);
        if (optionTypeStore.selectedOptionType) {
            optionType.value = { ...optionTypeStore.selectedOptionType };
            await optionValueStore.fetchPagedOptionValues({ optionTypeId: [optionTypeId.value] });
        }
    }
});

async function onSave() {
    let success = false;
    if (isNew.value) {
        success = await optionTypeStore.createOptionType(optionType.value);
    } else {
        success = await optionTypeStore.updateOptionType(optionTypeId.value, optionType.value);
    }

    if (success) {
        toast.add({ severity: 'success', summary: 'Success', detail: 'Option Type saved', life: 3000 });
        if (isNew.value) router.push('/catalog/option-types');
    }
}
</script>

<template>
    <div class="card">
        <div class="flex items-center justify-between mb-6">
            <div>
                <div class="text-3xl font-medium mb-2">{{ isNew ? 'Create Option Type' : 'Edit Option Type' }}</div>
                <div class="text-muted-color">Define attributes for product variants</div>
            </div>
            <div class="flex gap-2">
                <Button label="Cancel" severity="secondary" outlined @click="router.push('/catalog/option-types')" />
                <Button label="Save" icon="pi pi-check" @click="onSave" :loading="optionTypeStore.loading" />
            </div>
        </div>

        <Tabs value="0">
            <TabList>
                <Tab value="0">Configuration</Tab>
                <Tab value="1" v-if="!isNew">Values</Tab>
            </TabList>
            <TabPanels>
                <TabPanel value="0">
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-6 pt-4">
                        <div class="flex flex-col gap-2">
                            <label class="font-bold">Internal Name</label>
                            <InputText v-model="optionType.name" placeholder="e.g. color" />
                        </div>
                        <div class="flex flex-col gap-2">
                            <label class="font-bold">Display Name</label>
                            <InputText v-model="optionType.presentation" placeholder="e.g. Color" />
                        </div>
                        <div class="flex items-center gap-2 mt-4">
                            <ToggleSwitch v-model="optionType.filterable" />
                            <label class="font-bold">Use in Filters</label>
                        </div>
                    </div>
                </TabPanel>

                <TabPanel value="1" v-if="!isNew">
                    <div class="flex justify-between items-center mb-4">
                        <h5 class="m-0 font-bold">Defined Values</h5>
                        <Button label="Add Value" icon="pi pi-plus" size="small" />
                    </div>
                    <DataTable :value="optionValueStore.optionValues" size="small">
                        <Column field="name" header="Internal Name"></Column>
                        <Column field="presentation" header="Display Value"></Column>
                        <Column field="position" header="Order"></Column>
                        <Column header="Actions">
                            <template #body>
                                <Button icon="pi pi-pencil" text rounded />
                                <Button icon="pi pi-trash" text rounded severity="danger" />
                            </template>
                        </Column>
                    </DataTable>
                </TabPanel>
            </TabPanels>
        </Tabs>
    </div>
</template>
