import { createRouter, createWebHistory } from 'vue-router';
import { isLoggedIn, isAdmin, requiresPasswordChange } from './auth';
import LoginView from './views/LoginView.vue';
import DashboardView from './views/DashboardView.vue';
import MonitorsView from './views/MonitorsView.vue';
import MonitorWizardView from './views/MonitorWizardView.vue';
import MonitorDetailView from './views/MonitorDetailView.vue';
import AlertsView from './views/AlertsView.vue';
import ChannelsView from './views/ChannelsView.vue';
import UsersView from './views/UsersView.vue';
import ChangePasswordView from './views/ChangePasswordView.vue';

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/login', component: LoginView },
    { path: '/change-password', component: ChangePasswordView, meta: { auth: true } },
    { path: '/', redirect: '/dashboard' },
    { path: '/dashboard', component: DashboardView, meta: { auth: true } },
    { path: '/monitors', component: MonitorsView, meta: { auth: true } },
    { path: '/monitors/new', component: MonitorWizardView, meta: { auth: true } },
    { path: '/monitors/:id/edit', component: MonitorWizardView, meta: { auth: true } },
    { path: '/monitors/:id', component: MonitorDetailView, meta: { auth: true } },
    { path: '/alerts', component: AlertsView, meta: { auth: true } },
    { path: '/settings/channels', component: ChannelsView, meta: { auth: true } },
    { path: '/settings/users', component: UsersView, meta: { auth: true, admin: true } }
  ]
});

router.beforeEach((to) => {
  if (to.meta.auth && !isLoggedIn()) {
    return '/login';
  }
  if (to.path !== '/change-password' && isLoggedIn() && requiresPasswordChange()) {
    return '/change-password';
  }
  if (to.meta.admin && !isAdmin()) {
    return '/dashboard';
  }
  return true;
});
