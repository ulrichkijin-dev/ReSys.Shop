<template>
  <Card>
    <template #title> Account Profile </template>
    <template #content>
      <form @submit.prevent="updateProfile" class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div class="flex flex-col gap-2">
          <label for="firstName">First Name</label>
          <InputText id="firstName" v-model="profile.firstName" />
        </div>
        <div class="flex flex-col gap-2">
          <label for="lastName">Last Name</label>
          <InputText id="lastName" v-model="profile.lastName" />
        </div>
        <div class="flex flex-col gap-2">
          <label for="username">Username</label>
          <InputText id="username" v-model="profile.username" disabled />
        </div>
        <div class="flex flex-col gap-2">
          <label for="email">Email</label>
          <InputText id="email" v-model="profile.email" disabled />
        </div>
        <div class="flex flex-col gap-2">
          <label for="dob">Date of Birth</label>
          <DatePicker id="dob" v-model="profile.dateOfBirth" />
        </div>
        <div class="col-span-full mt-4">
          <Button type="submit" label="Save Changes" :loading="saving" />
        </div>
      </form>
    </template>
  </Card>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { accountService } from '@/services/account.service';
import Card from 'primevue/card';
import InputText from 'primevue/inputtext';
import DatePicker from 'primevue/datepicker';
import Button from 'primevue/button';

const profile = ref({
  firstName: '',
  lastName: '',
  username: '',
  email: '',
  dateOfBirth: null,
});

const loading = ref(true);
const saving = ref(false);

onMounted(async () => {
  try {
    const response = await accountService.getProfile();
    profile.value = response.data;
  } catch (err) {
    console.error(err);
  } finally {
    loading.value = false;
  }
});

const updateProfile = async () => {
  saving.value = true;
  try {
    await accountService.updateProfile({
      firstName: profile.value.firstName,
      lastName: profile.value.lastName,
      dateOfBirth: profile.value.dateOfBirth,
    });
  } catch (err) {
    console.error(err);
  } finally {
    saving.value = false;
  }
};
</script>
