# OpsMonitor MVP

## 中文说明

### 功能概览
- 证书监控（`CERT`）：采集证书有效期、SAN/CN 匹配、证书链构建结果和到期天数告警。
- 链路监控（`LINK`）：执行 DNS/TCP/TLS/HTTP 探测，分类失败原因，支持连续失败触发告警。
- 告警中心：支持 `FIRING/RESOLVED` 状态，按 `MonitorId+RuleType` 去重，支持升级与恢复通知。
- 通知渠道：支持钉钉 Webhook（可选签名密钥），失败指数退避重试。
- 认证与授权：用户名密码登录（PBKDF2），JWT Bearer，角色 `Admin` / `User`。
- 前端 UI：Vue3 + Vite，覆盖登录、总览、监控创建/列表/详情、告警、渠道、用户页面。
- 双语支持：`zh-CN` 与 `en-US`，默认中文，手动切换并持久化。

### 技术栈
- 后端：ASP.NET Core (.NET 8, C# 10), SQLSugar, SQLite (WAL)。
- 前端：Vue3 + Vite + vue-i18n。

### 项目结构
- `src/OpsMonitor.Api`：后端 API、后台任务、静态资源托管。
- `frontend`：前端工程，构建输出到 `src/OpsMonitor.Api/wwwroot`。
- `tests/OpsMonitor.Tests`：单元测试。

### 快速开始
1. 配置 `src/OpsMonitor.Api/appsettings.json`：
   - `Jwt:SigningKey`（强随机密钥）
   - `Security:ConfigEncryptionKey`（强随机密钥）
   - 可选：种子管理员账号密码
2. 启动后端：
   ```bash
   dotnet restore
   dotnet run --project src/OpsMonitor.Api/OpsMonitor.Api.csproj
   ```
3. 启动前端开发环境：
   ```bash
   cd frontend
   npm install
   npm run dev
   ```
4. 构建前端到后端静态目录：
   ```bash
   cd frontend
   npm run build
   ```
5. 运行测试：
   ```bash
   dotnet test
   ```

### 默认登录信息
- 用户名：`admin`
- 密码：`ChangeMe123!`
- 首次登录默认启用改密标记（`RequirePasswordChange=true`）。

### 已实现 API
- 认证：`POST /api/auth/login`, `POST /api/auth/logout`, `GET /api/auth/me`
- 用户（Admin）：`GET/POST/PUT /api/users`
- 监控：`GET/POST /api/monitors`, `GET/PUT /api/monitors/{id}`, `POST /api/monitors/{id}/enable|disable`
- 结果：`GET /api/monitors/{id}/results?days=7|30`
- 告警：`GET /api/alerts?state=FIRING|RESOLVED`, `POST /api/alerts/{id}/ack`
- 渠道：`GET/POST/PUT /api/channels`
- 总览：`GET /api/dashboard/summary`

### 说明
- 当前 MVP 不包含静默/维护窗口（计划 V2 引入）。
- 调度与探测工作器以进程内 Hosted Service 运行。
- `mon_check_result` 数据按天清理。

---

## English

### Features
- Certificate monitoring (`CERT`): checks validity window, SAN/CN hostname match, chain validation, and days-left alerts.
- Link monitoring (`LINK`): performs DNS/TCP/TLS/HTTP probes, classifies failures, and triggers alerts on consecutive failures.
- Alert center: supports `FIRING/RESOLVED`, deduplicates by `MonitorId+RuleType`, and handles escalation/recovery notifications.
- Notification channels: DingTalk webhook support (optional signing secret) with exponential backoff retry.
- Auth/RBAC: username/password login (PBKDF2), JWT bearer auth, roles `Admin` / `User`.
- UI: Vue3 + Vite pages for login, dashboard, monitor wizard/list/detail, alerts, channels, and users.
- Bilingual support: `zh-CN` and `en-US`, defaulting to Chinese with manual switch + persistence.

### Stack
- Backend: ASP.NET Core (.NET 8, C# 10), SQLSugar, SQLite (WAL).
- Frontend: Vue3 + Vite + vue-i18n.

### Project Layout
- `src/OpsMonitor.Api`: backend API, hosted services, static hosting.
- `frontend`: Vue project; build output goes to `src/OpsMonitor.Api/wwwroot`.
- `tests/OpsMonitor.Tests`: unit tests.

### Quick Start
1. Configure `src/OpsMonitor.Api/appsettings.json`:
   - `Jwt:SigningKey` (strong key)
   - `Security:ConfigEncryptionKey` (strong key)
   - optional seed admin credentials
2. Run backend:
   ```bash
   dotnet restore
   dotnet run --project src/OpsMonitor.Api/OpsMonitor.Api.csproj
   ```
3. Run frontend dev server:
   ```bash
   cd frontend
   npm install
   npm run dev
   ```
4. Build frontend into backend `wwwroot`:
   ```bash
   cd frontend
   npm run build
   ```
5. Run tests:
   ```bash
   dotnet test
   ```

### Default Login
- Username: `admin`
- Password: `ChangeMe123!`
- `RequirePasswordChange=true` is enabled by default on the seeded account.

### Implemented APIs
- Auth: `POST /api/auth/login`, `POST /api/auth/logout`, `GET /api/auth/me`
- Users (Admin): `GET/POST/PUT /api/users`
- Monitors: `GET/POST /api/monitors`, `GET/PUT /api/monitors/{id}`, `POST /api/monitors/{id}/enable|disable`
- Results: `GET /api/monitors/{id}/results?days=7|30`
- Alerts: `GET /api/alerts?state=FIRING|RESOLVED`, `POST /api/alerts/{id}/ack`
- Channels: `GET/POST/PUT /api/channels`
- Dashboard: `GET /api/dashboard/summary`

### Notes
- Silence/maintenance windows are intentionally excluded from the current MVP (planned for V2).
- Scheduler and workers run in-process as hosted services.
- `mon_check_result` retention cleanup runs daily.
