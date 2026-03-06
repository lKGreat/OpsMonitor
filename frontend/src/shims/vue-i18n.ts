import { computed, inject, type App, type ComputedRef, type InjectionKey, type Ref, ref } from 'vue';

type MessageTree = Record<string, string | MessageTree>;
type MessageMap = Record<string, MessageTree>;

type I18nGlobal = {
  locale: Ref<string>;
  t: (key: string, params?: Record<string, string | number>) => string;
};

type I18nInstance = {
  install: (app: App) => void;
  global: I18nGlobal;
};

type CreateI18nOptions = {
  locale: string;
  fallbackLocale?: string;
  messages: MessageMap;
};

const I18N_KEY: InjectionKey<I18nGlobal> = Symbol('opsmonitor.i18n');

function resolveMessage(messages: MessageMap, locale: string, key: string): string | undefined {
  const segments = key.split('.');
  let current: string | MessageTree | undefined = messages[locale];
  for (const segment of segments) {
    if (!current || typeof current === 'string') {
      return undefined;
    }
    current = current[segment];
  }
  return typeof current === 'string' ? current : undefined;
}

function interpolate(template: string, params?: Record<string, string | number>): string {
  if (!params) {
    return template;
  }

  return template.replace(/\{(\w+)\}/g, (_, name: string) => String(params[name] ?? `{${name}}`));
}

export function createI18n(options: CreateI18nOptions): I18nInstance {
  const locale = ref(options.locale);
  const fallbackLocale = options.fallbackLocale ?? options.locale;

  const t = (key: string, params?: Record<string, string | number>) => {
    const message = resolveMessage(options.messages, locale.value, key)
      ?? resolveMessage(options.messages, fallbackLocale, key)
      ?? key;
    return interpolate(message, params);
  };

  const global: I18nGlobal = { locale, t };

  return {
    install(app: App) {
      app.provide(I18N_KEY, global);
    },
    global
  };
}

export function useI18n(): { t: I18nGlobal['t']; locale: ComputedRef<string> } {
  const i18n = inject(I18N_KEY);
  if (!i18n) {
    throw new Error('i18n has not been installed');
  }

  return {
    t: i18n.t,
    locale: computed(() => i18n.locale.value)
  };
}
