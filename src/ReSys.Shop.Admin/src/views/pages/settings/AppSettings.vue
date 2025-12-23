<script setup>
import { ref, onMounted } from 'vue';
import { useSettingStore } from '@/stores';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import Toolbar from 'primevue/toolbar';
import Dialog from 'primevue/dialog';
import InputText from 'primevue/inputtext';
import Textarea from 'primevue/textarea';
import Select from 'primevue/select';
import { useToast } from 'primevue/usetoast';
import MetadataEditor from '@/components/MetadataEditor.vue';

const store = useSettingStore();
const toast = useToast();

const showDialog = ref(false);
const submitted = ref(false);
const setting = ref({
    key: '',
    value: '',
    description: '',
    defaultValue: '',
    valueType: 'String',
    publicMetadata: {},
    privateMetadata: {}
});

const typeOptions = ['String', 'Number', 'Boolean', 'Json'];

onMounted(async () => {
    await store.fetchPagedSettings();
});

function openNew() {
    setting.value = {
        valueType: 'String',
        publicMetadata: {},
        privateMetadata: {}
    };
    submitted.value = false;
    showDialog.value = true;
}

function editSetting(data) {
    setting.value = { ...data };
    showDialog.value = true;
}

async function saveSetting() {
    submitted.value = true;
    if (setting.value.key && setting.value.value) {
        let success = false;
        if (setting.value.id) {
            success = await store.updateSetting(setting.value.id, setting.value);
        } else {
            success = await store.createSetting(setting.value);
        }

        if (success) {
            toast.add({ severity: 'success', summary: 'Success', detail: 'Setting saved', life: 3000 });
            showDialog.value = false;
        }
    }
}
</script>

<template>
    <div class="card">
        <Toolbar class="mb-4">
            <template #start>
                <Button label="New Setting" icon="pi pi-plus" class="mr-2" @click="openNew" />
                <Button label="Refresh" icon="pi pi-refresh" severity="secondary" @click="store.fetchPagedSettings" :loading="store.loading" />
            </template>
        </Toolbar>

        <DataTable :value="store.settings" :loading="store.loading" paginator :rows="15">
            <Column field="key" header="Key" sortable>
                <template #body="slotProps">
                    <code class="font-bold text-primary">{{ slotProps.data.key }}</code>
                </template>
            </Column>
            <Column field="value" header="Value">
                <template #body="slotProps">
                    <span class="truncate max-w-[20rem] block">{{ slotProps.data.value }}</span>
                </template>
            </Column>
            <Column field="valueType" header="Type"></Column>
            <Column field="description" header="Description"></Column>
            <Column header="Actions">
                <template #body="slotProps">
                    <Button icon="pi pi-pencil" outlined rounded class="mr-2" @click="editSetting(slotProps.data)" />
                    <Button icon="pi pi-trash" outlined rounded severity="danger" />
                </template>
            </Column>
        </DataTable>

        <Dialog v-model:visible="showDialog" :header="setting.id ? 'Edit Setting' : 'New Setting'" modal class="p-fluid w-140">
            <div class="flex flex-col gap-4">
                <div class="flex flex-col gap-2">
                    <label class="font-bold">Key</label>
                    <InputText v-model.trim="setting.key" required :disabled="!!setting.id" :class="{ 'p-invalid': submitted && !setting.key }" />
                </div>
                <div class="flex flex-col gap-2">
                    <label class="font-bold">Value</label>
                    <Textarea v-model="setting.value" rows="3" required :class="{ 'p-invalid': submitted && !setting.value }" />
                </div>
                <div class="flex flex-col gap-2">
                    <label class="font-bold">Value Type</label>
                    <Select v-model="setting.valueType" :options="typeOptions" />
                </div>
                <div class="flex flex-col gap-2">
                    <label class="font-bold">Default Value</label>
                    <InputText v-model="setting.defaultValue" />
                </div>
                <div class="flex flex-col gap-2">
                    <label class="font-bold">Description</label>
                    <Textarea v-model="setting.description" rows="2" />
                </div>

                <div class="flex flex-col gap-6 mt-4">
                    <MetadataEditor
                        v-model="setting.publicMetadata"
                        title="Public Metadata"
                        helpText="Extended configuration for specific application modules."
                    />

                    <MetadataEditor
                        v-model="setting.privateMetadata"
                        title="Private Metadata"
                        helpText="Sensitive flags or internal-only system overrides."
                    />
                </div>
            </div>
            <template #footer>
                <Button label="Cancel" text @click="showDialog = false" />
                <Button label="Save" icon="pi pi-check" @click="saveSetting" :loading="store.loading" />
            </template>
        </Dialog>
    </div>
</template>
