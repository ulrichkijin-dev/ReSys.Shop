<template>
  <div class="max-w-md mx-auto py-20 px-4">
    <Card>
      <template #title> Reset Password </template>
      <template #content>
        <form @submit.prevent="handleSubmit" class="flex flex-col gap-4">
          <div class="flex flex-col gap-2">
            <label for="newPassword">New Password</label>
            <Password id="newPassword" v-model="form.newPassword" toggleMask required />
          </div>
          <div class="flex flex-col gap-2">
            <label for="confirmPassword">Confirm Password</label>
            <Password id="confirmPassword" v-model="confirmPassword" :feedback="false" toggleMask required />
          </div>
          <Button type="submit" label="Update Password" :loading="loading" class="mt-4" />
        </form>
      </template>
    </Card>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { passwordService } from '@/services/accounts/password/password.service';
import Card from 'primevue/card';
import Password from 'primevue/password';
import Button from 'primevue/button';

const route = useRoute();
const router = useRouter();
const loading = ref(false);
const confirmPassword = ref('');

const form = ref({
  email: route.query.email || '',
  resetCode: route.query.code || '',
  newPassword: ''
});

const handleSubmit = async () => {
  if (form.value.newPassword !== confirmPassword.value) {
    alert("Passwords do not match");
    return;
  }

  loading.value = true;
  try {
    await passwordService.reset(form.value);
    router.push('/login?reset=success');
  } catch (err) {
    alert(err.response?.data?.message || "Reset failed");
  } finally {
    loading.value = false;
  }
};
</script>
