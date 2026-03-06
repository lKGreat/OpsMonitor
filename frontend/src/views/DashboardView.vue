<template>
  <div class="stack">
    <h2>{{ t('dashboard.title') }}</h2>
    <div class="row">
      <div class="card">
        <div class="muted">{{ t('dashboard.totalMonitors') }}</div>
        <div>{{ summary.totalMonitors }}</div>
      </div>
      <div class="card">
        <div class="muted">{{ t('dashboard.enabledMonitors') }}</div>
        <div>{{ summary.enabledMonitors }}</div>
      </div>
      <div class="card">
        <div class="muted">{{ t('dashboard.firingAlerts') }}</div>
        <div>{{ summary.firingAlerts }}</div>
      </div>
      <div class="card">
        <div class="muted">{{ t('dashboard.resolvedToday') }}</div>
        <div>{{ summary.resolvedToday }}</div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, reactive } from 'vue';
import { useI18n } from 'vue-i18n';
import { apiGet } from '../api';

const { t } = useI18n();
const summary = reactive({
  totalMonitors: 0,
  enabledMonitors: 0,
  firingAlerts: 0,
  resolvedToday: 0
});

onMounted(async () => {
  const data = await apiGet<any>('/api/dashboard/summary');
  Object.assign(summary, data);
});
</script>
