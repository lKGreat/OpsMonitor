<template>
  <div class="stack">
    <div class="row">
      <h2>告警中心</h2>
      <select v-model="state" @change="load">
        <option value="">ALL</option>
        <option value="FIRING">FIRING</option>
        <option value="RESOLVED">RESOLVED</option>
      </select>
      <button @click="load">刷新</button>
    </div>
    <div class="card">
      <table class="table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Monitor</th>
            <th>Rule</th>
            <th>Severity</th>
            <th>State</th>
            <th>Message</th>
            <th>First</th>
            <th>Last</th>
            <th>Action</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="a in alerts" :key="a.id">
            <td>{{ a.id }}</td>
            <td>{{ a.monitorId }}</td>
            <td>{{ a.ruleType }}</td>
            <td>{{ a.severity }}</td>
            <td>{{ a.state }}</td>
            <td>{{ a.message }}</td>
            <td>{{ a.firstTriggeredAt }}</td>
            <td>{{ a.lastTriggeredAt }}</td>
            <td><button @click="ack(a.id)">Ack</button></td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { apiGet, apiPost } from '../api';

const state = ref('');
const alerts = ref<any[]>([]);

async function load() {
  const query = state.value ? `?state=${state.value}` : '';
  alerts.value = await apiGet(`/api/alerts${query}`);
}

async function ack(id: number) {
  await apiPost(`/api/alerts/${id}/ack`, { note: '' });
  await load();
}

onMounted(load);
</script>
