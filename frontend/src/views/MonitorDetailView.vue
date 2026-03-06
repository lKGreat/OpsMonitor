<template>
  <div class="stack">
    <div class="row">
      <h2>监控详情 #{{ monitor?.id }}</h2>
      <RouterLink v-if="monitor" :to="`/monitors/${monitor.id}/edit`"><button>编辑</button></RouterLink>
      <button @click="load">刷新</button>
    </div>
    <div class="card" v-if="monitor">
      <div><strong>{{ monitor.name }}</strong> ({{ monitor.type }})</div>
      <div class="muted">分组: {{ monitor.groupName || '-' }}</div>
      <div class="muted">目标: {{ monitor.target.urlOrHost }}:{{ monitor.target.port }}{{ monitor.target.path || '' }}</div>
      <div class="muted">频率: {{ monitor.policy.intervalSec }}s / 超时: {{ monitor.policy.timeoutMs }}ms</div>
    </div>
    <div class="card">
      <h3>最近探测结果</h3>
      <table class="table">
        <thead>
          <tr>
            <th>时间</th>
            <th>状态</th>
            <th>耗时</th>
            <th>错误类型</th>
            <th>HTTP</th>
            <th>证书剩余天数</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="r in results" :key="r.id">
            <td>{{ r.checkedAt }}</td>
            <td>{{ r.isSuccess ? 'OK' : 'FAIL' }}</td>
            <td>{{ r.durationMs }}</td>
            <td>{{ r.errorType }}</td>
            <td>{{ r.httpStatusCode ?? '-' }}</td>
            <td>{{ r.certDaysLeft ?? '-' }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { RouterLink, useRoute } from 'vue-router';
import { apiGet } from '../api';

const route = useRoute();
const monitor = ref<any>(null);
const results = ref<any[]>([]);

async function load() {
  const id = route.params.id;
  monitor.value = await apiGet(`/api/monitors/${id}`);
  results.value = await apiGet(`/api/monitors/${id}/results?days=7`);
}

onMounted(load);
</script>
