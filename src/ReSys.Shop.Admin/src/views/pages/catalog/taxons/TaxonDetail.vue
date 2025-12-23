<script setup>
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useTaxonStore } from '@/stores';
import { useTaxonomyStore } from '@/stores';
import { useToast } from 'primevue/usetoast';

import Button from 'primevue/button';
import InputText from 'primevue/inputtext';
import Textarea from 'primevue/textarea';
import Select from 'primevue/select';
import ToggleSwitch from 'primevue/toggleswitch';

const route = useRoute();
const router = useRouter();
const taxonStore = useTaxonStore();
const taxonomyStore = useTaxonomyStore();
const toast = useToast();

const isNew = computed(() => route.params.id === 'new');
const taxonId = ref(route.params.id);
const targetTaxonomyId = ref(route.query.taxonomyId);

const taxon = ref({
    taxonomyId: '',
    parentId: null,
    name: '',
    presentation: '',
    description: '',
    position: 0,
    hideFromNav: false,
    automatic: false,
    rulesMatchPolicy: 'all',
    sortOrder: 'position'
});

const matchPolicies = [
    { label: 'Match All Rules', value: 'all' },
    { label: 'Match Any Rule', value: 'any' }
];

const sortOrders = [
    { label: 'Position', value: 'position' },
    { label: 'Name Ascending', value: 'name_asc' },
    { label: 'Name Descending', value: 'name_desc' }
];

onMounted(async () => {
    await taxonomyStore.fetchSelectTaxonomies();
    
    if (!isNew.value) {
        await taxonStore.fetchTaxonById(taxonId.value);
        if (taxonStore.selectedTaxon) {
            taxon.value = { ...taxonStore.selectedTaxon };
        }
    } else if (targetTaxonomyId.value) {
        taxon.value.taxonomyId = targetTaxonomyId.value;
    }
});

async function onSave() {
    let success = false;
    if (isNew.value) {
        success = await taxonStore.createTaxon(taxon.value);
    } else {
        success = await taxonStore.updateTaxon(taxonId.value, taxon.value);
    }

    if (success) {
        toast.add({ severity: 'success', summary: 'Success', detail: 'Taxon saved successfully', life: 3000 });
        router.back();
    }
}
</script>

<template>
    <div class="card">
        <div class="flex items-center justify-between mb-6">
            <div>
                <div class="text-3xl font-medium mb-2">{{ isNew ? 'Create Taxon' : 'Edit Taxon' }}</div>
                <div class="text-muted-color">Manage hierarchical node within a taxonomy</div>
            </div>
            <div class="flex gap-2">
                <Button label="Cancel" severity="secondary" outlined @click="router.back()" />
                <Button label="Save" icon="pi pi-check" @click="onSave" :loading="taxonStore.loading" />
            </div>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div class="flex flex-col gap-2">
                <label class="font-bold">Taxonomy</label>
                <Select v-model="taxon.taxonomyId" :options="taxonomyStore.selectTaxonomies?.items" optionLabel="presentation" optionValue="id" placeholder="Select Taxonomy" :disabled="!isNew" />
            </div>
            
            <div class="flex flex-col gap-2">
                <label class="font-bold">Parent Taxon</label>
                <InputText v-model="taxon.parentId" placeholder="Parent ID (Optional)" />
            </div>

            <div class="flex flex-col gap-2">
                <label class="font-bold">Internal Name</label>
                <InputText v-model="taxon.name" />
            </div>

            <div class="flex flex-col gap-2">
                <label class="font-bold">Display Name</label>
                <InputText v-model="taxon.presentation" />
            </div>

            <div class="flex flex-col gap-2 md:col-span-2">
                <label class="font-bold">Description</label>
                <Textarea v-model="taxon.description" rows="3" autoResize />
            </div>

            <div class="flex items-center gap-2">
                <ToggleSwitch v-model="taxon.hideFromNav" />
                <label class="font-bold">Hide from Navigation</label>
            </div>

            <div class="flex items-center gap-2">
                <ToggleSwitch v-model="taxon.automatic" />
                <label class="font-bold">Automatic Product Assignment</label>
            </div>

            <template v-if="taxon.automatic">
                <div class="flex flex-col gap-2">
                    <label class="font-bold">Rules Match Policy</label>
                    <Select v-model="taxon.rulesMatchPolicy" :options="matchPolicies" optionLabel="label" optionValue="value" />
                </div>
                <div class="flex flex-col gap-2">
                    <label class="font-bold">Sort Order</label>
                    <Select v-model="taxon.sortOrder" :options="sortOrders" optionLabel="label" optionValue="value" />
                </div>
            </template>
        </div>
    </div>
</template>
