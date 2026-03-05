<template>
  <div class="stack">
    <h2>Dashboard</h2>
    <div class="row">
      <div class="card">
        <div class="muted">监控总数</div>
        <div>{{ summary.totalMonitors }}</div>
      </div>
      <div class="card">
        <div class="muted">启用监控</div>
        <div>{{ summary.enabledMonitors }}</div>
      </div>
      <div class="card">
        <div class="muted">当前告警</div>
        <div>{{ summary.firingAlerts }}</div>
      </div>
      <div class="card">
        <div class="muted">今日恢复</div>
        <div>{{ summary.resolvedToday }}</div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, reactive } from 'vue';
import { apiGet } from '../api';

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
