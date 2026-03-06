<template>
  <div class="stack">
    <div class="row">
      <h2>{{ t('alerts.title') }}</h2>
      <select v-model="state" @change="load">
        <option value="">{{ t('common.all') }}</option>
        <option value="FIRING">{{ t('value.stateFiring') }}</option>
        <option value="RESOLVED">{{ t('value.stateResolved') }}</option>
      </select>
      <button @click="load">{{ t('common.refresh') }}</button>
    </div>
    <div class="card">
      <table class="table">
        <thead>
          <tr>
            <th>ID</th>
            <th>{{ t('alerts.monitor') }}</th>
            <th>{{ t('alerts.rule') }}</th>
            <th>{{ t('alerts.severity') }}</th>
            <th>{{ t('alerts.state') }}</th>
            <th>{{ t('alerts.message') }}</th>
            <th>{{ t('alerts.first') }}</th>
            <th>{{ t('alerts.last') }}</th>
            <th>{{ t('alerts.action') }}</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="a in alerts" :key="a.id">
            <td>{{ a.id }}</td>
            <td>{{ a.monitorId }}</td>
            <td>{{ a.ruleType }}</td>
            <td>{{ a.severity }}</td>
            <td>{{ stateLabel(a.state) }}</td>
            <td>{{ a.message }}</td>
            <td>{{ a.firstTriggeredAt }}</td>
            <td>{{ a.lastTriggeredAt }}</td>
            <td><button @click="ack(a.id)">{{ t('alerts.ack') }}</button></td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { apiGet, apiPost } from '../api';

const state = ref('');
const alerts = ref<any[]>([]);
const { t } = useI18n();

async function load() {
  const query = state.value ? `?state=${state.value}` : '';
  alerts.value = await apiGet(`/api/alerts${query}`);
}

async function ack(id: number) {
  await apiPost(`/api/alerts/${id}/ack`, { note: '' });
  await load();
}

function stateLabel(value: string): string {
  if (value === 'FIRING') {
    return t('value.stateFiring');
  }

  if (value === 'RESOLVED') {
    return t('value.stateResolved');
  }

  return value;
}

onMounted(load);
</script>
