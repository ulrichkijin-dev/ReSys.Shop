<template>
  <div class="max-w-md mx-auto py-12">
    <Card>
      <template #title> Login to ReSys.Shop </template>
      <template #content>
        <Message v-if="$route.query.registered" severity="success" class="mb-4">Registration successful! Please login.</Message>
        <Message v-if="$route.query.message === 'loginRequired'" severity="warn" class="mb-4">You need to login to access that page.</Message>
        
        <form @submit.prevent="handleLogin" class="flex flex-col gap-4">
          <div class="flex flex-col gap-2">
            <label for="credential">Email or Username</label>
            <InputText id="credential" v-model="form.credential" required />
          </div>
          <div class="flex flex-col gap-2">
            <label for="password">Password</label>
            <Password id="password" v-model="form.password" :feedback="false" toggleMask required />
          </div>
          <Button type="submit" label="Login" :loading="loading" class="mt-4" />
          
          <p class="text-center text-sm text-surface-600 mt-4">
            Don't have an account? 
            <router-link to="/register" class="text-primary font-bold hover:underline">Register here</router-link>
          </p>
        </form>
      </template>
    </Card>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth';
import Card from 'primevue/card';
import Message from 'primevue/message';
import InputText from 'primevue/inputtext';
import Password from 'primevue/password';
import Button from 'primevue/button';

const router = useRouter();
const authStore = useAuthStore();

const form = ref({
  credential: '',
  password: '',
});

const loading = ref(false);

const handleLogin = async () => {
  loading.value = true;
  try {
    const response = await authStore.login(credentials);
    // Redirect to the original requested path or to home
    router.push(router.currentRoute.value.query.redirect || '/');
  } catch (err) {
    console.error(err);
  } finally {
    loading.value = false;
  }
};
</script>
