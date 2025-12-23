<script setup>
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useProductStore } from '@/stores';
import { useToast } from 'primevue/usetoast';

import Tabs from 'primevue/tabs';
import TabList from 'primevue/tablist';
import Tab from 'primevue/tab';
import TabPanels from 'primevue/tabpanels';
import TabPanel from 'primevue/tabpanel';
import Button from 'primevue/button';
import InputText from 'primevue/inputtext';
import Textarea from 'primevue/textarea';
import Select from 'primevue/select';
import DatePicker from 'primevue/datepicker';
import Checkbox from 'primevue/checkbox';
import ToggleSwitch from 'primevue/toggleswitch';

import ProductImageManager from './components/ProductImageManager.vue';
import ProductVariantManager from './components/ProductVariantManager.vue';
import ProductClassificationManager from './components/ProductClassificationManager.vue';
import ProductPropertyManager from './components/ProductPropertyManager.vue';

const route = useRoute();
const router = useRouter();
const productStore = useProductStore();
const toast = useToast();

const isNew = computed(() => route.params.id === 'new');
const productId = ref(route.params.id);

const statusOptions = ref([
    { label: 'Active', value: 'Active' },
    { label: 'Draft', value: 'Draft' },
    { label: 'Archived', value: 'Archived' }
]);

const product = ref({
    name: '',
    presentation: '',
    description: '',
    slug: '',
    status: 'Draft',
    availableOn: null,
    makeActiveAt: null,
    discontinueOn: null,
    isDigital: false,
    metaTitle: '',
    metaDescription: '',
    metaKeywords: ''
});

onMounted(async () => {
    if (!isNew.value) {
        await productStore.fetchProductById(productId.value);
        if (productStore.selectedProduct) {
            product.value = { ...productStore.selectedProduct };
        }
    }
});

async function onSave() {
    let success = false;
    if (isNew.value) {
        success = await productStore.createProduct(product.value);
    } else {
        success = await productStore.updateProduct(productId.value, product.value);
    }

    if (success) {
        toast.add({ severity: 'success', summary: 'Success', detail: 'Product saved successfully', life: 3000 });
        if (isNew.value) router.push('/catalog/products');
    }
}

function onCancel() {
    router.push('/catalog/products');
}
</script>

<template>
    <div class="card">
        <div class="flex items-center justify-between mb-6">
            <div>
                <div class="text-3xl font-medium text-surface-900 dark:text-surface-0 mb-2">
                    {{ isNew ? 'Create Product' : 'Edit Product' }}
                </div>
                <div class="text-muted-color">{{ isNew ? 'Add a new product to your catalog' : `Managing: ${product.name}` }}</div>
            </div>
            <div class="flex gap-2">
                <Button label="Cancel" severity="secondary" outlined @click="onCancel" />
                <Button label="Save Product" icon="pi pi-check" @click="onSave" :loading="productStore.loading" />
            </div>
        </div>

        <Tabs value="0">
            <TabList>
                <Tab value="0">General Info</Tab>
                <Tab value="1">Images</Tab>
                <Tab value="2">Variants & Pricing</Tab>
                <Tab value="3">Classifications</Tab>
                <Tab value="4">Properties</Tab>
                <Tab value="5">SEO</Tab>
            </TabList>
            <TabPanels>
                <!-- General Info -->
                <TabPanel value="0">
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-6 pt-4">
                        <div class="flex flex-col gap-2">
                            <label for="name" class="font-bold">Name</label>
                            <InputText id="name" v-model="product.name" placeholder="Enter internal name" />
                        </div>
                        <div class="flex flex-col gap-2">
                            <label for="presentation" class="font-bold">Display Name</label>
                            <InputText id="presentation" v-model="product.presentation" placeholder="Customer facing name" />
                        </div>
                        <div class="flex flex-col gap-2 md:col-span-2">
                            <label for="slug" class="font-bold">URL Slug</label>
                            <InputText id="slug" v-model="product.slug" placeholder="product-url-identifier" />
                        </div>
                        <div class="flex flex-col gap-2 md:col-span-2">
                            <label for="description" class="font-bold">Description</label>
                            <Textarea id="description" v-model="product.description" rows="5" autoResize />
                        </div>
                        <div class="flex flex-col gap-2">
                            <label for="status" class="font-bold">Status</label>
                            <Select id="status" v-model="product.status" :options="statusOptions" optionLabel="label" optionValue="value" />
                        </div>
                        <div class="flex items-center gap-2 mt-8">
                            <ToggleSwitch v-model="product.isDigital" />
                            <label class="font-bold">Is Digital Product</label>
                        </div>
                    </div>
                </TabPanel>

                <!-- Images -->
                <TabPanel value="1">
                    <div v-if="isNew" class="flex flex-col items-center justify-center p-12 bg-surface-50 dark:bg-surface-950 rounded-lg">
                        <i class="pi pi-info-circle text-4xl mb-4 text-blue-500"></i>
                        <p>Images can be uploaded after saving the product for the first time.</p>
                    </div>
                    <ProductImageManager v-else :productId="productId" />
                </TabPanel>

                <!-- Variants & Pricing -->
                <TabPanel value="2">
                    <div v-if="isNew" class="flex flex-col items-center justify-center p-12 bg-surface-50 dark:bg-surface-950 rounded-lg">
                        <i class="pi pi-info-circle text-4xl mb-4 text-blue-500"></i>
                        <p>Variants can be managed after saving the product for the first time.</p>
                    </div>
                    <ProductVariantManager v-else :productId="productId" />
                </TabPanel>

                <!-- Classifications -->
                <TabPanel value="3">
                    <div v-if="isNew" class="flex flex-col items-center justify-center p-12 bg-surface-50 dark:bg-surface-950 rounded-lg">
                        <i class="pi pi-info-circle text-4xl mb-4 text-blue-500"></i>
                        <p>Classifications can be managed after saving the product for the first time.</p>
                    </div>
                    <ProductClassificationManager v-else :productId="productId" />
                </TabPanel>

                <!-- Properties -->
                <TabPanel value="4">
                    <div v-if="isNew" class="flex flex-col items-center justify-center p-12 bg-surface-50 dark:bg-surface-950 rounded-lg">
                        <i class="pi pi-info-circle text-4xl mb-4 text-blue-500"></i>
                        <p>Properties can be managed after saving the product for the first time.</p>
                    </div>
                    <ProductPropertyManager v-else :productId="productId" />
                </TabPanel>

                <!-- SEO -->
                <TabPanel value="5">
                    <div class="flex flex-col gap-6 pt-4">
                        <div class="flex flex-col gap-2">
                            <label class="font-bold">Meta Title</label>
                            <InputText v-model="product.metaTitle" />
                        </div>
                        <div class="flex flex-col gap-2">
                            <label class="font-bold">Meta Description</label>
                            <Textarea v-model="product.metaDescription" rows="3" />
                        </div>
                        <div class="flex flex-col gap-2">
                            <label class="font-bold">Meta Keywords</label>
                            <InputText v-model="product.metaKeywords" placeholder="comma, separated, keywords" />
                        </div>
                    </div>
                </TabPanel>
            </TabPanels>
        </Tabs>
    </div>
</template>
