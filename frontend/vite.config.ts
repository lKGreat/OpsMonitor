import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';
import { fileURLToPath, URL } from 'node:url';

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      'vue-i18n': fileURLToPath(new URL('./src/shims/vue-i18n.ts', import.meta.url))
    }
  },
  server: {
    host: '127.0.0.1',
    port: 5173,
    proxy: {
      '/api': 'http://localhost:5000'
    }
  },
  build: {
    outDir: '../src/OpsMonitor.Api/wwwroot',
    emptyOutDir: true
  }
});
