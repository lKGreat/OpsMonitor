# Repository Guidelines

## Project Structure & Module Organization
- `src/OpsMonitor.Api`: ASP.NET Core API, domain/services, hosted workers, middleware, and static hosting.
- `src/OpsMonitor.Api/Contracts`: request/response contracts shared across controllers.
- `frontend`: Vue 3 + Vite client (`src/views`, router, auth, i18n).
- `tests/OpsMonitor.Tests`: xUnit unit tests for backend services and localization.
- Frontend production assets are emitted to `src/OpsMonitor.Api/wwwroot`; treat `wwwroot/assets/*` as generated output.

## Build, Test, and Development Commands
- `dotnet restore`: restore backend/test dependencies.
- `dotnet build OpsMonitor.sln`: compile API and tests.
- `dotnet run --project src/OpsMonitor.Api/OpsMonitor.Api.csproj`: run API locally (default `http://localhost:5000`).
- `cd frontend && npm install && npm run dev`: run Vite dev server (`http://localhost:5173`) with `/api` proxy.
- `cd frontend && npm run build`: build frontend into backend `wwwroot`.
- `dotnet test`: run unit tests.
- `npm run test:e2e` (repo root): run Playwright CRUD flow tests (starts API + frontend via `playwright.config.ts`).

## Coding Style & Naming Conventions
- Backend targets `.NET 8`, `LangVersion 10`, `Nullable` enabled; keep warnings at zero.
- Use 4-space indentation in C# and preserve existing brace/newline style.
- Naming: `PascalCase` for types/methods/properties, `camelCase` for locals/parameters, interfaces prefixed with `I`.
- Vue SFCs use `PascalCase` file names (for example `MonitorDetailView.vue`); keep route/page components in `frontend/src/views`.
- No dedicated lint config is committed; follow existing formatting in touched files and keep diffs minimal.

## Testing Guidelines
- Unit tests use xUnit in `tests/OpsMonitor.Tests/*Tests.cs`.
- Prefer test names in `Method_Scenario_ExpectedResult` style.
- Add or update tests for any changed service logic, API contract behavior, or localization/security paths.
- For UI changes, update/add Playwright specs in `frontend/e2e/*.spec.ts` and verify locally.

## Commit & Pull Request Guidelines
- Use concise, imperative commit subjects (examples from history: `Add bilingual support in OpsMonitor`, `fix: handle empty API responses in frontend client`).
- Keep each commit focused on one logical change.
- PRs should include: purpose/scope, test evidence (`dotnet test`, `npm run test:e2e` when relevant), linked issue(s), and screenshots for UI updates.

## Security & Configuration Tips
- Set strong values for `Jwt:SigningKey` and `Security:ConfigEncryptionKey` in `src/OpsMonitor.Api/appsettings*.json`.
- Do not commit real secrets or production credentials.
- Default seeded admin credentials are for local bootstrap only; rotate immediately outside local development.
