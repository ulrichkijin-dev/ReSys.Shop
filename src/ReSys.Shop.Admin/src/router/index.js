import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/accounts/auth/auth.store.js'
import AppLayout from '@/layouts/AppLayout.vue'
const router = createRouter({
  history: createWebHistory(),

    routes: [
        {
            path: '/auth/login',
            name: 'login',
            component: () => import('@/views/pages/auth/Login.vue'),
            meta: { guest: true }
        },
        {
            path: '/',
            component: AppLayout,
            meta: { requiresAuth: true },
            children: [
                {
                    path: '', // Empty path means it matches the parent's path: '/'
                    name: 'dashboard',
                    component: () => import('@/views/Dashboard.vue')
                },
                //  Catalog
                {
                    path: 'catalog/products', // No leading slash here, it's relative to parent '/'
                    name: 'adminproducts',
                    component: () => import('@/views/pages/catalog/Products.vue'),
                    meta: { breadcrumb: 'Products' }
                },
                {
                    path: 'catalog/products/:id',
                    name: 'adminproductdetail',
                    component: () => import('@/views/pages/catalog/ProductDetail.vue'),
                    meta: { breadcrumb: 'Product Details' }
                },
                {
                    path: 'catalog/taxonomies',
                    name: 'admintaxonomies',
                    component: () => import('@/views/pages/catalog/Taxonomies.vue'),
                    meta: { breadcrumb: 'Taxonomies' }
                },
                {
                    path: 'catalog/taxonomies/:id',
                    name: 'admintaxonomydetail',
                    component: () => import('@/views/pages/catalog/TaxonomyDetail.vue'),
                    meta: { breadcrumb: 'Taxonomy Details' }
                },
                {
                    path: 'catalog/taxonomies/:id/tree',
                    name: 'admintaxonomytree',
                    component: () => import('@/views/pages/catalog/TaxonTree.vue'),
                    meta: { breadcrumb: 'Hierarchy Builder' }
                },
                {
                    path: 'catalog/taxons/:id',
                    name: 'admintaxondetail',
                    component: () => import('@/views/pages/catalog/taxons/TaxonDetail.vue'),
                    meta: { breadcrumb: 'Taxon Details' }
                },
                /*
                {
                    path: 'catalog/taxons/new',
                    name: 'admintaxonnew',
                    component: () => import('@/views/pages/catalog/taxons/TaxonNew.vue'),
                    meta: { breadcrumb: 'New Taxon' }
                },
                */
                {
                    path: 'catalog/optiontypes',
                    name: 'adminoptiontypes',
                    component: () => import('@/views/pages/catalog/OptionTypes.vue'),
                    meta: { breadcrumb: 'Option Types' }
                },
                {
                    path: 'catalog/optiontypes/:id',
                    name: 'adminoptiontypedetail',
                    component: () => import('@/views/pages/catalog/OptionTypeDetail.vue'),
                    meta: { breadcrumb: 'Option Type Details' }
                },
                {
                    path: 'catalog/propertytypes',
                    name: 'adminpropertytypes',
                    component: () => import('@/views/pages/catalog/PropertyTypes.vue'),
                    meta: { breadcrumb: 'Property Types' }
                },
                {
                    path: 'catalog/propertytypes/:id',
                    name: 'adminpropertytypedetail',
                    component: () => import('@/views/pages/catalog/PropertyTypeDetail.vue'),
                    meta: { breadcrumb: 'Property Type Details' }
                },
                {
                    path: 'catalog/reviews',
                    name: 'adminreviews',
                    component: () => import('@/views/pages/catalog/Reviews.vue'),
                    meta: { breadcrumb: 'Reviews' }
                },

                //  Orders
                {
                    path: 'orders',
                    name: 'adminorders',
                    component: () => import('@/views/pages/orders/Orders.vue'),
                    meta: { breadcrumb: 'Orders' }
                },
                {
                    path: 'orders/:id',
                    name: 'adminorderdetail',
                    component: () => import('@/views/pages/orders/OrderDetail.vue'),
                    meta: { breadcrumb: 'Order Details' }
                },

                //  Inventory
                {
                    path: 'inventory/stockitems',
                    name: 'adminstockitems',
                    component: () => import('@/views/pages/inventories/StockItems.vue'),
                    meta: { breadcrumb: 'Stock Items' }
                },
                {
                    path: 'inventory/stocklocations',
                    name: 'adminstocklocations',
                    component: () => import('@/views/pages/inventories/StockLocations.vue'),
                    meta: { breadcrumb: 'Stock Locations' }
                },
                {
                    path: 'inventory/stocktransfers',
                    name: 'adminstocktransfers',
                    component: () => import('@/views/pages/inventories/StockTransfers.vue'),
                    meta: { breadcrumb: 'Stock Transfers' }
                },

                //  Promotions
                {
                    path: 'promotions',
                    name: 'adminpromotions',
                    component: () => import('@/views/pages/promotions/Promotions.vue'),
                    meta: { breadcrumb: 'Promotions' }
                },
                {
                    path: 'promotions/:id',
                    name: 'adminpromotiondetail',
                    component: () => import('@/views/pages/promotions/PromotionDetail.vue'),
                    meta: { breadcrumb: 'Promotion Details' }
                },

                //  Identity
                {
                    path: 'identity/users',
                    name: 'adminusers',
                    component: () => import('@/views/pages/identity/Users.vue'),
                    meta: { breadcrumb: 'Users' }
                },
                {
                    path: 'identity/users/:id',
                    name: 'adminuserdetail',
                    component: () => import('@/views/pages/identity/UserDetail.vue'),
                    meta: { breadcrumb: 'User Details' }
                },
                {
                    path: 'identity/roles',
                    name: 'adminroles',
                    component: () => import('@/views/pages/identity/Roles.vue'),
                    meta: { breadcrumb: 'Roles' }
                },
                {
                    path: 'identity/roles/:id',
                    name: 'adminroledetail',
                    component: () => import('@/views/pages/identity/RoleDetail.vue'),
                    meta: { breadcrumb: 'Role Details' }
                },
                {
                    path: 'identity/permissions',
                    name: 'adminpermissions',
                    component: () => import('@/views/pages/identity/Permissions.vue'),
                    meta: { breadcrumb: 'Permissions' }
                },

                //  Settings
                {
                    path: 'settings/paymentmethods',
                    name: 'adminpaymentmethods',
                    component: () => import('@/views/pages/settings/PaymentMethods.vue'),
                    meta: { breadcrumb: 'Payment Methods' }
                },
                {
                    path: 'settings/shippingmethods',
                    name: 'adminshippingmethods',
                    component: () => import('@/views/pages/settings/ShippingMethods.vue'),
                    meta: { breadcrumb: 'Shipping Methods' }
                },
                {
                    path: 'settings/config',
                    name: 'adminsettings',
                    component: () => import('@/views/pages/system/Config.vue'),
                    meta: { breadcrumb: 'Application Settings' }
                },

                //  Account
                {
                    path: 'account/profile',
                    name: 'accountprofile',
                    component: () => import('@/views/pages/Profile.vue'),
                    meta: { breadcrumb: 'Profile' }
                },
                {
                    path: 'account/addresses',
                    name: 'accountaddresses',
                    component: () => import('@/views/pages/Addresses.vue'),
                    meta: { breadcrumb: 'My Addresses' }
                }
            ]
        },
        {
            path: '/pages/empty',
            name: 'empty',
            component: () => import('@/views/pages/Empty.vue'),
        },
        {
            path: '/pages/notfound',
            name: 'notfound',
            component: () => import('@/views/pages/NotFound.vue'),
        },
        {
            path: '/auth/unauthorized',
            name: 'accessDenied',
            component: () => import('@/views/pages/auth/Access.vue'),
        },
        {
            path: '/auth/error',
            name: 'error',
            component: () => import('@/views/pages/auth/Error.vue'),
        },
        // Catch all - 404
        {
          path: '/:catchAll(.*)',
          name: 'notFoundCatchAll',
          component: () => import('@/views/pages/NotFound.vue'),
        }
    ],

})

// Navigation Guard
router.beforeEach(async (to, from, next) => {
  const authStore = useAuthStore()

  // Initialize auth state if not already done
  if (!authStore.isAuthenticated) {
    authStore.initializeAuth()
  }

  const requiresAuth = to.matched.some((record) => record.meta.requiresAuth)
  const isGuestOnly = to.matched.some((record) => record.meta.guest)

  if (requiresAuth && !authStore.isAuthenticated) {
    // Redirect to login if not authenticated
    next({ name: 'login', query: { redirect: to.fullPath } })
  } else if (isGuestOnly && authStore.isAuthenticated) {
    // Redirect to dashboard if already authenticated and trying to access guest page
    next({ name: 'dashboard' })
  } else {
    next()
  }
})
export default router
