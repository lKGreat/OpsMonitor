<template>
  <div class="stack">
    <div class="row">
      <h2>{{ t('monitors.title') }}</h2>
      <RouterLink to="/monitors/new"><button data-testid="monitor-create-entry">{{ t('monitors.newMonitor') }}</button></RouterLink>
      <button data-testid="monitor-refresh" @click="load">{{ t('common.refresh') }}</button>
    </div>
    <div class="card">
      <table class="table">
        <thead>
          <tr>
            <th>{{ t('monitors.id') }}</th>
            <th>{{ t('monitors.name') }}</th>
            <th>{{ t('monitors.type') }}</th>
            <th>{{ t('monitors.group') }}</th>
            <th>{{ t('monitors.status') }}</th>
            <th>{{ t('monitors.lastCheck') }}</th>
            <th>{{ t('monitors.actions') }}</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="m in monitors" :key="m.id">
            <td>{{ m.id }}</td>
            <td><RouterLink :to="`/monitors/${m.id}`" :data-testid="`monitor-link-${m.id}`">{{ m.name }}</RouterLink></td>
            <td>{{ m.type }}</td>
            <td>{{ m.groupName || t('common.unknown') }}</td>
            <td>{{ monitorStatusLabel(m.lastIsSuccess) }}</td>
            <td>{{ m.lastCheckedAt || t('common.unknown') }}</td>
            <td class="row">
              <button v-if="!m.isEnabled" @click="toggle(m.id, true)">{{ t('monitors.enable') }}</button>
              <button v-else class="danger" @click="toggle(m.id, false)">{{ t('monitors.disable') }}</button>
              <button class="danger" :data-testid="`monitor-delete-${m.id}`" @click="remove(m.id)">{{ t('common.delete') }}</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { RouterLink } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { apiDelete, apiGet, apiPost } from '../api';

type MonitorItem = {
  id: number;
  name: string;
  type: string;
  groupName?: string;
  isEnabled: boolean;
  lastIsSuccess: boolean | null;
  lastCheckedAt?: string;
};

const monitors = ref<MonitorItem[]>([]);
const { t } = useI18n();

async function load() {
  monitors.value = await apiGet<MonitorItem[]>('/api/monitors');
}

async function toggle(id: number, enable: boolean) {
  await apiPost(`/api/monitors/${id}/${enable ? 'enable' : 'disable'}`);
  await load();
}

async function remove(id: number) {
  await apiDelete(`/api/monitors/${id}`);
  await load();
}

function monitorStatusLabel(status: boolean | null): string {
  if (status === null) {
    return t('common.unknown');
  }

  return status ? t('common.ok') : t('common.fail');
}

onMounted(load);
</script>
