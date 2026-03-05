<template>
  <div class="stack">
    <h2>用户管理</h2>
    <div class="card stack">
      <h3>创建用户</h3>
      <div class="row">
        <input v-model="form.userName" placeholder="用户名" />
        <input v-model="form.displayName" placeholder="显示名" />
        <select v-model="form.role">
          <option value="User">User</option>
          <option value="Admin">Admin</option>
        </select>
        <input v-model="form.password" type="password" placeholder="密码" />
        <button @click="create">创建</button>
      </div>
      <p class="error" v-if="error">{{ error }}</p>
    </div>

    <div class="card">
      <table class="table">
        <thead>
          <tr>
            <th>ID</th>
            <th>UserName</th>
            <th>DisplayName</th>
            <th>Role</th>
            <th>RequirePasswordChange</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="u in users" :key="u.id">
            <td>{{ u.id }}</td>
            <td>{{ u.userName }}</td>
            <td>{{ u.displayName }}</td>
            <td>{{ u.role }}</td>
            <td>{{ u.requirePasswordChange ? 'YES' : 'NO' }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue';
import { apiGet, apiPost } from '../api';

const users = ref<any[]>([]);
const error = ref('');
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
