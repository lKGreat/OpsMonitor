<template>
  <div class="stack">
    <h2>{{ t('users.title') }}</h2>
    <div class="card stack">
      <h3>{{ t('users.createUser') }}</h3>
      <div class="row">
        <input data-testid="user-create-username" v-model="form.userName" :placeholder="t('users.userName')" />
        <input data-testid="user-create-display-name" v-model="form.displayName" :placeholder="t('users.displayName')" />
        <select data-testid="user-create-role" v-model="form.role">
          <option value="User">{{ t('value.roleUser') }}</option>
          <option value="Admin">{{ t('value.roleAdmin') }}</option>
        </select>
        <input data-testid="user-create-password" v-model="form.password" type="password" :placeholder="t('users.password')" />
        <button data-testid="user-create-submit" @click="create">{{ t('common.create') }}</button>
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
            <th>{{ t('monitors.actions') }}</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="u in users" :key="u.id">
            <td>{{ u.id }}</td>
            <td>{{ u.userName }}</td>
            <td>
              <input v-if="editingId === u.id" :data-testid="`user-edit-display-name-${u.id}`" v-model="editForm.displayName" />
              <span v-else>{{ u.displayName }}</span>
            </td>
            <td>
              <select v-if="editingId === u.id" :data-testid="`user-edit-role-${u.id}`" v-model="editForm.role">
                <option value="User">{{ t('value.roleUser') }}</option>
                <option value="Admin">{{ t('value.roleAdmin') }}</option>
              </select>
              <span v-else>{{ u.role === 'Admin' ? t('value.roleAdmin') : t('value.roleUser') }}</span>
            </td>
            <td>{{ u.requirePasswordChange ? t('common.yes') : t('common.no') }}</td>
            <td class="row">
              <button v-if="editingId !== u.id" :data-testid="`user-edit-${u.id}`" @click="startEdit(u)">{{ t('common.edit') }}</button>
              <button v-if="editingId !== u.id" class="danger" :data-testid="`user-delete-${u.id}`" @click="remove(u.id)">{{ t('common.delete') }}</button>
              <template v-else>
                <input :data-testid="`user-edit-new-password-${u.id}`" v-model="editForm.newPassword" type="password" :placeholder="t('users.newPasswordOptional')" />
                <button :data-testid="`user-save-${u.id}`" @click="save(u.id)">{{ t('common.save') }}</button>
                <button class="danger" :data-testid="`user-cancel-${u.id}`" @click="cancelEdit">{{ t('common.cancel') }}</button>
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

const users = ref<any[]>([]);
const error = ref('');
const editingId = ref<number | null>(null);
const { t } = useI18n();
const form = reactive({
  userName: '',
  displayName: '',
  role: 'User',
  password: ''
});
const editForm = reactive({
  displayName: '',
  role: 'User',
  isEnabled: true,
  newPassword: ''
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

function startEdit(user: any) {
  editingId.value = user.id;
  editForm.displayName = user.displayName;
  editForm.role = user.role;
  editForm.isEnabled = true;
  editForm.newPassword = '';
}

function cancelEdit() {
  editingId.value = null;
}

async function save(id: number) {
  error.value = '';
  try {
    await apiPut(`/api/users/${id}`, {
      displayName: editForm.displayName,
      role: editForm.role,
      isEnabled: editForm.isEnabled,
      newPassword: editForm.newPassword || null
    });
    editingId.value = null;
    await load();
  } catch (e: any) {
    error.value = e.message;
  }
}

async function remove(id: number) {
  error.value = '';
  try {
    await apiDelete(`/api/users/${id}`);
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
