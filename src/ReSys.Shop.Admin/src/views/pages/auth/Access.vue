<script setup>
  import Card from 'primevue/card'
  import Button from 'primevue/button'
</script>

<template>
  <div
    class="min-h-screen bg-linear-to-br from-slate-900 to-slate-800 flex items-center justify-center"
  >
    <Card class="w-full max-w-md text-center shadow-2xl">
      <template #content>
        <i class="pi pi-exclamation-triangle text-5xl text-red-500 mb-4"></i>
        <h1 class="text-2xl font-bold text-white mb-2">Access Denied</h1>
        <p class="text-slate-400 mb-6">You don't have permission to access this resource.</p>
        <Button label="Go Back" icon="pi pi-arrow-left" @click="$router.back()" class="w-full" />
      </template>
    </Card>
  </div>
</template>
