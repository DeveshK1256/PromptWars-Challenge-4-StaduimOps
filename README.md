# Smart Stadium & Tournament Operations Platform

Enterprise scaffold for a Generative AI-enabled Smart Stadium and Tournament Operations Platform for FIFA World Cup 2026.

## Implemented

- ASP.NET Core `net10.0` API with Clean Architecture project split.
- ASP.NET Identity JWT auth, refresh-token persistence, RBAC role policies, audit logs, and seeded roles.
- EF Core SQL Server model and initial migration for stadium, match, route, crowd, incident, notification, AI, sustainability, transport, volunteer, and audit entities.
- Stage 3 backend foundation with OpenAPI, structured JSON logging, readiness checks, audit writer, and integration event outbox writer.
- Stage 4 AI intelligence foundation with multi-agent orchestration, prompt guardrails, confidence metadata, AI analytics, knowledge-base schema, and AI metadata endpoints.
- React `19.2.0` + Vite `8.1.2` frontend with accessible auth, fan console, command center, AI assistant, incidents, notifications, route request, map readiness, and persisted session/theme state.
- Live integration adapters for Vertex AI Gemini and Firebase Cloud Messaging that fail clearly when credentials are missing.
- Health checks, ProblemDetails errors, correlation IDs, rate limiting, CORS, env templates, Docker/Cloud Run scaffolding, unit/API/frontend/E2E test hooks.
- Stage 3 enterprise architecture docs, authenticated SignalR operations hub, and SQL Server event outbox migration.

## Architecture Docs

- [Design checkpoint](docs/design-checkpoint.md)
- [Stage 2 bootstrap checkpoint](docs/stage-2-bootstrap-checkpoint.md)
- [Stage 3 enterprise architecture](docs/stage-3-enterprise-architecture.md)
- [Stage 3 backend foundation checkpoint](docs/stage-3-backend-foundation-checkpoint.md)
- [Stage 4 AI intelligence checkpoint](docs/stage-4-ai-intelligence-checkpoint.md)
- [Next stage generative AI architecture](docs/next-stage-generative-ai-architecture.md)
- [Cloud setup](docs/cloud-setup.md)

## Pending Credentials

The scaffold does not fake cloud connectivity. Set real local secrets before using live integrations:

- `ConnectionStrings__StadiumOps`
- `Jwt__SigningKey`
- `VertexAI__ProjectId` plus Google Application Default Credentials
- `Firebase__ServiceAccountPath`
- `VITE_GOOGLE_MAPS_API_KEY`

## Local Development

```powershell
dotnet restore StadiumOps.slnx
dotnet build StadiumOps.slnx -m:1 /nr:false
dotnet run --project src\StadiumOps.Api\StadiumOps.Api.csproj
npm install
npm run dev:web
```

The API defaults to explicit development demo mode with in-memory persistence. Production defaults to SQL Server and requires a connection string.

## Verification

```powershell
dotnet test StadiumOps.slnx --no-build -m:1 /nr:false
npm run build
npm run test:web:run
npm run test:e2e --workspace apps/web
```

The installed `dotnet ef` global tool is `10.0.8` while EF runtime packages are `10.0.9`, so migration commands may print a tool-version warning. The migration was generated successfully.
