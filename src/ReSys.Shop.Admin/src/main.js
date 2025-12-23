// src/ReSys.Shop.Admin/src/main.js

import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'

import '@/assets/styles.scss'
import '@/assets/tailwind.css'

// PrimeVue setup
import PrimeVue from 'primevue/config'
import Aura from '@primevue/themes/aura'
import ToastService from 'primevue/toastservice'
import Toast from 'primevue/toast'

// HTTP Client configuration
import { configureHttpClient } from './utils/http-client'

// Notification Service
import { PrimeVueNotificationService } from './services/notification/primevue.notification.service'

import router from './router'

const app = createApp(App)
const pinia = createPinia()

app.use(pinia) // Install Pinia first
app.use(router)

// Install PrimeVue and ToastService
app.use(PrimeVue, {
  theme: {
    preset: Aura,
    options: {
      darkModeSelector: '.app-dark',
    },
  },
})
app.use(ToastService)

// Initialize the notification service with PrimeVue's $toast instance
const notificationService = new PrimeVueNotificationService(app.config.globalProperties.$toast)

// Configure the HTTP client by injecting the notification service
const httpClient = configureHttpClient(notificationService)

// Make the HTTP client and notification service globally available for convenience
app.config.globalProperties.$httpClient = httpClient
app.config.globalProperties.$notification = notificationService

// Register the PrimeVue Toast component globally
app.component('PrimeToast', Toast)

// If you have a router, enable it here
// app.use(router);

app.mount('#app')
