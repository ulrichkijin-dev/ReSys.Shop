<script setup>
import { useAuthStore } from '@/stores';
import { useProfileStore } from '@/stores';
import { onMounted } from 'vue';
import ProgressSpinner from 'primevue/progressspinner';

const profileStore = useProfileStore();

onMounted(async () => {
    await profileStore.fetchProfile();
});
</script>

<template>
    <div class="card">
        <div class="font-semibold text-xl mb-4">User Profile</div>
        <div v-if="profileStore.loading" class="flex justify-center py-8">
            <ProgressSpinner />
        </div>
        <div v-else-if="profileStore.profile" class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div class="flex flex-col gap-2">
                <label class="font-bold">Username</label>
                <div class="p-2 border rounded bg-surface-50 dark:bg-surface-950">{{ profileStore.profile.username }}</div>
            </div>
            <div class="flex flex-col gap-2">
                <label class="font-bold">Email</label>
                <div class="p-2 border rounded bg-surface-50 dark:bg-surface-950">{{ profileStore.profile.email }}</div>
            </div>
            <div class="flex flex-col gap-2">
                <label class="font-bold">First Name</label>
                <div class="p-2 border rounded bg-surface-50 dark:bg-surface-950">{{ profileStore.profile.firstName || 'N/A' }}</div>
            </div>
            <div class="flex flex-col gap-2">
                <label class="font-bold">Last Name</label>
                <div class="p-2 border rounded bg-surface-50 dark:bg-surface-950">{{ profileStore.profile.lastName || 'N/A' }}</div>
            </div>
            <div class="flex flex-col gap-2">
                <label class="font-bold">Phone Number</label>
                <div class="p-2 border rounded bg-surface-50 dark:bg-surface-950">{{ profileStore.profile.phoneNumber || 'N/A' }}</div>
            </div>
            <div class="flex flex-col gap-2">
                <label class="font-bold">Last Sign In</label>
                <div class="p-2 border rounded bg-surface-50 dark:bg-surface-950">{{ profileStore.profile.lastSignInAt ? new Date(profileStore.profile.lastSignInAt).toLocaleString() : 'Never' }}</div>
            </div>
        </div>
    </div>
</template>
