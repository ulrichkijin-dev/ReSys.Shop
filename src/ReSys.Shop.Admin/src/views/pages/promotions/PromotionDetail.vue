<script setup>
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { usePromotionStore } from '@/stores';
import { usePromotionRuleStore } from '@/stores';
import { useToast } from 'primevue/usetoast';

import Tabs from 'primevue/tabs';
import TabList from 'primevue/tablist';
import Tab from 'primevue/tab';
import TabPanels from 'primevue/tabpanels';
import TabPanel from 'primevue/tabpanel';
import Button from 'primevue/button';
import InputText from 'primevue/inputtext';
import InputNumber from 'primevue/inputnumber';
import Select from 'primevue/select';
import DatePicker from 'primevue/datepicker';
import ToggleSwitch from 'primevue/toggleswitch';
import Textarea from 'primevue/textarea';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';

const route = useRoute();
const router = useRouter();
const store = usePromotionStore();
const ruleStore = usePromotionRuleStore();
const toast = useToast();

const isNew = computed(() => route.params.id === 'new');
const promotionId = ref(route.params.id);

const promotion = ref({
    name: '',
    promotionCode: '',
    description: '',
    active: true,
    requiresCouponCode: false,
    usageLimit: null,
    minimumOrderAmount: 0,
    maximumDiscountAmount: null,
    startsAt: null,
    expiresAt: null,
    action: {
        type: 'OrderDiscount',
        discountType: 'Percentage',
        value: 0
    }
});

const promoTypes = ['OrderDiscount', 'ItemDiscount', 'FreeShipping', 'BuyXGetY'];
const discountTypes = ['Percentage', 'FixedAmount'];

onMounted(async () => {
    if (!isNew.value) {
        await store.fetchPromotionById(promotionId.value);
        if (store.selectedPromotion) {
            promotion.value = { ...store.selectedPromotion };
        }
        await ruleStore.fetchPromotionRules(promotionId.value);
    }
});

async function onSave() {
    let success = false;
    if (isNew.value) {
        success = await store.createPromotion(promotion.value);
    } else {
        success = await store.updatePromotion(promotionId.value, promotion.value);
    }

    if (success) {
        toast.add({ severity: 'success', summary: 'Success', detail: 'Promotion saved', life: 3000 });
        if (isNew.value) router.push('/promotions');
    }
}
</script>

<template>
    <div class="card">
        <div class="flex items-center justify-between mb-6">
            <div>
                <div class="text-3xl font-medium mb-2">{{ isNew ? 'New Promotion' : 'Edit Promotion' }}</div>
                <div class="text-muted-color">Configure discounts and marketing rules</div>
            </div>
            <div class="flex gap-2">
                <Button label="Cancel" severity="secondary" outlined @click="router.push('/promotions')" />
                <Button label="Save" icon="pi pi-check" @click="onSave" :loading="store.loading" />
            </div>
        </div>

        <Tabs value="0">
            <TabList>
                <Tab value="0">Configuration</Tab>
                <Tab value="1" v-if="!isNew">Eligibility Rules</Tab>
                <Tab value="2" v-if="!isNew">Analytics</Tab>
            </TabList>
            <TabPanels>
                <TabPanel value="0">
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-6 pt-4">
                        <!-- Identity -->
                        <div class="flex flex-col gap-2">
                            <label class="font-bold">Name</label>
                            <InputText v-model="promotion.name" placeholder="Summer Sale 2025" />
                        </div>
                        <div class="flex flex-col gap-2">
                            <label class="font-bold">Coupon Code (Optional)</label>
                            <InputText v-model="promotion.promotionCode" placeholder="SUMMER25" />
                        </div>

                        <!-- Action -->
                        <div class="flex flex-col gap-2">
                            <label class="font-bold">Promotion Type</label>
                            <Select v-model="promotion.action.type" :options="promoTypes" />
                        </div>
                        <div class="flex flex-col gap-2">
                            <label class="font-bold">Discount Type</label>
                            <Select v-model="promotion.action.discountType" :options="discountTypes" />
                        </div>
                        <div class="flex flex-col gap-2">
                            <label class="font-bold">Discount Value</label>
                            <InputNumber v-model="promotion.action.value" mode="decimal" :minFractionDigits="2" />
                        </div>

                        <!-- Settings -->
                        <div class="flex flex-col gap-2">
                            <label class="font-bold">Usage Limit</label>
                            <InputNumber v-model="promotion.usageLimit" placeholder="No limit" />
                        </div>
                        <div class="flex flex-col gap-2">
                            <label class="font-bold">Starts At</label>
                            <DatePicker v-model="promotion.startsAt" showTime hourFormat="24" />
                        </div>
                        <div class="flex flex-col gap-2">
                            <label class="font-bold">Expires At</label>
                            <DatePicker v-model="promotion.expiresAt" showTime hourFormat="24" />
                        </div>

                        <div class="md:col-span-2 flex flex-wrap gap-6 items-center py-2">
                            <div class="flex items-center gap-2">
                                <ToggleSwitch v-model="promotion.active" />
                                <label class="font-bold">Active</label>
                            </div>
                            <div class="flex items-center gap-2">
                                <ToggleSwitch v-model="promotion.requiresCouponCode" />
                                <label class="font-bold">Require Coupon Entry</label>
                            </div>
                        </div>

                        <div class="flex flex-col gap-2 md:col-span-2">
                            <label class="font-bold">Description</label>
                            <Textarea v-model="promotion.description" rows="3" autoResize />
                        </div>
                    </div>
                </TabPanel>

                <TabPanel value="1" v-if="!isNew">
                    <div class="flex justify-between items-center mb-4">
                        <h5 class="m-0 font-bold">Rules</h5>
                        <Button label="Add Rule" icon="pi pi-plus" size="small" />
                    </div>
                    <DataTable :value="ruleStore.promotionRules" size="small">
                        <Column field="type" header="Condition"></Column>
                        <Column field="value" header="Parameter"></Column>
                        <Column header="Actions">
                            <template #body>
                                <Button icon="pi pi-trash" severity="danger" text rounded />
                            </template>
                        </Column>
                    </DataTable>
                </TabPanel>

                <TabPanel value="2" v-if="!isNew">
                    <div class="flex flex-col items-center justify-center p-12 bg-surface-50 dark:bg-surface-950 rounded-lg">
                        <i class="pi pi-chart-bar text-4xl mb-4 text-blue-500"></i>
                        <p>Detailed performance stats and usage logs will appear here as orders are placed.</p>
                    </div>
                </TabPanel>
            </TabPanels>
        </Tabs>
    </div>
</template>
