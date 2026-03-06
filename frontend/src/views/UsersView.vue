<template>
  <div class="stack">
    <h2>{{ t('users.title') }}</h2>
    <div class="card stack">
      <h3>{{ t('users.createUser') }}</h3>
      <div class="row">
        <input v-model="form.userName" :placeholder="t('users.userName')" />
        <input v-model="form.displayName" :placeholder="t('users.displayName')" />
        <select v-model="form.role">
          <option value="User">{{ t('value.roleUser') }}</option>
          <option value="Admin">{{ t('value.roleAdmin') }}</option>
        </select>
        <input v-model="form.password" type="password" :placeholder="t('users.password')" />
        <button @click="create">{{ t('common.create') }}</button>
      </div>
      <p class="error" v-if="error">{{ error }}</p>
    </div>

    <div class="card">
      <table class="table">
        <thead>
          <tr>
            <th>{{ t('users.id') }}</th>
            <th>{{ t('users.userName') }}</th>
            <th>{{ t('users.displayName') }}</th>
            <th>{{ t('users.role') }}</th>
            <th>{{ t('users.requirePasswordChange') }}</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="u in users" :key="u.id">
            <td>{{ u.id }}</td>
            <td>{{ u.userName }}</td>
            <td>{{ u.displayName }}</td>
            <td>{{ u.role === 'Admin' ? t('value.roleAdmin') : t('value.roleUser') }}</td>
            <td>{{ u.requirePasswordChange ? t('common.yes') : t('common.no') }}</td>
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

const users = ref<any[]>([]);
const error = ref('');
const { t } = useI18n();
const form = reactive({
  userName: '',
  displayName: '',
  role: 'User',
  password: ''
});

async function load() {
  users.value = await apiGet('/api/users');
}

async function create() {
  error.value = '';
  try {
    await apiPost('/api/users', form);
    form.userName = '';
    form.displayName = '';
    form.password = '';
    await load();
  } catch (e: any) {
    error.value = e.message;
  }
}

onMounted(load);
</script>
