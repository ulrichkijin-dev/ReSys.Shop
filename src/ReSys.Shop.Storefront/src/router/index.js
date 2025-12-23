import { createRouter, createWebHistory } from 'vue-router';
import BaseLayout from '@/layout/BaseLayout.vue';
import { useAuthStore } from '@/stores/auth'; // Import the auth store

const routes = [
  {
    path: '/',
    component: BaseLayout,
    children: [
      {
        path: '',
        name: 'home',
        component: () => import('@/views/HomeView.vue'),
      },
      {
        path: 'products',
        name: 'products',
        component: () => import('@/views/ProductListView.vue'),
      },
      {
        path: 'product/:slug',
        name: 'product-detail',
        component: () => import('@/views/ProductDetailView.vue'),
      },
      {
        path: 'cart',
        name: 'cart',
        component: () => import('@/views/CartView.vue'),
      },
      {
        path: 'visual-search',
        name: 'visual-search',
        component: () => import('@/views/VisualSearchView.vue'),
      },
      {
        path: 'account',
        component: () => import('@/layout/AccountLayout.vue'),
        children: [
          {
            path: '',
            redirect: '/account/profile'
          },
          {
            path: 'profile',
            name: 'account-profile',
            component: () => import('@/views/account/ProfileView.vue'),
          },
          {
            path: 'addresses',
            name: 'account-addresses',
            component: () => import('@/views/account/AddressView.vue'),
          },
          {
            path: 'orders',
            name: 'account-orders',
            component: () => import('@/views/account/OrderHistoryView.vue'),
          },
          {
            path: 'orders/:number',
            name: 'order-detail',
            component: () => import('@/views/account/OrderDetailView.vue'),
          }
        ]
      },
      {
        path: 'checkout',
        name: 'checkout',
        component: () => import('@/views/CheckoutView.vue'),
        meta: { requiresAuth: true } // Add meta field to require auth
      },
      {
        path: 'checkout/complete',
        name: 'checkout-complete',
        component: () => import('@/views/CheckoutCompleteView.vue'),
        meta: { requiresAuth: true } // Also requires auth for completion page if checkout does
      },
      {
        path: 'login',
        name: 'login',
        component: () => import('@/views/LoginView.vue'),
      },
      {
        path: 'register',
        name: 'register',
        component: () => import('@/views/RegisterView.vue'),
      },
      {
        path: 'confirm-email',
        name: 'confirm-email',
        component: () => import('@/views/auth/ConfirmEmailView.vue'),
      },
      {
        path: 'reset-password',
        name: 'reset-password',
        component: () => import('@/views/auth/ResetPasswordView.vue'),
      }
    ],
  },
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
});

// Global navigation guard
router.beforeEach((to, from, next) => {
  const authStore = useAuthStore();
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next({ name: 'login', query: { redirect: to.fullPath, message: 'loginRequired' } });
  } else {
    next();
  }
});

export default router;