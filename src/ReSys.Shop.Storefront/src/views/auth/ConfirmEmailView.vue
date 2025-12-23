<template>
  <div class="max-w-md mx-auto py-20 text-center px-4">
    <Card>
      <template #content>
        <div v-if="loading" class="py-12">
          <ProgressSpinner />
          <p class="mt-4">Confirming your email...</p>
        </div>
        <div v-else class="py-12">
          <i :class="success ? 'pi pi-check-circle text-green-500' : 'pi pi-times-circle text-red-500'" class="text-6xl mb-6"></i>
          <h1 class="text-2xl font-bold mb-4">{{ success ? 'Success!' : 'Verification Failed' }}</h1>
          <p class="text-surface-600 mb-8">{{ message }}</p>
          <router-link to="/login">
            <Button :label="success ? 'Continue to Login' : 'Back to Login'" />
          </router-link>
        </div>
      </template>
    </Card>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import { emailService } from '@/services/accounts/email/email.service'; // I'll ensure this exists
import Card from 'primevue/card';
import Button from 'primevue/button';
import ProgressSpinner from 'primevue/progressspinner';

const route = useRoute();
const loading = ref(true);
const success = ref(false);
const message = ref('');

onMounted(async () => {
  const { userId, code, changedEmail } = route.query;
  try {
    const res = await emailService.confirm({ userId, code, changedEmail });
    success.value = true;
    message.value = res.data.confirmMessage;
  } catch (err) {
    success.value = false;
    message.value = err.response?.data?.message || 'The verification link is invalid or has expired.';
  } finally {
    loading.value = false;
  }
});
</script>
