import { authStore, logout } from './auth';

const jsonHeaders: Record<string, string> = {
  'Content-Type': 'application/json'
};

export async function apiGet<T>(url: string): Promise<T> {
  return request<T>(url, { method: 'GET' });
}

export async function apiPost<T>(url: string, body?: unknown): Promise<T> {
  return request<T>(url, { method: 'POST', body: body ? JSON.stringify(body) : undefined });
}

export async function apiPut<T>(url: string, body?: unknown): Promise<T> {
  return request<T>(url, { method: 'PUT', body: body ? JSON.stringify(body) : undefined });
}

async function request<T>(url: string, init: RequestInit): Promise<T> {
  const headers: Record<string, string> = { ...jsonHeaders };
  if (authStore.token) {
    headers.Authorization = `Bearer ${authStore.token}`;
  }

  const response = await fetch(url, { ...init, headers });
  if (response.status === 403) {
    const body = await response.text();
    if (body.includes('PASSWORD_CHANGE_REQUIRED')) {
      window.location.href = '/change-password';
      throw new Error('Password change required');
    }
  }
  if (response.status === 401) {
    logout();
    window.location.href = '/login';
    throw new Error('Unauthorized');
  }
  if (!response.ok) {
    const text = await response.text();
    throw new Error(text || `Request failed: ${response.status}`);
  }
  const text = await response.text();
  if (!text) {
    return undefined as T;
  }
  try {
    return JSON.parse(text) as T;
  } catch {
    return text as T;
  }
}
