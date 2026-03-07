import { reactive } from 'vue';

type UserInfo = {
  id: number;
  userName: string;
  displayName: string;
  role: string;
  requirePasswordChange: boolean;
};

const AUTH_STORAGE_KEY = 'opsmonitor.auth';

function loadAuth(): { token: string; user: UserInfo | null } {
  if (typeof window === 'undefined') {
    return { token: '', user: null };
  }

  try {
    const raw = window.localStorage.getItem(AUTH_STORAGE_KEY);
    if (!raw) {
      return { token: '', user: null };
    }

    const parsed = JSON.parse(raw) as { token?: string; user?: UserInfo | null };
    return {
      token: parsed.token ?? '',
      user: parsed.user ?? null
    };
  } catch {
    return { token: '', user: null };
  }
}

function persistAuth(): void {
  if (typeof window === 'undefined') {
    return;
  }

  window.localStorage.setItem(
    AUTH_STORAGE_KEY,
    JSON.stringify({
      token: authStore.token,
      user: authStore.user
    })
  );
}

const initialAuth = loadAuth();

export const authStore = reactive({
  token: initialAuth.token as string,
  user: initialAuth.user as UserInfo | null
});

export function isLoggedIn(): boolean {
  return !!authStore.token;
}

export function isAdmin(): boolean {
  return authStore.user?.role === 'Admin';
}

export function setAuth(token: string, user: UserInfo): void {
  authStore.token = token;
  authStore.user = user;
  persistAuth();
}

export function logout() {
  authStore.token = '';
  authStore.user = null;
  if (typeof window !== 'undefined') {
    window.localStorage.removeItem(AUTH_STORAGE_KEY);
  }
}
