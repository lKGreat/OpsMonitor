<template>
  <div class="stack">
    <h2>通知渠道</h2>
    <div class="card stack">
      <h3>新增钉钉渠道</h3>
      <div class="row">
        <input v-model="form.name" placeholder="渠道名" />
        <input v-model="form.config.webhook" placeholder="Webhook" />
        <input v-model="form.config.secret" placeholder="Secret (optional)" />
        <button @click="create">创建</button>
      </div>
      <p class="error" v-if="error">{{ error }}</p>
    </div>
    <div class="card">
      <table class="table">
        <thead>
          <tr>
            <th>ID</th>
            <th>名称</th>
            <th>类型</th>
            <th>Webhook</th>
            <th>Secret</th>
            <th>启用</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="c in channels" :key="c.id">
            <td>{{ c.id }}</td>
            <td>{{ c.name }}</td>
            <td>{{ c.type }}</td>
            <td>{{ c.webhookMasked }}</td>
            <td>{{ c.hasSecret ? 'YES' : 'NO' }}</td>
            <td>{{ c.isEnabled ? 'YES' : 'NO' }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue';
import { apiGet, apiPost } from '../api';

const channels = ref<any[]>([]);
const error = ref('');
const form = reactive({
  type: 'DINGTALK',
  name: '',
  config: {
    webhook: '',
    secret: ''
  },
  isEnabled: true
});

async function load() {
  channels.value = await apiGet('/api/channels');
}

async function create() {
  error.value = '';
  try {
    await apiPost('/api/channels', form);
    form.name = '';
    form.config.webhook = '';
    form.config.secret = '';
    await load();
  } catch (e: any) {
    error.value = e.message;
  }
}

onMounted(load);
</script>
