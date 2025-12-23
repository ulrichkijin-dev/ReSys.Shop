<script setup>
import { ref } from 'vue';

import AppMenuItem from './AppMenuItem.vue';

const model = ref([
    {
        label: 'Home',
        items: [{ label: 'Dashboard', icon: 'pi pi-fw pi-home', to: '/' }]
    },
    {
        label: 'Catalog',
        items: [
            {
                label: 'Products',
                icon: 'pi pi-fw pi-shopping-bag',
                to: '/catalog/products'
            },
            {
                label: 'Taxonomies',
                icon: 'pi pi-fw pi-tags',
                to: '/catalog/taxonomies'
            },
            {
                label: 'Option Types',
                icon: 'pi pi-fw pi-list',
                to: '/catalog/option-types'
            },
            {
                label: 'Property Types',
                icon: 'pi pi-fw pi-sliders-h',
                to: '/catalog/property-types'
            },
            {
                label: 'Reviews',
                icon: 'pi pi-fw pi-comment',
                to: '/catalog/reviews'
            }
        ]
    },
    {
        label: 'Sales',
        items: [
            {
                label: 'Orders',
                icon: 'pi pi-fw pi-shopping-cart',
                to: '/orders'
            }
        ]
    },
    {
        label: 'Inventory',
        items: [
            {
                label: 'Stock Items',
                icon: 'pi pi-fw pi-box',
                to: '/inventory/stock-items'
            },
            {
                label: 'Stock Locations',
                icon: 'pi pi-fw pi-map-marker',
                to: '/inventory/stock-locations'
            },
            {
                label: 'Stock Transfers',
                icon: 'pi pi-fw pi-sync',
                to: '/inventory/stock-transfers'
            }
        ]
    },
    {
        label: 'Marketing',
        items: [
            {
                label: 'Promotions',
                icon: 'pi pi-fw pi-percentage',
                to: '/promotions'
            }
        ]
    },
    {
        label: 'Identity',
        items: [
            {
                label: 'Users',
                icon: 'pi pi-fw pi-users',
                to: '/identity/users'
            },
            {
                label: 'Roles',
                icon: 'pi pi-fw pi-shield',
                to: '/identity/roles'
            },
            {
                label: 'Permissions',
                icon: 'pi pi-fw pi-key',
                to: '/identity/permissions'
            }
        ]
    },
    {
        label: 'Settings',
        items: [
            {
                label: 'Payment Methods',
                icon: 'pi pi-fw pi-credit-card',
                to: '/settings/payment-methods'
            },
            {
                label: 'Shipping Methods',
                icon: 'pi pi-fw pi-truck',
                to: '/settings/shipping-methods'
            },
            {
                label: 'App Configuration',
                icon: 'pi pi-fw pi-cog',
                to: '/settings/config'
            }
        ]
    },
    {
        label: 'System',
        items: [
            {
                label: 'Audit Logs',
                icon: 'pi pi-fw pi-history',
                to: '/system/audit-logs'
            }
        ]
    },
    {
        label: 'Account',
        items: [
            {
                label: 'My Profile',
                icon: 'pi pi-fw pi-user-edit',
                to: '/account/profile'
            },
            {
                label: 'My Addresses',
                icon: 'pi pi-fw pi-map',
                to: '/account/addresses'
            }
        ]
    }
]);
</script>

<template>
    <ul class="layout-menu">
        <template v-for="(item, i) in model" :key="item.label">
            <app-menu-item v-if="!item.separator" :item="item" :index="i"></app-menu-item>
            <li v-if="item.separator" class="menu-separator"></li>
        </template>
    </ul>
</template>

<style lang="scss" scoped></style>
