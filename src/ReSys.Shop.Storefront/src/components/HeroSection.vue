<template>
  <section class="relative py-16 mb-12 overflow-hidden bg-surface-50 dark:bg-surface-900 border-b border-surface-200 dark:border-surface-800">
    <!-- Subtle Paper Texture Overlay -->
    <div class="absolute inset-0 opacity-[0.03] dark:opacity-[0.05] pointer-events-none" 
         style="background-image: url('https://www.transparenttextures.com/patterns/p6.png');"></div>
    
    <div class="container mx-auto px-4 relative z-10">
      <nav v-if="breadcrumbs.length" class="flex mb-4 text-sm font-medium text-surface-500 uppercase tracking-widest">
        <ol class="flex items-center space-x-2">
          <li><router-link to="/" class="hover:text-primary transition-colors">Home</router-link></li>
          <li v-for="(crumb, index) in breadcrumbs" :key="index" class="flex items-center space-x-2">
            <i class="pi pi-chevron-right text-[10px]"></i>
            <router-link v-if="crumb.to" :to="crumb.to" class="hover:text-primary transition-colors">{{ crumb.label }}</router-link>
            <span v-else>{{ crumb.label }}</span>
          </li>
        </ol>
      </nav>

      <h1 class="text-4xl md:text-6xl font-extrabold tracking-tight mb-4 text-surface-900 dark:text-surface-0">
        {{ title }}
      </h1>
      <p v-if="subtitle" class="text-lg md:text-xl text-surface-600 dark:text-surface-400 max-w-2xl leading-relaxed">
        {{ subtitle }}
      </p>
    </div>

    <!-- Decorative Paper Edge/Shadow -->
    <div class="absolute bottom-0 left-0 right-0 h-px bg-gradient-to-r from-transparent via-surface-300 dark:via-surface-700 to-transparent"></div>
  </section>
</template>

<script setup>
defineProps({
  title: { type: String, required: true },
  subtitle: { type: String, default: '' },
  breadcrumbs: { type: Array, default: () => [] }
});
</script>
