<template>
  <div class="max-w-2xl mx-auto py-12 px-4">
    <Card class="shadow-xl border-none">
      <template #title>
        <div class="text-center">
          <h1 class="text-3xl font-bold">Create Account</h1>
          <p class="text-surface-500 text-sm font-normal mt-2">Join ReSys.Shop to manage orders and save addresses.</p>
        </div>
      </template>
      <template #content>
        <form @submit.prevent="handleRegister" class="space-y-6">
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div class="flex flex-col gap-2">
              <label for="firstName">First Name</label>
              <InputText id="firstName" v-model="form.firstName" placeholder="John" />
            </div>
            <div class="flex flex-col gap-2">
              <label for="lastName">Last Name</label>
              <InputText id="lastName" v-model="form.lastName" placeholder="Doe" />
            </div>
          </div>

          <div class="flex flex-col gap-2">
            <label for="email">Email *</label>
            <InputText id="email" v-model="form.email" type="email" placeholder="john.doe@example.com" required />
          </div>

          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div class="flex flex-col gap-2">
              <label for="userName">Username</label>
              <InputText id="userName" v-model="form.userName" placeholder="johndoe123" />
            </div>
            <div class="flex flex-col gap-2">
              <label for="phoneNumber">Phone Number</label>
              <InputMask id="phoneNumber" v-model="form.phoneNumber" mask="(999) 999-9999" placeholder="(555) 000-0000" />
            </div>
          </div>

          <div class="flex flex-col gap-2">
            <label for="dob">Date of Birth</label>
            <DatePicker id="dob" v-model="form.dateOfBirth" :maxDate="new Date()" showIcon />
          </div>

          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div class="flex flex-col gap-2">
              <label for="password">Password *</label>
              <Password id="password" v-model="form.password" toggleMask required />
            </div>
            <div class="flex flex-col gap-2">
              <label for="confirmPassword">Confirm Password *</label>
              <Password id="confirmPassword" v-model="form.confirmPassword" :feedback="false" toggleMask required />
            </div>
          </div>

          <div class="flex items-start gap-3 mt-4">
            <Checkbox id="terms" v-model="acceptTerms" :binary="true" required />
            <label for="terms" class="text-sm text-surface-600">
              I agree to the <a href="#" class="text-primary hover:underline">Terms of Service</a> and 
              <a href="#" class="text-primary hover:underline">Privacy Policy</a>.
            </label>
          </div>

          <div class="pt-4">
            <Button type="submit" label="Sign Up" class="w-full" size="large" :loading="loading" />
          </div>

          <p class="text-center text-sm text-surface-600 mt-6">
            Already have an account? 
            <router-link to="/login" class="text-primary font-bold hover:underline">Login here</router-link>
          </p>
        </form>
      </template>
    </Card>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { authService } from '@/services/auth.service';
import Card from 'primevue/card';
import InputText from 'primevue/inputtext';
import Password from 'primevue/password';
import Button from 'primevue/button';
import InputMask from 'primevue/inputmask';
import DatePicker from 'primevue/datepicker';
import Checkbox from 'primevue/checkbox';

const router = useRouter();
const loading = ref(false);
const acceptTerms = ref(false);

const form = ref({
  userName: '',
  email: '',
  firstName: '',
  lastName: '',
  phoneNumber: '',
  password: '',
  confirmPassword: '',
  dateOfBirth: null
});

const handleRegister = async () => {
  if (form.value.password !== form.value.confirmPassword) {
    alert("Passwords do not match!");
    return;
  }

  loading.value = true;
  try {
    const payload = {
      ...form.value,
      // Date formatting for backend
      dateOfBirth: form.value.dateOfBirth ? form.value.dateOfBirth.toISOString() : null
    };
    
    await authService.register(payload);
    // After registration, redirect to login
    router.push('/login?registered=true');
  } catch (err) {
    console.error('Registration failed:', err);
    alert(err.response?.data?.message || 'Registration failed. Please check your details.');
  } finally {
    loading.value = false;
  }
};
</script>
