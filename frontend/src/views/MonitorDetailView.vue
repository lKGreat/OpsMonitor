<template>
  <div class="stack">
    <div class="row">
      <h2>{{ t('monitorDetail.title') }} #{{ monitor?.id }}</h2>
      <button data-testid="monitor-detail-refresh" @click="load">{{ t('common.refresh') }}</button>
      <button v-if="monitor && !editing" data-testid="monitor-edit" @click="startEdit">{{ t('common.edit') }}</button>
      <template v-if="monitor && editing">
        <button data-testid="monitor-save" @click="save">{{ t('common.save') }}</button>
        <button class="danger" data-testid="monitor-cancel" @click="cancelEdit">{{ t('common.cancel') }}</button>
      </template>
    </div>
    <div class="card stack" v-if="monitor && editing">
      <label>{{ t('monitors.name') }} <input data-testid="monitor-name-input" v-model="editForm.name" /></label>
      <label>{{ t('monitors.type') }}
        <select data-testid="monitor-type-input" v-model="editForm.type">
          <option value="LINK">LINK</option>
          <option value="CERT">CERT</option>
        </select>
      </label>
      <label>{{ t('monitorDetail.group') }} <input data-testid="monitor-group-input" v-model="editForm.groupName" /></label>
      <label>{{ t('monitorDetail.target') }} <input data-testid="monitor-target-input" v-model="editForm.target.urlOrHost" /></label>
      <label>{{ t('monitorWizard.port') }} <input data-testid="monitor-port-input" v-model.number="editForm.target.port" type="number" /></label>
      <label>{{ t('monitorWizard.path') }} <input data-testid="monitor-path-input" v-model="editForm.target.path" /></label>
      <label>{{ t('monitorWizard.intervalSec') }} <input data-testid="monitor-interval-input" v-model.number="editForm.policy.intervalSec" type="number" /></label>
      <label>{{ t('monitorWizard.timeoutMs') }} <input data-testid="monitor-timeout-input" v-model.number="editForm.policy.timeoutMs" type="number" /></label>
      <label>{{ t('monitorWizard.retryCount') }} <input data-testid="monitor-retry-input" v-model.number="editForm.policy.retryCount" type="number" /></label>
      <label>{{ t('monitorWizard.failThreshold') }} <input data-testid="monitor-fail-threshold-input" v-model.number="editForm.policy.failThreshold" type="number" /></label>
      <label>{{ t('monitorWizard.successCodeRule') }} <input data-testid="monitor-success-rule-input" v-model="editForm.policy.successCodeRule" /></label>
      <label>{{ t('monitorWizard.certThresholdJson') }} <input data-testid="monitor-cert-threshold-input" v-model="editForm.policy.certExpireDaysThresholdsJson" /></label>
      <label>{{ t('monitorWizard.channelIdsJson') }} <input data-testid="monitor-channel-ids-input" v-model="editForm.policy.channelIdsJson" /></label>
    </div>
    <div class="card" v-else-if="monitor">
      <div><strong>{{ monitor.name }}</strong> ({{ monitor.type }})</div>
      <div class="muted">{{ t('monitorDetail.group') }}: {{ monitor.groupName || t('common.unknown') }}</div>
      <div class="muted">{{ t('monitorDetail.target') }}: {{ monitor.target.urlOrHost }}:{{ monitor.target.port }}{{ monitor.target.path || '' }}</div>
      <div class="muted">{{ t('monitorDetail.frequency') }}: {{ monitor.policy.intervalSec }}s / {{ t('monitorDetail.timeout') }}: {{ monitor.policy.timeoutMs }}ms</div>
    </div>
    <p v-if="error" class="error">{{ error }}</p>
    <div class="card">
      <h3>{{ t('monitorDetail.recentResults') }}</h3>
      <table class="table">
        <thead>
          <tr>
            <th>{{ t('monitorDetail.time') }}</th>
            <th>{{ t('monitorDetail.status') }}</th>
            <th>{{ t('monitorDetail.duration') }}</th>
            <th>{{ t('monitorDetail.errorType') }}</th>
            <th>{{ t('monitorDetail.http') }}</th>
            <th>{{ t('monitorDetail.certDaysLeft') }}</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="r in results" :key="r.id">
            <td>{{ r.checkedAt }}</td>
            <td>{{ r.isSuccess ? t('common.ok') : t('common.fail') }}</td>
            <td>{{ r.durationMs }}</td>
            <td>{{ r.errorType }}</td>
            <td>{{ r.httpStatusCode ?? t('common.unknown') }}</td>
            <td>{{ r.certDaysLeft ?? t('common.unknown') }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue';
import { useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { apiGet, apiPut } from '../api';

type MonitorUpsertPayload = {
  name: string;
  type: string;
  groupName: string;
  tagsJson: string;
  target: {
    urlOrHost: string;
    port: number;
    path: string;
    useSni: boolean;
    headersJson: string;
  };
  policy: {
    intervalSec: number;
    timeoutMs: number;
    retryCount: number;
    failThreshold: number;
    successCodeRule: string;
    contentContains: string;
    latencyMsThreshold: number | null;
    certExpireDaysThresholdsJson: string;
    channelIdsJson: string;
  };
};

const route = useRoute();
const { t } = useI18n();
const monitor = ref<any>(null);
const results = ref<any[]>([]);
const editing = ref(false);
const error = ref('');
const editForm = reactive<MonitorUpsertPayload>({
  name: '',
  type: 'LINK',
  groupName: '',
  tagsJson: '',
  target: {
    urlOrHost: '',
    port: 443,
    path: '',
    useSni: true,
    headersJson: ''
  },
  policy: {
    intervalSec: 60,
    timeoutMs: 5000,
    retryCount: 1,
    failThreshold: 3,
    successCodeRule: '200-399',
    contentContains: '',
    latencyMsThreshold: null,
    certExpireDaysThresholdsJson: '[30,15,7,3,1]',
    channelIdsJson: '[1]'
  }
});

async function load() {
  const id = route.params.id;
  monitor.value = await apiGet(`/api/monitors/${id}`);
  results.value = await apiGet(`/api/monitors/${id}/results?days=7`);
}

function startEdit() {
  if (!monitor.value) {
    return;
  }

  error.value = '';
  editing.value = true;
  Object.assign(editForm, {
    name: monitor.value.name ?? '',
    type: monitor.value.type ?? 'LINK',
    groupName: monitor.value.groupName ?? '',
    tagsJson: monitor.value.tagsJson ?? '',
    target: {
      urlOrHost: monitor.value.target?.urlOrHost ?? '',
      port: monitor.value.target?.port ?? 443,
      path: monitor.value.target?.path ?? '',
      useSni: monitor.value.target?.useSni ?? true,
      headersJson: monitor.value.target?.headersJson ?? ''
    },
    policy: {
      intervalSec: monitor.value.policy?.intervalSec ?? 60,
      timeoutMs: monitor.value.policy?.timeoutMs ?? 5000,
      retryCount: monitor.value.policy?.retryCount ?? 1,
      failThreshold: monitor.value.policy?.failThreshold ?? 3,
      successCodeRule: monitor.value.policy?.successCodeRule ?? '200-399',
      contentContains: monitor.value.policy?.contentContains ?? '',
      latencyMsThreshold: monitor.value.policy?.latencyMsThreshold ?? null,
      certExpireDaysThresholdsJson: monitor.value.policy?.certExpireDaysThresholdsJson ?? '[30,15,7,3,1]',
      channelIdsJson: monitor.value.policy?.channelIdsJson ?? '[1]'
    }
  });
}

function cancelEdit() {
  editing.value = false;
}

async function save() {
  if (!monitor.value) {
    return;
  }

  error.value = '';
  try {
    await apiPut(`/api/monitors/${monitor.value.id}`, editForm);
    editing.value = false;
    await load();
  } catch (e: any) {
    error.value = e.message;
  }
}

onMounted(load);
</script>
