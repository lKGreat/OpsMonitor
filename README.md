# OpsMonitor MVP

MVP features:
- Cert monitor (`CERT`): NotBefore/NotAfter, SAN/CN hostname match, chain build, days-left alert.
- Link monitor (`LINK`): DNS/TCP/TLS/HTTP probe, failure classification, continuous-failure alert.
- Alert center: `FIRING/RESOLVED`, dedup by `MonitorId+RuleType`, escalation, recovery notification.
- Notifier: DingTalk webhook (+ optional sign secret), retry with exponential backoff.
- Auth/RBAC: username/password login (PBKDF2), JWT bearer, roles `Admin` / `User`.
- UI: Vue3 + Vite pages for login, dashboard, monitor wizard/list/detail, alerts, channels, users.

## Stack
- Backend: ASP.NET Core (.NET 8, C# 10), SQLSugar, SQLite (WAL).
- Frontend: Vue3 + Vite.

## Project Layout
- `src/OpsMonitor.Api`: backend API + hosted services + static hosting.
- `frontend`: Vue project, build output to `src/OpsMonitor.Api/wwwroot`.
- `tests/OpsMonitor.Tests`: unit tests.

## Quick Start
1. Configure `src/OpsMonitor.Api/appsettings.json`:
   - `Jwt:SigningKey` (strong key)
   - `Security:ConfigEncryptionKey` (strong key)
   - optional seed account/password.
2. Backend:
   ```bash
   dotnet restore
   dotnet run --project src/OpsMonitor.Api/OpsMonitor.Api.csproj
   ```
3. Frontend (dev):
   ```bash
   cd frontend
   npm install
   npm run dev
   ```
4. Frontend (build to backend wwwroot):
   ```bash
   cd frontend
   npm run build
   ```
5. Tests:
   ```bash
   dotnet test
   ```

## Default Login
- Username: `admin`
- Password: `ChangeMe123!`
- On first login, update password policy is enabled at account level (`RequirePasswordChange=true`).

## Implemented APIs
- Auth: `POST /api/auth/login`, `POST /api/auth/logout`, `GET /api/auth/me`
- Users(Admin): `GET/POST/PUT /api/users`
- Monitors: `GET/POST /api/monitors`, `GET/PUT /api/monitors/{id}`, `POST /api/monitors/{id}/enable|disable`
- Results: `GET /api/monitors/{id}/results?days=7|30`
- Alerts: `GET /api/alerts?state=FIRING|RESOLVED`, `POST /api/alerts/{id}/ack`
- Channels: `GET/POST/PUT /api/channels`
- Dashboard: `GET /api/dashboard/summary`

## Notes
- Current MVP intentionally excludes silence/maintenance windows (planned for V2).
- Scheduler and workers run in-process as hosted services.
- `mon_check_result` retention cleanup runs daily.
