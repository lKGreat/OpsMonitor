<template>
  <div class="stack">
    <h2>{{ t('monitorWizard.title') }}</h2>
    <div class="card stack">
      <div class="muted">{{ t('monitorWizard.step', { step, total: totalSteps }) }}</div>

      <div v-if="step === 1" class="stack">
        <label>{{ t('monitorWizard.name') }} <input v-model="form.name" /></label>
        <label>{{ t('monitorWizard.type') }}
          <select v-model="form.type">
            <option value="LINK">LINK</option>
            <option value="CERT">CERT</option>
          </select>
        </label>
        <label>{{ t('monitorWizard.group') }} <input v-model="form.groupName" /></label>
      </div>

      <div v-else-if="step === 2" class="stack">
        <label>{{ t('monitorWizard.target') }} <input v-model="form.target.urlOrHost" :placeholder="t('monitorWizard.targetPlaceholder')" /></label>
        <label>{{ t('monitorWizard.port') }} <input v-model.number="form.target.port" type="number" /></label>
        <label>{{ t('monitorWizard.path') }} <input v-model="form.target.path" /></label>
      </div>

      <div v-else-if="step === 3" class="stack">
        <label>{{ t('monitorWizard.intervalSec') }} <input v-model.number="form.policy.intervalSec" type="number" /></label>
        <label>{{ t('monitorWizard.timeoutMs') }} <input v-model.number="form.policy.timeoutMs" type="number" /></label>
        <label>{{ t('monitorWizard.retryCount') }} <input v-model.number="form.policy.retryCount" type="number" /></label>
        <label v-if="form.type === 'LINK'">{{ t('monitorWizard.successCodeRule') }} <input v-model="form.policy.successCodeRule" /></label>
      </div>

      <div v-else-if="step === 4" class="stack">
        <label v-if="form.type === 'LINK'">{{ t('monitorWizard.failThreshold') }}
          <input v-model.number="form.policy.failThreshold" type="number" />
        </label>
        <label v-else>{{ t('monitorWizard.certThresholdJson') }}
          <input v-model="form.policy.certExpireDaysThresholdsJson" />
        </label>
      </div>

      <div v-else class="stack">
        <label>{{ t('monitorWizard.channelIdsJson') }}
          <input v-model="form.policy.channelIdsJson" />
        </label>
      </div>

      <div class="row">
        <button v-if="step > 1" @click="step--">{{ t('common.previous') }}</button>
        <button v-if="step < totalSteps" @click="step++">{{ t('common.next') }}</button>
        <button v-if="step === totalSteps" @click="submit">{{ t('common.create') }}</button>
      </div>
      <p v-if="error" class="error">{{ error }}</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { apiPost } from '../api';

const router = useRouter();
const step = ref(1);
const totalSteps = 5;
const error = ref('');
const { t } = useI18n();
const form = reactive({
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
    latencyMsThreshold: null as number | null,
    certExpireDaysThresholdsJson: '[30,15,7,3,1]',
    channelIdsJson: '[1]'
  }
});

async function submit() {
  error.value = '';
  try {
    const resp = await apiPost<{ id: number }>('/api/monitors', form);
    router.push(`/monitors/${resp.id}`);
  } catch (e: any) {
    error.value = e.message;
  }
}
</script>
