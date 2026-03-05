<template>
  <div class="layout">
    <aside v-if="authed" class="sidebar">
      <h1>OpsMonitor</h1>
      <nav>
        <RouterLink to="/dashboard">Dashboard</RouterLink>
        <RouterLink to="/monitors">Monitors</RouterLink>
        <RouterLink to="/alerts">Alerts</RouterLink>
        <RouterLink to="/settings/channels">Channels</RouterLink>
        <RouterLink v-if="isAdminUser" to="/settings/users">Users</RouterLink>
      </nav>
      <div class="user-panel">
        <div>{{ authStore.user?.displayName }}</div>
        <div class="muted">{{ authStore.user?.role }}</div>
        <button @click="doLogout">Logout</button>
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

const router = useRouter();
const authed = computed(() => isLoggedIn());
const isAdminUser = computed(() => isAdmin());

function doLogout() {
  logout();
  router.push('/login');
}
</script>
