<template>
  <div class="stack">
    <div class="row">
      <h2>Monitors</h2>
      <RouterLink to="/monitors/new"><button>新建监控</button></RouterLink>
      <button @click="load">刷新</button>
    </div>
    <div class="card">
      <table class="table">
        <thead>
          <tr>
            <th>ID</th>
            <th>名称</th>
            <th>类型</th>
            <th>分组</th>
            <th>状态</th>
            <th>最近探测</th>
            <th>动作</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="m in monitors" :key="m.id">
            <td>{{ m.id }}</td>
            <td><RouterLink :to="`/monitors/${m.id}`">{{ m.name }}</RouterLink></td>
            <td>{{ m.type }}</td>
            <td>{{ m.groupName || '-' }}</td>
            <td>{{ m.lastIsSuccess === null ? '-' : (m.lastIsSuccess ? 'OK' : 'FAIL') }}</td>
            <td>{{ m.lastCheckedAt || '-' }}</td>
            <td class="row">
              <RouterLink :to="`/monitors/${m.id}/edit`"><button>编辑</button></RouterLink>
              <button v-if="!m.isEnabled" @click="toggle(m.id, true)">启用</button>
              <button v-else class="danger" @click="toggle(m.id, false)">停用</button>
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
import { apiGet, apiPost } from '../api';

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

async function load() {
  monitors.value = await apiGet<MonitorItem[]>('/api/monitors');
}

async function toggle(id: number, enable: boolean) {
  await apiPost(`/api/monitors/${id}/${enable ? 'enable' : 'disable'}`);
  await load();
}

onMounted(load);
</script>
