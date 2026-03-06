import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './frontend/e2e',
  fullyParallel: false,
  workers: 1,
  reporter: 'list',
  use: {
    baseURL: 'http://127.0.0.1:5173',
    trace: 'on-first-retry'
  },
  webServer: [
    {
      command: 'dotnet run --project src/OpsMonitor.Api/OpsMonitor.Api.csproj --urls http://127.0.0.1:5000',
      url: 'http://127.0.0.1:5000/swagger/index.html',
      reuseExistingServer: false,
      timeout: 120000
    },
    {
      command: 'npm run dev -- --host 127.0.0.1 --port 5173',
      cwd: './frontend',
      url: 'http://127.0.0.1:5173/login',
      reuseExistingServer: false,
      timeout: 120000
    }
  ]
});
