<template>
  <div class="stack">
    <h2>{{ t('channels.title') }}</h2>
    <div class="card stack">
      <h3>{{ t('channels.createDingTalk') }}</h3>
      <div class="row">
        <input v-model="form.name" :placeholder="t('channels.channelName')" />
        <input v-model="form.config.webhook" :placeholder="t('channels.webhook')" />
        <input v-model="form.config.secret" :placeholder="t('channels.secret')" />
        <button @click="create">{{ t('common.create') }}</button>
      </div>
      <p class="error" v-if="error">{{ error }}</p>
    </div>
    <div class="card">
      <table class="table">
        <thead>
          <tr>
            <th>{{ t('channels.id') }}</th>
            <th>{{ t('channels.name') }}</th>
            <th>{{ t('channels.type') }}</th>
            <th>{{ t('channels.webhook') }}</th>
            <th>{{ t('channels.secret') }}</th>
            <th>{{ t('channels.enabled') }}</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="c in channels" :key="c.id">
            <td>{{ c.id }}</td>
            <td>{{ c.name }}</td>
            <td>{{ c.type }}</td>
            <td>{{ c.webhookMasked }}</td>
            <td>{{ c.hasSecret ? t('common.yes') : t('common.no') }}</td>
            <td>{{ c.isEnabled ? t('common.yes') : t('common.no') }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { apiGet, apiPost } from '../api';

const channels = ref<any[]>([]);
const error = ref('');
const { t } = useI18n();
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
