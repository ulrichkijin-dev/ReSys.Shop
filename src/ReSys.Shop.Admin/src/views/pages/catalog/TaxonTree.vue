<script setup>
import { ref, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useTaxonStore } from '@/stores';
import { useTaxonomyStore } from '@/stores';
import { useToast } from 'primevue/usetoast';
import Tree from 'primevue/tree';
import Button from 'primevue/button';

const route = useRoute();
const router = useRouter();
const taxonStore = useTaxonStore();
const taxonomyStore = useTaxonomyStore();
const toast = useToast();

const taxonomyId = route.params.id;
const treeData = ref([]);

onMounted(async () => {
    await taxonomyStore.fetchTaxonomyById(taxonomyId);
    await loadTree();
});

async function loadTree() {
    await taxonStore.fetchTaxonTree({ taxonomyId: [taxonomyId] });
    if (taxonStore.taxonTree) {
        // Transform backend TreeNodeItem to PrimeVue Tree nodes
        treeData.value = transformToPrimeTree(taxonStore.taxonTree.tree);
    }
}

function transformToPrimeTree(nodes) {
    return nodes.map(node => ({
        key: node.id,
        label: node.presentation,
        data: node,
        icon: 'pi pi-fw pi-folder',
        children: node.children ? transformToPrimeTree(node.children) : []
    }));
}

async function rebuildHierarchy() {
    const success = await taxonStore.rebuildTaxonHierarchy(taxonomyId);
    if (success) {
        toast.add({ severity: 'success', summary: 'Success', detail: 'Hierarchy rebuilt successfully', life: 3000 });
        await loadTree();
    }
}
</script>

<template>
    <div class="card">
        <div class="flex items-center justify-between mb-6">
            <div>
                <div class="text-3xl font-medium mb-2">Hierarchy Builder</div>
                <div class="text-muted-color">Visual structure for {{ taxonomyStore.selectedTaxonomy?.presentation }}</div>
            </div>
            <div class="flex gap-2">
                <Button label="Back" icon="pi pi-arrow-left" severity="secondary" outlined @click="router.back()" />
                <Button label="Rebuild & Sync" icon="pi pi-refresh" severity="warning" @click="rebuildHierarchy" :loading="taxonStore.loading" />
            </div>
        </div>

        <div class="p-4 border rounded-lg bg-surface-50 dark:bg-surface-950">
            <Tree :value="treeData" class="w-full md:w-[30rem] border-none bg-transparent" />
            
            <div v-if="treeData.length === 0 && !taxonStore.loading" class="text-center py-8 text-muted-color">
                No hierarchical data found.
            </div>
        </div>
        
        <div class="mt-4 p-4 bg-blue-50 dark:bg-blue-900/20 text-blue-700 dark:text-blue-300 rounded-lg flex items-start gap-3">
            <i class="pi pi-info-circle mt-1"></i>
            <div>
                <div class="font-bold mb-1">Hierarchy Management</div>
                <p class="m-0 text-sm">The tree above represents the nested set structure. Use "Rebuild & Sync" after manual changes to ensure all permalinks and positions are calculated correctly by the server.</p>
            </div>
        </div>
    </div>
</template>
