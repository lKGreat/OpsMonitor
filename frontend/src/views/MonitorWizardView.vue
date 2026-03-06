<template>
  <div class="stack">
    <h2>{{ isEdit ? '编辑监控项' : '创建监控项' }}</h2>
    <div class="card stack">
      <div class="muted">Step {{ step }} / 5</div>

      <div v-if="step === 1" class="stack">
        <label>名称 <input v-model="form.name" /></label>
        <label>类型
          <select v-model="form.type" :disabled="isEdit">
            <option value="LINK">LINK</option>
            <option value="CERT">CERT</option>
          </select>
        </label>
        <label>分组 <input v-model="form.groupName" /></label>
      </div>

      <div v-else-if="step === 2" class="stack">
        <label>URL/域名 <input v-model="form.target.urlOrHost" placeholder="https://api.example.com/health 或 example.com" /></label>
        <label>端口 <input v-model.number="form.target.port" type="number" /></label>
        <label>Path <input v-model="form.target.path" /></label>
      </div>

      <div v-else-if="step === 3" class="stack">
        <label>频率(秒) <input v-model.number="form.policy.intervalSec" type="number" /></label>
        <label>超时(ms) <input v-model.number="form.policy.timeoutMs" type="number" /></label>
        <label>重试次数 <input v-model.number="form.policy.retryCount" type="number" /></label>
        <label v-if="form.type === 'LINK'">成功码规则 <input v-model="form.policy.successCodeRule" /></label>
      </div>

      <div v-else-if="step === 4" class="stack">
        <label v-if="form.type === 'LINK'">连续失败阈值
          <input v-model.number="form.policy.failThreshold" type="number" />
        </label>
        <label v-else>证书阈值JSON
          <input v-model="form.policy.certExpireDaysThresholdsJson" />
        </label>
      </div>

      <div v-else class="stack">
        <div class="muted">通知渠道（钉钉）</div>
        <div v-if="channels.length === 0" class="muted">
          暂无可用渠道，请先到 Channels 页面创建。
        </div>
        <label v-for="c in channels" :key="c.id" class="row">
          <input type="checkbox" :value="c.id" v-model="selectedChannelIds" />
          <span>{{ c.name }} (ID: {{ c.id }})</span>
        </label>
      </div>

      <div class="row">
        <button v-if="step > 1" @click="step--">上一步</button>
        <button v-if="step < 5" @click="step++">下一步</button>
        <button v-if="step === 5" @click="submit">{{ isEdit ? '保存' : '创建' }}</button>
      </div>
      <p v-if="error" class="error">{{ error }}</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { apiGet, apiPost, apiPut } from '../api';

const router = useRouter();
const route = useRoute();
const step = ref(1);
const error = ref('');
const channels = ref<any[]>([]);
const selectedChannelIds = ref<number[]>([]);
const monitorId = Number(route.params.id || 0);
const isEdit = monitorId > 0;

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
    channelIdsJson: '[]'
  }
});

async function loadChannels() {
  channels.value = await apiGet<any[]>('/api/channels');
}

async function loadMonitorForEdit() {
  if (!isEdit) return;
  const m = await apiGet<any>(`/api/monitors/${monitorId}`);
  Object.assign(form, m);
  try {
    selectedChannelIds.value = JSON.parse(m.policy.channelIdsJson || '[]');
  } catch {
    selectedChannelIds.value = [];
  }
}

async function submit() {
  error.value = '';
  form.policy.channelIdsJson = JSON.stringify(selectedChannelIds.value || []);
  try {
    if (isEdit) {
      await apiPut(`/api/monitors/${monitorId}`, form);
      router.push(`/monitors/${monitorId}`);
    } else {
      const resp = await apiPost<{ id: number }>('/api/monitors', form);
      router.push(`/monitors/${resp.id}`);
    }
  } catch (e: any) {
    error.value = e.message;
  }
}

onMounted(async () => {
  await loadChannels();
  await loadMonitorForEdit();
});
</script>
