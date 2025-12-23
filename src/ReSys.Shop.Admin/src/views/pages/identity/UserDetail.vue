<script setup>
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useUserStore } from '@/stores';
import { useRoleStore } from '@/stores';
import { useToast } from 'primevue/usetoast';

import Tabs from 'primevue/tabs';
import TabList from 'primevue/tablist';
import Tab from 'primevue/tab';
import TabPanels from 'primevue/tabpanels';
import TabPanel from 'primevue/tabpanel';
import Button from 'primevue/button';
import InputText from 'primevue/inputtext';
import ToggleSwitch from 'primevue/toggleswitch';
import PickList from 'primevue/picklist';
import MetadataEditor from '@/components/MetadataEditor.vue';

const route = useRoute();
const router = useRouter();
const userStore = useUserStore();
const roleStore = useRoleStore();
const toast = useToast();

const isNew = computed(() => route.params.id === 'new');
const userId = ref(route.params.id);

const user = ref({
    email: '',
    userName: '',
    password: '',
    firstName: '',
    lastName: '',
    emailConfirmed: true,
    phoneNumberConfirmed: false,
    publicMetadata: {},
    privateMetadata: {}
});

const userRoles = ref([[], []]); // [Available, Assigned]

onMounted(async () => {
    if (!isNew.value) {
        await userStore.fetchUserById(userId.value);
        if (userStore.selectedUser) {
            user.value = { ...userStore.selectedUser };
            await loadRoles();
        }
    }
});

async function loadRoles() {
    await roleStore.fetchSelectRoles();
    await userStore.fetchUserRoles(userId.value);
    
    const assignedIds = new Set(userStore.userRoles?.map(r => r.id) || []);
    const available = roleStore.selectRoles?.items.filter(r => !assignedIds.has(r.id)) || [];
    const assigned = roleStore.selectRoles?.items.filter(r => assignedIds.has(r.id)) || [];
    
    userRoles.value = [available, assigned];
}

async function onSave() {
    let success = false;
    if (isNew.value) {
        success = await userStore.createUser(user.value);
    } else {
        success = await userStore.updateUser(userId.value, user.value);
    }

    if (success) {
        toast.add({ severity: 'success', summary: 'Success', detail: 'User updated successfully', life: 3000 });
        if (isNew.value) router.push('/identity/users');
    }
}

async function moveRoleToTarget(event) {
    for (const role of event.items) {
        await userStore.assignRoleToUser(userId.value, { roleName: role.name });
    }
}

async function moveRoleFromTarget(event) {
    for (const role of event.items) {
        await userStore.unassignRoleFromUser(userId.value, { roleName: role.name });
    }
}
</script>

<template>
    <div class="card">
        <div class="flex items-center justify-between mb-6">
            <div>
                <div class="text-3xl font-medium mb-2">{{ isNew ? 'Create User' : 'Edit User' }}</div>
                <div class="text-muted-color">Manage account details and access levels</div>
            </div>
            <div class="flex gap-2">
                <Button label="Cancel" severity="secondary" outlined @click="router.push('/identity/users')" />
                <Button label="Save User" icon="pi pi-check" @click="onSave" :loading="userStore.loading" />
            </div>
        </div>

        <Tabs value="0">
            <TabList>
                <Tab value="0">Profile</Tab>
                <Tab value="1" v-if="!isNew">Roles</Tab>
                <Tab value="2" v-if="!isNew">Direct Permissions</Tab>
            </TabList>
            <TabPanels>
                <TabPanel value="0">
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-6 pt-4">
                        <div class="flex flex-col gap-2">
                            <label class="font-bold">Email</label>
                            <InputText v-model="user.email" type="email" />
                        </div>
                        <div class="flex flex-col gap-2">
                            <label class="font-bold">Username</label>
                            <InputText v-model="user.userName" />
                        </div>
                        <div class="flex flex-col gap-2" v-if="isNew">
                            <label class="font-bold">Initial Password</label>
                            <InputText v-model="user.password" type="password" />
                        </div>
                        <div class="flex flex-col gap-2">
                            <label class="font-bold">First Name</label>
                            <InputText v-model="user.firstName" />
                        </div>
                        <div class="flex flex-col gap-2">
                            <label class="font-bold">Last Name</label>
                            <InputText v-model="user.lastName" />
                        </div>
                        <div class="flex items-center gap-4 mt-4">
                            <div class="flex items-center gap-2">
                                <ToggleSwitch v-model="user.emailConfirmed" />
                                <label>Email Verified</label>
                            </div>
                        </div>
                    </div>

                    <div class="flex flex-col gap-6 mt-8">
                        <MetadataEditor 
                            v-model="user.publicMetadata" 
                            title="Public User Metadata" 
                            helpText="Non-sensitive attributes (e.g. preferences, custom_labels)." 
                        />
                        <MetadataEditor 
                            v-model="user.privateMetadata" 
                            title="Private User Metadata" 
                            helpText="Internal flags or integration mapping IDs." 
                        />
                    </div>
                </TabPanel>

                <TabPanel value="1" v-if="!isNew">
                    <PickList v-model="userRoles" dataKey="id" breakpoint="1400px" 
                              @moveToTarget="moveRoleToTarget" @moveFromTarget="moveRoleFromTarget">
                        <template #sourceheader> Available Roles </template>
                        <template #targetheader> Assigned Roles </template>
                        <template #item="slotProps">
                            <div class="flex flex-wrap p-2 items-center gap-3">
                                <div class="flex-1 flex flex-col">
                                    <span class="font-bold">{{ slotProps.item.presentation || slotProps.item.name }}</span>
                                    <small class="text-muted-color">{{ slotProps.item.name }}</small>
                                </div>
                            </div>
                        </template>
                    </PickList>
                </TabPanel>

                <TabPanel value="2" v-if="!isNew">
                    <div class="flex flex-col gap-4">
                        <div class="p-4 bg-orange-50 dark:bg-orange-900/20 text-orange-700 dark:text-orange-300 rounded-lg flex items-start gap-3">
                            <i class="pi pi-exclamation-triangle mt-1"></i>
                            <div>
                                <div class="font-bold mb-1">Direct Permission Overrides</div>
                                <p class="m-0 text-sm">Directly assigned permissions (claims) will be added to the user in addition to their roles. Use with caution.</p>
                            </div>
                        </div>
                        <MetadataEditor 
                            v-model="user.directPermissions" 
                            title="Assigned Claims" 
                            helpText="Key = Permission Name, Value = Allowed/Denied." 
                        />
                    </div>
                </TabPanel>
            </TabPanels>
        </Tabs>
    </div>
</template>
