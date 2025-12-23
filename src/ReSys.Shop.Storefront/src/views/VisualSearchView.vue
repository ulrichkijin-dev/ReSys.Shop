<template>
  <div class="max-w-6xl mx-auto">
    <div class="text-center mb-12">
      <h1 class="text-4xl font-bold mb-4">Visual Search</h1>
      <p class="text-surface-600 dark:text-surface-400">Upload an image to find similar fashion items using AI.</p>
    </div>

    <div class="mb-12">
      <div class="flex flex-col items-center justify-center border-2 border-dashed border-surface-300 dark:border-surface-700 rounded-2xl p-12 bg-surface-50 dark:bg-surface-900 transition-colors hover:border-primary">
        <FileUpload 
          mode="basic" 
          name="file" 
          accept="image/*" 
          :maxFileSize="5000000" 
          @select="onFileSelect" 
          :auto="false"
          chooseLabel="Select Fashion Photo"
          class="p-button-lg"
          v-if="!previewUrl"
        />
        
        <div v-if="previewUrl" class="flex flex-col items-center">
          <img :src="previewUrl" class="w-64 h-64 object-cover rounded-lg shadow-lg mb-6" />
          <div class="flex gap-4">
            <Button label="Search Similar" icon="pi pi-search" @click="handleSearch" :loading="loading" />
            <Button label="Clear" icon="pi pi-refresh" severity="secondary" @click="clear" />
          </div>
        </div>
        
        <p v-if="!previewUrl" class="mt-4 text-sm text-surface-500">Max size 5MB. Powered by Fashion-CLIP.</p>
      </div>
    </div>

    <div v-if="loading" class="grid grid-cols-1 md:grid-cols-4 gap-6">
      <div v-for="i in 8" :key="i" class="border border-surface-200 dark:border-surface-700 rounded-lg p-4">
        <Skeleton width="100%" height="200px" class="mb-4" />
        <Skeleton width="60%" height="1.5rem" class="mb-2" />
      </div>
    </div>

    <div v-else-if="results.length > 0">
      <h2 class="text-2xl font-bold mb-6">Search Results</h2>
      <div class="grid grid-cols-1 md:grid-cols-4 gap-6">
        <router-link 
          v-for="item in results" 
          :key="item.image_id" 
          :to="'/product/' + item.product_slug"
          class="group"
        >
          <div class="border border-surface-200 dark:border-surface-700 rounded-lg overflow-hidden transition-shadow hover:shadow-xl">
            <div class="h-64 relative overflow-hidden">
              <img :src="item.image_url" class="w-full h-full object-cover transition-transform group-hover:scale-105" />
              <div class="absolute top-2 right-2 bg-black/60 text-white text-xs px-2 py-1 rounded">
                {{ (item.similarity * 100).toFixed(1) }}% match
              </div>
            </div>
            <div class="p-4 bg-surface-0 dark:bg-surface-900">
              <h3 class="font-bold truncate">{{ item.product_name }}</h3>
              <p class="text-primary font-bold">{{ item.price }} {{ item.currency }}</p>
            </div>
          </div>
        </router-link>
      </div>
    </div>
    
    <div v-else-if="hasSearched && !loading" class="text-center py-12">
      <i class="pi pi-search text-4xl text-surface-300 mb-4"></i>
      <p>No matches found. Try a different image.</p>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue';
import { useImageSearchStore } from '@/stores/imageSearch';
import FileUpload from 'primevue/fileupload';
import Button from 'primevue/button';
import Skeleton from 'primevue/skeleton';

const store = useImageSearchStore();
const previewUrl = ref(null);
const selectedFile = ref(null);
const hasSearched = ref(false);

const loading = computed(() => store.loading);
const results = computed(() => store.searchResults);

const onFileSelect = (event) => {
  const file = event.files[0];
  if (file) {
    selectedFile.value = file;
    previewUrl.value = URL.createObjectURL(file);
    hasSearched.value = false;
  }
};

const handleSearch = async () => {
  if (!selectedFile.value) return;
  try {
    await store.searchByImage(selectedFile.value);
    hasSearched.value = true;
  } catch (err) {
    console.error(err);
  }
};

const clear = () => {
  selectedFile.value = null;
  previewUrl.value = null;
  store.$patch({ searchResults: [] });
  hasSearched.value = false;
};
</script>
