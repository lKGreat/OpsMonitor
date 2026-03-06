<template>
  <div class="card" style="max-width: 420px; margin: 48px auto;">
    <h2>{{ t('login.title') }}</h2>
    <div class="stack">
      <label>
        {{ t('login.userName') }}
        <input data-testid="login-username" v-model="form.userName" />
      </label>
      <label>
        {{ t('login.password') }}
        <input data-testid="login-password" v-model="form.password" type="password" />
      </label>
      <button data-testid="login-submit" :disabled="loading" @click="submit">{{ t('login.submit') }}</button>
      <p class="error" v-if="error">{{ error }}</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { apiPost } from '../api';
import { authStore } from '../auth';

const router = useRouter();
const { t } = useI18n();
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
    const response = await apiPost<{
      accessToken?: string;
      AccessToken?: string;
      user?: any;
      User?: any;
    }>('/api/auth/login', form);

    const accessToken = response.accessToken ?? response.AccessToken ?? '';
    const user = response.user ?? response.User ?? null;
    if (!accessToken || !user) {
      throw new Error(t('login.invalidResponse'));
    }

    authStore.token = accessToken;
    authStore.user = user;
    router.push('/dashboard');
  } catch (e: any) {
    error.value = e.message;
  } finally {
    loading.value = false;
  }
}
</script>
