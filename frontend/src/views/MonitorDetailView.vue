<template>
  <div class="stack">
    <div class="row">
      <h2>{{ t('monitorDetail.title') }} #{{ monitor?.id }}</h2>
      <button @click="load">{{ t('common.refresh') }}</button>
    </div>
    <div class="card" v-if="monitor">
      <div><strong>{{ monitor.name }}</strong> ({{ monitor.type }})</div>
      <div class="muted">{{ t('monitorDetail.group') }}: {{ monitor.groupName || t('common.unknown') }}</div>
      <div class="muted">{{ t('monitorDetail.target') }}: {{ monitor.target.urlOrHost }}:{{ monitor.target.port }}{{ monitor.target.path || '' }}</div>
      <div class="muted">{{ t('monitorDetail.frequency') }}: {{ monitor.policy.intervalSec }}s / {{ t('monitorDetail.timeout') }}: {{ monitor.policy.timeoutMs }}ms</div>
    </div>
    <div class="card">
      <h3>{{ t('monitorDetail.recentResults') }}</h3>
      <table class="table">
        <thead>
          <tr>
            <th>{{ t('monitorDetail.time') }}</th>
            <th>{{ t('monitorDetail.status') }}</th>
            <th>{{ t('monitorDetail.duration') }}</th>
            <th>{{ t('monitorDetail.errorType') }}</th>
            <th>{{ t('monitorDetail.http') }}</th>
            <th>{{ t('monitorDetail.certDaysLeft') }}</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="r in results" :key="r.id">
            <td>{{ r.checkedAt }}</td>
            <td>{{ r.isSuccess ? t('common.ok') : t('common.fail') }}</td>
            <td>{{ r.durationMs }}</td>
            <td>{{ r.errorType }}</td>
            <td>{{ r.httpStatusCode ?? t('common.unknown') }}</td>
            <td>{{ r.certDaysLeft ?? t('common.unknown') }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { apiGet } from '../api';

const route = useRoute();
const { t } = useI18n();
const monitor = ref<any>(null);
const results = ref<any[]>([]);

async function load() {
  const id = route.params.id;
  monitor.value = await apiGet(`/api/monitors/${id}`);
  results.value = await apiGet(`/api/monitors/${id}/results?days=7`);
}

onMounted(load);
</script>
