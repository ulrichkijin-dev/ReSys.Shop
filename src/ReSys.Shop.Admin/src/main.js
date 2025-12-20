import { createApp } from 'vue';
import App from './App.vue';
import router from './router';

// PrimeVue imports
import Nora  from '@primeuix/themes/nora';
import PrimeVue from 'primevue/config';
import ConfirmationService from 'primevue/confirmationservice';
import ToastService from 'primevue/toastservice';

// Axios initialization
// import { initializeAxios } from "@/api/axios"

import '@/assets/tailwind.css';
import '@/assets/styles.scss';

const app = createApp(App);

app.use(router);
app.use(PrimeVue, {
    theme: {
        preset: Nora,
        options: {
             prefix: 'p',
            darkModeSelector: '.app-dark'
        }
    }
});
app.use(ToastService);
app.use(ConfirmationService);

// initializeAxios();

app.mount('#app');
