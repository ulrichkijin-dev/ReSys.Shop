<template>
  <div class="min-h-screen bg-linear-to-br from-slate-900 to-slate-800 flex items-center justify-center">
    <Card class="w-full max-w-md text-center shadow-2xl">
      <template #content>
        <i class="pi pi-times-circle text-5xl text-orange-500 mb-4"></i>
        <h1 class="text-2xl font-bold text-white mb-2">404 - Not Found</h1>
        <p class="text-slate-400 mb-6">The page you're looking for doesn't exist.</p>
        <Button
          label="Go to Dashboard"
          icon="pi pi-home"
          @click="$router.push('/dashboard')"
          class="w-full"
        />
      </template>
    </Card>
  </div>
</template>

<script setup>
import Card from 'primevue/card'
import Button from 'primevue/button'
</script>
