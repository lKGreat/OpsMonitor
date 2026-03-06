<template>
  <div class="stack">
    <h2>{{ t('channels.title') }}</h2>
    <div class="card stack">
      <h3>{{ t('channels.createDingTalk') }}</h3>
      <div class="row">
        <input data-testid="channel-create-name" v-model="form.name" :placeholder="t('channels.channelName')" />
        <input data-testid="channel-create-webhook" v-model="form.config.webhook" :placeholder="t('channels.webhook')" />
        <input data-testid="channel-create-secret" v-model="form.config.secret" :placeholder="t('channels.secret')" />
        <button data-testid="channel-create-submit" @click="create">{{ t('common.create') }}</button>
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
            <th>{{ t('monitors.actions') }}</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="c in channels" :key="c.id">
            <td>{{ c.id }}</td>
            <td>
              <input v-if="editingId === c.id" :data-testid="`channel-edit-name-${c.id}`" v-model="editForm.name" />
              <span v-else>{{ c.name }}</span>
            </td>
            <td>{{ c.type }}</td>
            <td>
              <input v-if="editingId === c.id" :data-testid="`channel-edit-webhook-${c.id}`" v-model="editForm.config.webhook" :placeholder="c.webhookMasked" />
              <span v-else>{{ c.webhookMasked }}</span>
            </td>
            <td>
              <input v-if="editingId === c.id" :data-testid="`channel-edit-secret-${c.id}`" v-model="editForm.config.secret" :placeholder="t('channels.secret')" />
              <span v-else>{{ c.hasSecret ? t('common.yes') : t('common.no') }}</span>
            </td>
            <td>
              <select v-if="editingId === c.id" :data-testid="`channel-edit-enabled-${c.id}`" v-model="editForm.isEnabled">
                <option :value="true">{{ t('common.yes') }}</option>
                <option :value="false">{{ t('common.no') }}</option>
              </select>
              <span v-else>{{ c.isEnabled ? t('common.yes') : t('common.no') }}</span>
            </td>
            <td class="row">
              <button v-if="editingId !== c.id" :data-testid="`channel-edit-${c.id}`" @click="startEdit(c)">{{ t('common.edit') }}</button>
              <button v-if="editingId !== c.id" class="danger" :data-testid="`channel-delete-${c.id}`" @click="remove(c.id)">{{ t('common.delete') }}</button>
              <template v-else>
                <button :data-testid="`channel-save-${c.id}`" @click="save(c.id)">{{ t('common.save') }}</button>
                <button class="danger" :data-testid="`channel-cancel-${c.id}`" @click="cancelEdit">{{ t('common.cancel') }}</button>
              </template>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { apiDelete, apiGet, apiPost, apiPut } from '../api';

const channels = ref<any[]>([]);
const error = ref('');
const editingId = ref<number | null>(null);
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
const editForm = reactive({
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

function startEdit(channel: any) {
  editingId.value = channel.id;
  editForm.type = channel.type;
  editForm.name = channel.name;
  editForm.config.webhook = '';
  editForm.config.secret = '';
  editForm.isEnabled = channel.isEnabled;
}

function cancelEdit() {
  editingId.value = null;
}

async function save(id: number) {
  error.value = '';
  if (!editForm.config.webhook.trim()) {
    error.value = t('channels.webhookRequiredForUpdate');
    return;
  }

  try {
    await apiPut(`/api/channels/${id}`, editForm);
    editingId.value = null;
    await load();
  } catch (e: any) {
    error.value = e.message;
  }
}

async function remove(id: number) {
  error.value = '';
  try {
    await apiDelete(`/api/channels/${id}`);
    if (editingId.value === id) {
      editingId.value = null;
    }
    await load();
  } catch (e: any) {
    error.value = e.message;
  }
}

onMounted(load);
</script>
