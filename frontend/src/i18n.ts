import { createI18n } from 'vue-i18n';

export const LOCALE_STORAGE_KEY = 'opsmonitor.locale';
export const SUPPORTED_LOCALES = ['zh-CN', 'en-US'] as const;
export type SupportedLocale = (typeof SUPPORTED_LOCALES)[number];

const messages = {
  'zh-CN': {
    app: {
      dashboard: '总览',
      monitors: '监控',
      alerts: '告警',
      channels: '渠道',
      users: '用户',
      logout: '退出登录',
      localeLabel: '语言',
      localeZh: '中文',
      localeEn: 'English'
    },
    common: {
      refresh: '刷新',
      create: '创建',
      delete: '删除',
      edit: '编辑',
      save: '保存',
      cancel: '取消',
      previous: '上一步',
      next: '下一步',
      yes: '是',
      no: '否',
      all: '全部',
      ok: '成功',
      fail: '失败',
      unknown: '-'
    },
    login: {
      title: '登录 OpsMonitor',
      userName: '用户名',
      password: '密码',
      submit: '登录'
    },
    dashboard: {
      title: '总览',
      totalMonitors: '监控总数',
      enabledMonitors: '启用监控',
      firingAlerts: '当前告警',
      resolvedToday: '今日恢复'
    },
    monitors: {
      title: '监控列表',
      newMonitor: '新建监控',
      id: 'ID',
      name: '名称',
      type: '类型',
      group: '分组',
      status: '状态',
      lastCheck: '最近探测',
      actions: '操作',
      enable: '启用',
      disable: '停用'
    },
    monitorWizard: {
      title: '创建监控项',
      step: '步骤 {step} / {total}',
      name: '名称',
      type: '类型',
      group: '分组',
      target: 'URL/域名',
      targetPlaceholder: 'https://api.example.com/health 或 example.com',
      port: '端口',
      path: '路径',
      intervalSec: '频率(秒)',
      timeoutMs: '超时(ms)',
      retryCount: '重试次数',
      successCodeRule: '成功码规则',
      failThreshold: '连续失败阈值',
      certThresholdJson: '证书阈值JSON',
      channelIdsJson: '通知渠道 ID 列表(JSON，如 [1,2])'
    },
    monitorDetail: {
      title: '监控详情',
      group: '分组',
      target: '目标',
      frequency: '频率',
      timeout: '超时',
      recentResults: '最近探测结果',
      time: '时间',
      status: '状态',
      duration: '耗时',
      errorType: '错误类型',
      http: 'HTTP',
      certDaysLeft: '证书剩余天数'
    },
    alerts: {
      title: '告警中心',
      monitor: '监控',
      rule: '规则',
      severity: '级别',
      state: '状态',
      message: '消息',
      first: '首次触发',
      last: '最近触发',
      action: '操作',
      ack: '确认'
    },
    channels: {
      title: '通知渠道',
      createDingTalk: '新增钉钉渠道',
      channelName: '渠道名',
      webhook: 'Webhook',
      secret: '密钥（可选）',
      id: 'ID',
      name: '名称',
      type: '类型',
      enabled: '启用',
      webhookRequiredForUpdate: '编辑渠道时需要填写完整 Webhook'
    },
    users: {
      title: '用户管理',
      createUser: '创建用户',
      userName: '用户名',
      displayName: '显示名',
      role: '角色',
      password: '密码',
      id: 'ID',
      requirePasswordChange: '需改密',
      newPasswordOptional: '新密码（可选）'
    },
    value: {
      stateFiring: '触发中',
      stateResolved: '已恢复',
      roleAdmin: '管理员',
      roleUser: '普通用户'
    }
  },
  'en-US': {
    app: {
      dashboard: 'Dashboard',
      monitors: 'Monitors',
      alerts: 'Alerts',
      channels: 'Channels',
      users: 'Users',
      logout: 'Logout',
      localeLabel: 'Language',
      localeZh: '中文',
      localeEn: 'English'
    },
    common: {
      refresh: 'Refresh',
      create: 'Create',
      delete: 'Delete',
      edit: 'Edit',
      save: 'Save',
      cancel: 'Cancel',
      previous: 'Previous',
      next: 'Next',
      yes: 'YES',
      no: 'NO',
      all: 'ALL',
      ok: 'OK',
      fail: 'FAIL',
      unknown: '-'
    },
    login: {
      title: 'Sign In to OpsMonitor',
      userName: 'User Name',
      password: 'Password',
      submit: 'Sign In'
    },
    dashboard: {
      title: 'Dashboard',
      totalMonitors: 'Total Monitors',
      enabledMonitors: 'Enabled Monitors',
      firingAlerts: 'Firing Alerts',
      resolvedToday: 'Resolved Today'
    },
    monitors: {
      title: 'Monitors',
      newMonitor: 'New Monitor',
      id: 'ID',
      name: 'Name',
      type: 'Type',
      group: 'Group',
      status: 'Status',
      lastCheck: 'Last Check',
      actions: 'Actions',
      enable: 'Enable',
      disable: 'Disable'
    },
    monitorWizard: {
      title: 'Create Monitor',
      step: 'Step {step} / {total}',
      name: 'Name',
      type: 'Type',
      group: 'Group',
      target: 'URL/Host',
      targetPlaceholder: 'https://api.example.com/health or example.com',
      port: 'Port',
      path: 'Path',
      intervalSec: 'Interval (sec)',
      timeoutMs: 'Timeout (ms)',
      retryCount: 'Retry Count',
      successCodeRule: 'Success Code Rule',
      failThreshold: 'Failure Threshold',
      certThresholdJson: 'Cert Threshold JSON',
      channelIdsJson: 'Channel IDs (JSON, e.g. [1,2])'
    },
    monitorDetail: {
      title: 'Monitor Detail',
      group: 'Group',
      target: 'Target',
      frequency: 'Frequency',
      timeout: 'Timeout',
      recentResults: 'Recent Probe Results',
      time: 'Time',
      status: 'Status',
      duration: 'Duration',
      errorType: 'Error Type',
      http: 'HTTP',
      certDaysLeft: 'Cert Days Left'
    },
    alerts: {
      title: 'Alert Center',
      monitor: 'Monitor',
      rule: 'Rule',
      severity: 'Severity',
      state: 'State',
      message: 'Message',
      first: 'First',
      last: 'Last',
      action: 'Action',
      ack: 'Acknowledge'
    },
    channels: {
      title: 'Notification Channels',
      createDingTalk: 'Add DingTalk Channel',
      channelName: 'Channel Name',
      webhook: 'Webhook',
      secret: 'Secret (optional)',
      id: 'ID',
      name: 'Name',
      type: 'Type',
      enabled: 'Enabled',
      webhookRequiredForUpdate: 'A full webhook is required to update a channel'
    },
    users: {
      title: 'Users',
      createUser: 'Create User',
      userName: 'User Name',
      displayName: 'Display Name',
      role: 'Role',
      password: 'Password',
      id: 'ID',
      requirePasswordChange: 'Require Password Change',
      newPasswordOptional: 'New Password (optional)'
    },
    value: {
      stateFiring: 'FIRING',
      stateResolved: 'RESOLVED',
      roleAdmin: 'Admin',
      roleUser: 'User'
    }
  }
};

function normalizeLocale(value: string | null | undefined): SupportedLocale {
  if (!value) {
    return 'zh-CN';
  }

  if (value.toLowerCase().startsWith('en')) {
    return 'en-US';
  }

  return 'zh-CN';
}

function getInitialLocale(): SupportedLocale {
  if (typeof window === 'undefined') {
    return 'zh-CN';
  }

  return normalizeLocale(window.localStorage.getItem(LOCALE_STORAGE_KEY));
}

const initialLocale = getInitialLocale();

if (typeof document !== 'undefined') {
  document.documentElement.lang = initialLocale;
}

export const i18n = createI18n({
  locale: initialLocale,
  fallbackLocale: 'zh-CN',
  messages
});

export function getCurrentLocale(): SupportedLocale {
  return normalizeLocale(i18n.global.locale.value);
}

export function setCurrentLocale(locale: SupportedLocale): void {
  const normalized = normalizeLocale(locale);
  i18n.global.locale.value = normalized;
  if (typeof window !== 'undefined') {
    window.localStorage.setItem(LOCALE_STORAGE_KEY, normalized);
  }
  if (typeof document !== 'undefined') {
    document.documentElement.lang = normalized;
  }
}
