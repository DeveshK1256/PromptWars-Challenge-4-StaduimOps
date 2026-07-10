# Architecture

For the staged enterprise packet, read:

- [Stage 2 Bootstrap Checkpoint](stage-2-bootstrap-checkpoint.md)
- [Stage 3 Enterprise Architecture](stage-3-enterprise-architecture.md)
- [Stage 3 Backend Foundation Checkpoint](stage-3-backend-foundation-checkpoint.md)
- [Stage 4 AI Intelligence Checkpoint](stage-4-ai-intelligence-checkpoint.md)
- [Next Stage Generative AI Architecture](next-stage-generative-ai-architecture.md)

## Monorepo

- `apps/web`: React/Vite frontend.
- `src/StadiumOps.Api`: minimal API, auth endpoints, health checks, error handling.
- `src/StadiumOps.Application`: DTOs, roles, service contracts, business helpers.
- `src/StadiumOps.Domain`: operational entities and shared entity base.
- `src/StadiumOps.Infrastructure`: Identity, EF Core, SQL Server persistence, cloud adapters.
- `tests/*`: xUnit unit and API integration tests.
- `infra/*`: Cloud Run and compose scaffolding.
- `docs/*`: design, cloud, architecture, and staged checkpoint artifacts.

## Runtime Flow

1. User authenticates through ASP.NET Identity endpoints.
2. API issues JWT access token and hashed refresh token.
3. Frontend stores session in `localStorage` and sends bearer tokens.
4. RBAC policies protect operations, incident triage, notification broadcast, and admin-oriented paths.
5. EF Core persists operational data to SQL Server in production.
6. Vertex AI and Firebase adapters are invoked only when configured; otherwise API returns explicit service errors.
7. Integration events are prepared through the SQL Server outbox and can be published to Pub/Sub workers.
8. Operations clients can connect to the authenticated SignalR hub at `/hubs/operations`.

## API Standards

- Base path: `/api/v1`.
- Successful responses use `{ success, data, error, correlationId }`.
- Errors use ASP.NET ProblemDetails with `correlationId`.
- Lists use page metadata.
- Sensitive operations write audit log records.
- Emergency/AI guidance is advisory and does not override venue protocols.
