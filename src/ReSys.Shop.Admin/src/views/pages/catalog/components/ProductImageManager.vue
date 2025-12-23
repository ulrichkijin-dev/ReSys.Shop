<script setup>
import { ref, onMounted } from 'vue';
import { useProductImageStore } from '@/stores';
import FileUpload from 'primevue/fileupload';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import Image from 'primevue/image';
import Tag from 'primevue/tag';

const props = defineProps({
    productId: {
        type: String,
        required: true
    }
});

const imageStore = useProductImageStore();

onMounted(async () => {
    await imageStore.fetchProductImages({ productId: [props.productId] });
});

async function onUpload(event) {
    for (let file of event.files) {
        const formData = new FormData();
        formData.append('file', file);
        formData.append('type', 'default');
        formData.append('position', '0');
        await imageStore.uploadProductImage(props.productId, formData);
    }
    await imageStore.fetchProductImages({ productId: [props.productId] });
}

async function removeImage(imageId) {
    const success = await imageStore.deleteProductImage(props.productId, imageId);
    if (success) {
        await imageStore.fetchProductImages({ productId: [props.productId] });
    }
}
</script>

<template>
    <div class="pt-4">
        <FileUpload 
            name="file" 
            mode="advanced" 
            multiple 
            accept="image/*" 
            :maxFileSize="1000000" 
            customUpload 
            @uploader="onUpload"
            class="mb-4"
        >
            <template #empty>
                <p>Drag and drop images here to upload.</p>
            </template>
        </FileUpload>

        <DataTable :value="imageStore.productImages" responsiveLayout="scroll">
            <Column header="Image">
                <template #body="slotProps">
                    <Image :src="slotProps.data.url" :alt="slotProps.data.alt" width="100" preview class="shadow-md rounded" />
                </template>
            </Column>
            <Column field="type" header="Type">
                <template #body="slotProps">
                    <Tag :value="slotProps.data.type" severity="info" />
                </template>
            </Column>
            <Column field="position" header="Order"></Column>
            <Column header="Actions">
                <template #body="slotProps">
                    <Button icon="pi pi-trash" severity="danger" outlined rounded @click="removeImage(slotProps.data.id)" />
                </template>
            </Column>
        </DataTable>
    </div>
</template>
