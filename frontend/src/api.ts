import { authStore, logout } from './auth';
import { getCurrentLocale } from './i18n';

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
  headers['Accept-Language'] = getCurrentLocale();
  if (authStore.token) {
    headers.Authorization = `Bearer ${authStore.token}`;
  }

  const response = await fetch(url, { ...init, headers });
  if (response.status === 401) {
    logout();
    window.location.href = '/login';
    throw new Error('Unauthorized');
  }
  if (!response.ok) {
    const text = await response.text();
    const parsed = tryParseApiError(text);
    throw new Error(parsed?.message || text || `Request failed: ${response.status}`);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  const responseText = await response.text();
  if (!responseText.trim()) {
    return undefined as T;
  }

  return JSON.parse(responseText) as T;
}

function tryParseApiError(text: string): { code?: string; message?: string } | null {
  if (!text.trim()) {
    return null;
  }

  try {
    const parsed = JSON.parse(text) as { code?: string; message?: string };
    return parsed && typeof parsed === 'object' ? parsed : null;
  } catch {
    return null;
  }
}
