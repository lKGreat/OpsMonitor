<template>
  <div class="layout">
    <label v-if="!authed" class="locale-control locale-floating">
      <span>{{ t('app.localeLabel') }}</span>
      <select v-model="selectedLocale">
        <option value="zh-CN">{{ t('app.localeZh') }}</option>
        <option value="en-US">{{ t('app.localeEn') }}</option>
      </select>
    </label>

    <aside v-if="authed" class="sidebar">
      <h1>OpsMonitor</h1>
      <nav>
        <RouterLink to="/dashboard">{{ t('app.dashboard') }}</RouterLink>
        <RouterLink to="/monitors">{{ t('app.monitors') }}</RouterLink>
        <RouterLink to="/alerts">{{ t('app.alerts') }}</RouterLink>
        <RouterLink to="/settings/channels">{{ t('app.channels') }}</RouterLink>
        <RouterLink v-if="isAdminUser" to="/settings/users">{{ t('app.users') }}</RouterLink>
      </nav>
      <div class="user-panel">
        <label class="locale-control">
          <span>{{ t('app.localeLabel') }}</span>
          <select v-model="selectedLocale">
            <option value="zh-CN">{{ t('app.localeZh') }}</option>
            <option value="en-US">{{ t('app.localeEn') }}</option>
          </select>
        </label>
        <div>{{ authStore.user?.displayName }}</div>
        <div class="muted">{{ roleLabel }}</div>
        <button @click="doLogout">{{ t('app.logout') }}</button>
      </div>
    </aside>
    <main class="content">
      <RouterView />
    </main>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { RouterLink, RouterView, useRouter } from 'vue-router';
import { authStore, isAdmin, isLoggedIn, logout } from './auth';
import { getCurrentLocale, setCurrentLocale, type SupportedLocale } from './i18n';
import { useI18n } from 'vue-i18n';

const router = useRouter();
const { t } = useI18n();
const authed = computed(() => isLoggedIn());
const isAdminUser = computed(() => isAdmin());
const roleLabel = computed(() => {
  if (authStore.user?.role === 'Admin') {
    return t('value.roleAdmin');
  }
  if (authStore.user?.role === 'User') {
    return t('value.roleUser');
  }
  return authStore.user?.role || '-';
});
const selectedLocale = computed({
  get: () => getCurrentLocale(),
  set: (value) => setCurrentLocale(value as SupportedLocale)
});

function doLogout() {
  logout();
  router.push('/login');
}
</script>
