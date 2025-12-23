<script setup>
import { ref, onMounted } from 'vue';
import { useTaxonomyStore } from '@/stores';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import Toolbar from 'primevue/toolbar';
import { useRouter } from 'vue-router';

const taxonomyStore = useTaxonomyStore();
const router = useRouter();

onMounted(async () => {
    await taxonomyStore.fetchPagedTaxonomies();
});

const viewTaxonomy = (id) => {
    router.push({ name: 'admin-taxonomy-detail', params: { id } });
};

const createTaxonomy = () => {
    router.push({ name: 'admin-taxonomy-detail', params: { id: 'new' } });
};

const onRefresh = async () => {
    await taxonomyStore.fetchPagedTaxonomies();
};
</script>

<template>
    <div class="card">
        <Toolbar class="mb-4">
            <template #start>
                <Button label="New Taxonomy" icon="pi pi-plus" class="mr-2" @click="createTaxonomy"/>
                <Button label="Refresh" icon="pi pi-refresh" severity="secondary" @click="onRefresh" :loading="taxonomyStore.loading" />
            </template>
        </Toolbar>

        <DataTable
            :value="taxonomyStore.taxonomies"
            :loading="taxonomyStore.loading"
            :paginator="true"
            :rows="10"
            dataKey="id"
            responsiveLayout="scroll"
        >
            <Column field="name" header="Name" sortable></Column>
            <Column field="presentation" header="Presentation" sortable></Column>
            <Column field="taxonCount" header="Taxons" sortable></Column>
            <Column field="position" header="Position" sortable></Column>
            <Column header="Actions">
                <template #body="slotProps">
                    <Button icon="pi pi-pencil" outlined rounded class="mr-2" @click="viewTaxonomy(slotProps.data.id)" />
                    <Button icon="pi pi-trash" outlined rounded severity="danger" />
                </template>
            </Column>
        </DataTable>
    </div>
</template>
