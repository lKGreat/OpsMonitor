import { reactive } from 'vue';

type UserInfo = {
  id: number;
  userName: string;
  displayName: string;
  role: string;
  requirePasswordChange: boolean;
};

export const authStore = reactive({
  token: '' as string,
  user: null as UserInfo | null
});

export function isLoggedIn(): boolean {
  return !!authStore.token;
}

export function isAdmin(): boolean {
  return authStore.user?.role === 'Admin';
}

export function requiresPasswordChange(): boolean {
  return !!authStore.user?.requirePasswordChange;
}

export function logout() {
  authStore.token = '';
  authStore.user = null;
}
