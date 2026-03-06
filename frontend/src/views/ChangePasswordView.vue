<template>
  <div class="card" style="max-width: 460px; margin: 48px auto;">
    <h2>首次登录请修改密码</h2>
    <div class="stack">
      <label>
        当前密码
        <input v-model="form.currentPassword" type="password" />
      </label>
      <label>
        新密码（至少8位）
        <input v-model="form.newPassword" type="password" />
      </label>
      <button :disabled="loading" @click="submit">提交</button>
      <p v-if="error" class="error">{{ error }}</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { apiPost, apiGet } from '../api';
import { authStore } from '../auth';

const router = useRouter();
const loading = ref(false);
const error = ref('');
const form = reactive({
  currentPassword: '',
  newPassword: ''
});

async function submit() {
  loading.value = true;
  error.value = '';
  try {
    await apiPost('/api/auth/change-password', form);
    const me = await apiGet<any>('/api/auth/me');
    authStore.user = me;
    router.push('/dashboard');
  } catch (e: any) {
    error.value = e.message;
  } finally {
    loading.value = false;
  }
}
</script>
