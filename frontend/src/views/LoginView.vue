<template>
  <div class="card" style="max-width: 420px; margin: 48px auto;">
    <h2>登录 OpsMonitor</h2>
    <div class="stack">
      <label>
        用户名
        <input v-model="form.userName" />
      </label>
      <label>
        密码
        <input v-model="form.password" type="password" />
      </label>
      <button :disabled="loading" @click="submit">登录</button>
      <p class="error" v-if="error">{{ error }}</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { apiPost } from '../api';
import { authStore } from '../auth';

const router = useRouter();
const loading = ref(false);
const error = ref('');
const form = reactive({
  userName: 'admin',
  password: 'ChangeMe123!'
});

async function submit() {
  loading.value = true;
  error.value = '';
  try {
    const response = await apiPost<{ accessToken: string; expiresAt: string; user: any }>('/api/auth/login', form);
    authStore.token = response.accessToken;
    authStore.user = response.user;
    router.push(response.user?.requirePasswordChange ? '/change-password' : '/dashboard');
  } catch (e: any) {
    error.value = e.message;
  } finally {
    loading.value = false;
  }
}
</script>
