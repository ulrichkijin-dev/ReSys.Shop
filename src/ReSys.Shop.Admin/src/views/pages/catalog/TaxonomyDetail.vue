<script setup>
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useTaxonomyStore } from '@/stores';
import { useTaxonStore } from '@/stores';
import { useToast } from 'primevue/usetoast';

import Tabs from 'primevue/tabs';
import TabList from 'primevue/tablist';
import Tab from 'primevue/tab';
import TabPanels from 'primevue/tabpanels';
import TabPanel from 'primevue/tabpanel';
import Button from 'primevue/button';
import InputText from 'primevue/inputtext';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';

const route = useRoute();
const router = useRouter();
const taxonomyStore = useTaxonomyStore();
const taxonStore = useTaxonStore();
const toast = useToast();

const isNew = computed(() => route.params.id === 'new');
const taxonomyId = ref(route.params.id);

const taxonomy = ref({
    name: '',
    presentation: '',
    position: 0
});

onMounted(async () => {
    if (!isNew.value) {
        await taxonomyStore.fetchTaxonomyById(taxonomyId.value);
        if (taxonomyStore.selectedTaxonomy) {
            taxonomy.value = { ...taxonomyStore.selectedTaxonomy };
            // Fetch taxons for this taxonomy
            await taxonStore.fetchPagedTaxons({ taxonomyId: [taxonomyId.value] });
        }
    }
});

async function onSave() {
    let success = false;
    if (isNew.value) {
        success = await taxonomyStore.createTaxonomy(taxonomy.value);
    } else {
        success = await taxonomyStore.updateTaxonomy(taxonomyId.value, taxonomy.value);
    }

    if (success) {
        toast.add({ severity: 'success', summary: 'Success', detail: 'Taxonomy saved successfully', life: 3000 });
        if (isNew.value) router.push('/catalog/taxonomies');
    }
}

const editTaxon = (id) => {
    router.push(`/catalog/taxons/${id}`);
};

const createTaxon = () => {
    router.push({ name: 'admin-taxon-new', query: { taxonomyId: taxonomyId.value } });
};

const goToTree = () => {
    router.push(`/catalog/taxonomies/${taxonomyId.value}/tree`);
};
</script>

<template>
    <div class="card">
        <div class="flex items-center justify-between mb-6">
            <div>
                <div class="text-3xl font-medium text-surface-900 dark:text-surface-0 mb-2">
                    {{ isNew ? 'Create Taxonomy' : 'Edit Taxonomy' }}
                </div>
                <div class="text-muted-color">{{ isNew ? 'Define a new classification group' : `Managing: ${taxonomy.presentation}` }}</div>
            </div>
            <div class="flex gap-2">
                <Button label="Cancel" severity="secondary" outlined @click="router.push('/catalog/taxonomies')" />
                <Button label="Save" icon="pi pi-check" @click="onSave" :loading="taxonomyStore.loading" />
            </div>
        </div>

        <Tabs value="0">
            <TabList>
                <Tab value="0">General Info</Tab>
                <Tab value="1" v-if="!isNew">Taxons (List)</Tab>
            </TabList>
            <TabPanels>
                <TabPanel value="0">
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-6 pt-4">
                        <div class="flex flex-col gap-2">
                            <label for="name" class="font-bold">Internal Name</label>
                            <InputText id="name" v-model="taxonomy.name" placeholder="e.g. category" />
                        </div>
                        <div class="flex flex-col gap-2">
                            <label for="presentation" class="font-bold">Display Name</label>
                            <InputText id="presentation" v-model="taxonomy.presentation" placeholder="e.g. Categories" />
                        </div>
                        <div class="flex flex-col gap-2">
                            <label for="position" class="font-bold">Position</label>
                            <InputText id="position" v-model.number="taxonomy.position" type="number" />
                        </div>
                    </div>
                </TabPanel>

                <TabPanel value="1" v-if="!isNew">
                    <div class="flex justify-between items-center mb-4">
                        <h5 class="m-0 font-bold">Taxons in this Taxonomy</h5>
                        <div class="flex gap-2">
                            <Button label="Build Tree" icon="pi pi-sitemap" severity="info" outlined @click="goToTree" />
                            <Button label="Add Taxon" icon="pi pi-plus" @click="createTaxon" />
                        </div>
                    </div>

                    <DataTable :value="taxonStore.taxons" :loading="taxonStore.loading" responsiveLayout="scroll">
                        <Column field="name" header="Name" sortable></Column>
                        <Column field="prettyName" header="Full Path" sortable></Column>
                        <Column field="parentName" header="Parent"></Column>
                        <Column field="position" header="Position" sortable></Column>
                        <Column header="Actions">
                            <template #body="slotProps">
                                <Button icon="pi pi-pencil" outlined rounded class="mr-2" @click="editTaxon(slotProps.data.id)" />
                                <Button icon="pi pi-trash" outlined rounded severity="danger" />
                            </template>
                        </Column>
                    </DataTable>
                </TabPanel>
            </TabPanels>
        </Tabs>
    </div>
</template>
