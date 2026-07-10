# Stage 2 Bootstrap Checkpoint

Status: complete for the scaffold milestone.

This checkpoint records what was completed before moving into Stage 3. It is intentionally explicit so the repo can be reviewed by engineering, product, cloud, and security stakeholders without guessing what is real.

## Completed Scope

- Monorepo layout with `apps/web`, `src/*`, `tests/*`, `docs/*`, and `infra/*`.
- React `19.2.0` and Vite `8.1.2` web app in `apps/web`.
- ASP.NET Core API targeting `net10.0`.
- Clean Architecture project split:
  - `src/StadiumOps.Api`
  - `src/StadiumOps.Application`
  - `src/StadiumOps.Domain`
  - `src/StadiumOps.Infrastructure`
- xUnit unit and API integration test projects.
- Vitest and Playwright test setup for the web app.
- Root npm workspace scripts.
- `.gitignore`, `.env.example`, app-specific env template, Dockerfile, Docker Compose, and Cloud Run service template.
- Design checkpoint documentation before deeper UI build-out.

## Implemented Backend Foundation

- ASP.NET Identity Core users and roles.
- JWT access tokens.
- Hashed refresh-token persistence.
- RBAC constants and role-protected policies.
- Audit log table and sensitive-action logging points.
- SQL Server EF Core provider with development in-memory fallback.
- Initial enterprise database migration.
- Health and readiness checks.
- ProblemDetails errors, response envelopes, correlation IDs, CORS, and rate limiting.
- Vertex AI Gemini and Firebase FCM adapters with explicit missing-configuration behavior.

## Implemented API Surface

Base path: `/api/v1`.

- Auth: register, login, refresh, logout, profile.
- Fan/navigation: today's matches, stadium list, stadium POIs, route requests.
- AI: chat and conversation history.
- Incidents: report, list, status transition.
- Operations: overview and crowd zones.
- Notifications: list and operations broadcast.
- Realtime foundation: authenticated SignalR operations hub at `/hubs/operations`.

## Implemented Frontend Shell

- Login and register views.
- Role-aware navigation.
- Fan dashboard.
- Operations command center.
- AI assistant.
- Navigation and map readiness panel.
- Incident reporting and triage.
- Notification center.
- High-contrast mode and persisted session state.

## Verification Baseline

Known passing commands from the scaffold milestone:

```powershell
dotnet build StadiumOps.slnx --no-restore -m:1 /nr:false
dotnet test StadiumOps.slnx --no-build -m:1 /nr:false
npm run build
npm run test:web:run
npm run test:e2e --workspace apps/web
```

EF migrations were generated with the installed `dotnet ef` global tool. The tool is `10.0.8` while runtime packages are `10.0.9`, so EF prints a patch-version warning.

## Stage 3 Entry Criteria

Stage 3 can proceed because the repo has a runnable API, runnable frontend shell, persistence model, migrations, test projects, config templates, and cloud-readiness docs. Stage 3 work should refine the enterprise architecture, service boundaries, event contracts, database design, cloud deployment model, security model, and scalability plan.
