<script setup>
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useRoleStore } from '@/stores';
import { usePermissionStore } from '@/stores';
import { useToast } from 'primevue/usetoast';

import Tabs from 'primevue/tabs';
import TabList from 'primevue/tablist';
import Tab from 'primevue/tab';
import TabPanels from 'primevue/tabpanels';
import TabPanel from 'primevue/tabpanel';
import Button from 'primevue/button';
import InputText from 'primevue/inputtext';
import PickList from 'primevue/picklist';
import MetadataEditor from '@/components/MetadataEditor.vue';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';

const route = useRoute();
const router = useRouter();
const roleStore = useRoleStore();
const permissionStore = usePermissionStore();
const toast = useToast();

const isNew = computed(() => route.params.id === 'new');
const roleId = ref(route.params.id);

const role = ref({
    name: '',
    displayName: '',
    description: '',
    priority: 1,
    isSystemRole: false,
    publicMetadata: {},
    privateMetadata: {}
});

const rolePermissions = ref([[], []]); // [Available, Assigned]

onMounted(async () => {
    if (!isNew.value) {
        await roleStore.fetchRoleById(roleId.value);
        if (roleStore.selectedRole) {
            role.value = { ...roleStore.selectedRole };
            await loadPermissions();
        }
    }
});

async function loadPermissions() {
    await permissionStore.fetchPagedPermissions({ pageSize: 1000 }); // Load all for picklist
    await roleStore.fetchRolePermissions(roleId.value);
    
    const assignedNames = new Set(roleStore.rolePermissions?.map(p => p.name) || []);
    const available = permissionStore.permissions.filter(p => !assignedNames.has(p.name)) || [];
    const assigned = permissionStore.permissions.filter(p => assignedNames.has(p.name)) || [];
    
    rolePermissions.value = [available, assigned];
}

async function onSave() {
    let success = false;
    if (isNew.value) {
        success = await roleStore.createRole(role.value);
    } else {
        success = await roleStore.updateRole(roleId.value, role.value);
    }

    if (success) {
        toast.add({ severity: 'success', summary: 'Success', detail: 'Role saved successfully', life: 3000 });
        if (isNew.value) router.push('/identity/roles');
    }
}

async function movePermissionToTarget(event) {
    for (const p of event.items) {
        await roleStore.assignPermissionsToRole(roleId.value, { claimValue: p.value });
    }
}

async function movePermissionFromTarget(event) {
    for (const p of event.items) {
        await roleStore.unassignPermissionsFromRole(roleId.value, { claimValue: p.value });
    }
}
</script>

<template>
    <div class="card">
        <div class="flex items-center justify-between mb-6">
            <div class="flex items-center gap-4">
                <Button icon="pi pi-arrow-left" severity="secondary" rounded outlined @click="router.push('/identity/roles')" />
                <div>
                    <div class="text-3xl font-medium mb-1">{{ isNew ? 'Create Role' : 'Edit Role' }}</div>
                    <div class="text-muted-color">Define authority and system access level</div>
                </div>
            </div>
            <div class="flex gap-2">
                <Button label="Save Role" icon="pi pi-check" @click="onSave" :loading="roleStore.loading" />
            </div>
        </div>

        <Tabs value="0">
            <TabList>
                <Tab value="0">Settings</Tab>
                <Tab value="1" v-if="!isNew">Permissions</Tab>
                <Tab value="2" v-if="!isNew">Users in Role</Tab>
            </TabList>
            <TabPanels>
                <TabPanel value="0">
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-6 pt-4">
                        <div class="flex flex-col gap-2">
                            <label class="font-bold">System Name</label>
                            <InputText v-model="role.name" :disabled="!isNew" placeholder="e.g. Order.Manager" />
                        </div>
                        <div class="flex flex-col gap-2">
                            <label class="font-bold">Display Name</label>
                            <InputText v-model="role.displayName" placeholder="e.g. Order Manager" />
                        </div>
                        <div class="flex flex-col gap-2 md:col-span-2">
                            <label class="font-bold">Description</label>
                            <InputText v-model="role.description" />
                        </div>
                    </div>

                    <div class="flex flex-col gap-6 mt-8">
                        <MetadataEditor 
                            v-model="role.publicMetadata" 
                            title="Public Role Metadata" 
                            helpText="UI hints or categorization for the storefront." 
                        />
                        <MetadataEditor 
                            v-model="role.privateMetadata" 
                            title="Private Role Metadata" 
                            helpText="Internal technical mapping or security flags." 
                        />
                    </div>
                </TabPanel>

                <TabPanel value="1" v-if="!isNew">
                    <PickList v-model="rolePermissions" dataKey="id" breakpoint="1400px" 
                              @moveToTarget="movePermissionToTarget" @moveFromTarget="movePermissionFromTarget">
                        <template #sourceheader> Available Permissions </template>
                        <template #targetheader> Granted to Role </template>
                        <template #item="slotProps">
                            <div class="flex flex-wrap p-2 items-center gap-3">
                                <div class="flex-1 flex flex-col">
                                    <span class="font-bold">{{ slotProps.item.displayName || slotProps.item.name }}</span>
                                    <small class="text-muted-color font-mono">{{ slotProps.item.value }}</small>
                                </div>
                            </div>
                        </template>
                    </PickList>
                </TabPanel>

                <TabPanel value="2" v-if="!isNew">
                    <div class="flex justify-between items-center mb-4">
                        <h5 class="m-0 font-bold">Users Assigned to this Role</h5>
                        <Button label="Refresh List" icon="pi pi-refresh" severity="secondary" text size="small" @click="roleStore.fetchRoleUsers(roleId)" />
                    </div>
                    
                    <DataTable :value="roleStore.roleUsers?.items" :loading="roleStore.loading" paginator :rows="10">
                        <template #empty> No users currently assigned to this role. </template>
                        <Column field="userName" header="Username"></Column>
                        <Column field="fullName" header="Full Name"></Column>
                        <Column header="Actions">
                            <template #body="slotProps">
                                <Button icon="pi pi-user-minus" severity="danger" text rounded title="Unassign User" />
                            </template>
                        </Column>
                    </DataTable>
                </TabPanel>
            </TabPanels>
        </Tabs>
    </div>
</template>
