<script setup>
import { onMounted, ref, reactive, watch } from 'vue';
import { useSystemStore } from '@/stores';
import { storeToRefs } from 'pinia';

const systemStore = useSystemStore();
const { auditLogs, loading } = storeToRefs(systemStore);

const lazyParams = reactive({
    pageIndex: 1,
    pageSize: 10,
    search: '',
    orderBy: 'timestamp desc'
});

const selectedLog = ref(null);
const detailDialog = ref(false);

const loadLazyData = async () => {
    await systemStore.fetchAuditLogs({
        pageIndex: lazyParams.pageIndex,
        pageSize: lazyParams.pageSize,
        search: lazyParams.search,
        orderBy: lazyParams.orderBy
    });
};

onMounted(() => {
    loadLazyData();
});

const onPage = (event) => {
    lazyParams.pageIndex = event.page + 1;
    lazyParams.pageSize = event.rows;
    loadLazyData();
};

const formatDate = (dateString) => {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleString();
};

const getSeverity = (severity) => {
    switch (severity) {
        case 0: return 'secondary'; // Information
        case 1: return 'warn';      // Warning
        case 2: return 'danger';    // Error
        case 3: return 'danger';    // Critical
        default: return 'info';
    }
};

const getSeverityLabel = (severity) => {
    switch (severity) {
        case 0: return 'Information';
        case 1: return 'Warning';
        case 2: return 'Error';
        case 3: return 'Critical';
        default: return 'Unknown';
    }
};

const viewDetails = (log) => {
    selectedLog.value = log;
    detailDialog.value = true;
};

const formatJson = (jsonString) => {
    if (!jsonString) return 'None';
    try {
        const obj = JSON.parse(jsonString);
        return JSON.stringify(obj, null, 2);
    } catch (e) {
        return jsonString;
    }
};
</script>

<template>
    <div class="card">
        <div class="font-semibold text-xl mb-4">Audit Logs</div>
        <p class="mb-6">View and track system changes and user activities.</p>

        <DataTable
            :value="auditLogs?.items"
            lazy
            paginator
            :rows="lazyParams.pageSize"
            :totalRecords="auditLogs?.totalCount"
            :loading="loading"
            @page="onPage"
            dataKey="id"
            filterDisplay="menu"
            class="p-datatable-sm"
            removableSort
        >
            <template #header>
                <div class="flex justify-between items-center">
                    <IconField>
                        <InputIcon class="pi pi-search" />
                        <InputText v-model="lazyParams.search" placeholder="Search logs..." @keydown.enter="loadLazyData" />
                    </IconField>
                    <Button type="button" icon="pi pi-refresh" label="Refresh" @click="loadLazyData" outlined />
                </div>
            </template>
            <template #empty> No audit logs found. </template>
            <template #loading> Loading audit logs data. Please wait. </template>

            <Column field="timestamp" header="Timestamp" style="min-width: 12rem">
                <template #body="{ data }">
                    {{ formatDate(data.timestamp) }}
                </template>
            </Column>
            <Column field="userName" header="User" style="min-width: 10rem">
                <template #body="{ data }">
                    <div class="flex flex-col">
                        <span class="font-medium">{{ data.userName || 'System' }}</span>
                        <small class="text-muted-color">{{ data.userEmail }}</small>
                    </div>
                </template>
            </Column>
            <Column field="action" header="Action" style="min-width: 8rem"></Column>
            <Column field="entityName" header="Entity" style="min-width: 8rem"></Column>
            <Column field="severity" header="Severity" style="min-width: 8rem">
                <template #body="{ data }">
                    <Tag :value="getSeverityLabel(data.severity)" :severity="getSeverity(data.severity)" />
                </template>
            </Column>
            <Column header="Actions" style="min-width: 5rem">
                <template #body="{ data }">
                    <Button icon="pi pi-eye" text rounded @click="viewDetails(data)" />
                </template>
            </Column>
        </DataTable>

        <Dialog v-model:visible="detailDialog" header="Audit Log Details" :style="{ width: '50vw' }" modal dismissableMask>
            <div v-if="selectedLog" class="flex flex-col gap-4">
                <div class="grid grid-cols-2 gap-4">
                    <div>
                        <div class="font-bold text-sm text-muted-color">LOG ID</div>
                        <div>{{ selectedLog.id }}</div>
                    </div>
                    <div>
                        <div class="font-bold text-sm text-muted-color">TIMESTAMP</div>
                        <div>{{ formatDate(selectedLog.timestamp) }}</div>
                    </div>
                    <div>
                        <div class="font-bold text-sm text-muted-color">USER</div>
                        <div>{{ selectedLog.userName || 'System' }} ({{ selectedLog.userId || 'N/A' }})</div>
                    </div>
                    <div>
                        <div class="font-bold text-sm text-muted-color">IP ADDRESS</div>
                        <div>{{ selectedLog.ipAddress || 'N/A' }}</div>
                    </div>
                    <div>
                        <div class="font-bold text-sm text-muted-color">ACTION</div>
                        <div>{{ selectedLog.action }}</div>
                    </div>
                    <div>
                        <div class="font-bold text-sm text-muted-color">ENTITY</div>
                        <div>{{ selectedLog.entityName }} ({{ selectedLog.entityId }})</div>
                    </div>
                </div>

                <div v-if="selectedLog.reason">
                    <div class="font-bold text-sm text-muted-color">REASON</div>
                    <div class="p-2 bg-surface-50 dark:bg-surface-900 border-round">{{ selectedLog.reason }}</div>
                </div>

                <div>
                    <div class="font-bold text-sm text-muted-color">USER AGENT</div>
                    <div class="text-xs break-all">{{ selectedLog.userAgent }}</div>
                </div>

                <TabView>
                    <TabPanel header="Changed Properties">
                        <pre class="text-xs p-3 bg-surface-50 dark:bg-surface-900 overflow-auto max-h-60 border-round">{{ formatJson(selectedLog.changedProperties) }}</pre>
                    </TabPanel>
                    <TabPanel header="New Values">
                        <pre class="text-xs p-3 bg-surface-50 dark:bg-surface-900 overflow-auto max-h-60 border-round">{{ formatJson(selectedLog.newValues) }}</pre>
                    </TabPanel>
                    <TabPanel header="Old Values">
                        <pre class="text-xs p-3 bg-surface-50 dark:bg-surface-900 overflow-auto max-h-60 border-round">{{ formatJson(selectedLog.oldValues) }}</pre>
                    </TabPanel>
                </TabView>
            </div>
        </Dialog>
    </div>
</template>

<style scoped>
pre {
    margin: 0;
    white-space: pre-wrap;
}
</style>
