<template>
  <form @submit.prevent="handleSubmit" class="grid grid-cols-1 md:grid-cols-2 gap-4">
    <div class="flex flex-col gap-2">
      <label for="firstName">First Name *</label>
      <InputText id="firstName" v-model="form.firstName" required />
    </div>
    <div class="flex flex-col gap-2">
      <label for="lastName">Last Name *</label>
      <InputText id="lastName" v-model="form.lastName" required />
    </div>
    <div class="col-span-full flex flex-col gap-2">
      <label for="company">Company</label>
      <InputText id="company" v-model="form.company" />
    </div>
    <div class="col-span-full flex flex-col gap-2">
      <label for="address1">Address line 1 *</label>
      <InputText id="address1" v-model="form.address1" required />
    </div>
    <div class="col-span-full flex flex-col gap-2">
      <label for="address2">Address line 2</label>
      <InputText id="address2" v-model="form.address2" />
    </div>
    <div class="flex flex-col gap-2">
      <label for="country">Country *</label>
      <Select 
        id="country" 
        v-model="selectedCountry" 
        :options="countries" 
        optionLabel="name" 
        placeholder="Select Country" 
        class="w-full"
        @change="onCountryChange"
        required
      />
    </div>
    <div class="flex flex-col gap-2">
      <label for="state">State / Province</label>
      <Select 
        id="state" 
        v-model="selectedState" 
        :options="states" 
        optionLabel="name" 
        placeholder="Select State" 
        class="w-full"
        :disabled="!states.length"
      />
    </div>
    <div class="flex flex-col gap-2">
      <label for="city">City *</label>
      <InputText id="city" v-model="form.city" required />
    </div>
    <div class="flex flex-col gap-2">
      <label for="zipcode">Zip / Postal Code *</label>
      <InputText id="zipcode" v-model="form.zipcode" required />
    </div>
    <div class="col-span-full flex flex-col gap-2">
      <label for="phone">Phone Number *</label>
      <InputMask id="phone" v-model="form.phone" mask="(999) 999-9999" required />
    </div>
    
    <div class="col-span-full mt-4 flex justify-end gap-2">
      <slot name="actions" :form="form"></slot>
    </div>
  </form>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue';
import { locationService } from '@/services/location.service';
import InputText from 'primevue/inputtext';
import Select from 'primevue/select';
import InputMask from 'primevue/inputmask';

const props = defineProps({
  initialData: { type: Object, default: () => ({}) }
});

const emit = defineEmits(['submit']);

const form = ref({
  firstName: '',
  lastName: '',
  company: '',
  address1: '',
  address2: '',
  city: '',
  zipcode: '',
  phone: '',
  countryId: null,
  stateId: null,
  stateName: '',
  ...props.initialData
});

const countries = ref([]);
const states = ref([]);
const selectedCountry = ref(null);
const selectedState = ref(null);

onMounted(async () => {
  const response = await locationService.getCountries();
  countries.value = response.data.items;
  
  if (form.value.countryId) {
    selectedCountry.value = countries.value.find(c => c.id === form.value.countryId);
    if (selectedCountry.value) await fetchStates(selectedCountry.value.id);
  }
});

const onCountryChange = async (e) => {
  form.value.countryId = e.value.id;
  await fetchStates(e.value.id);
};

const fetchStates = async (countryId) => {
  const response = await locationService.getStates({ countryId });
  states.value = response.data.items;
};

watch(selectedState, (val) => {
  if (val) {
    form.value.stateId = val.id;
    form.value.stateName = val.name;
  }
});

const handleSubmit = () => {
  emit('submit', { ...form.value });
};
</script>
