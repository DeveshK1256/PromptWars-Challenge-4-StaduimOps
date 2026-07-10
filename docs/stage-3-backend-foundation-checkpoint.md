# Stage 3 Backend Foundation Checkpoint

Status: complete for the backend foundation milestone.

Stage 3 turns the scaffold into a production-shaped API foundation. The goal is not to finish every product module yet; it is to make authentication, authorization, persistence, migrations, logging, health, and sensitive-event handling reliable enough for the core modules in the next stage.

## Completed Scope

- ASP.NET Core API targeting `net10.0`.
- ASP.NET Identity Core users and roles.
- JWT bearer authentication.
- Hashed refresh-token persistence.
- Refresh-token rotation and logout revocation.
- RBAC policies for operations, incident response, and admin access.
- Self-registration restricted to permitted public roles only:
  - `RegisteredFan`
  - `Volunteer`
- EF Core SQL Server model with migrations.
- Development in-memory provider for local/demo runs.
- Seeded roles and clearly labeled demo operational seed data.
- JSON console logging with correlation ID scope.
- OpenAPI document generation in development.
- ProblemDetails error responses with correlation ID.
- Response envelopes for successful API responses.
- Readiness and liveness health endpoints.
- JSON readiness response including dependency checks.
- Audit writer service for sensitive actions.
- Integration event outbox writer service for event-driven workflows.
- Authenticated SignalR operations hub at `/hubs/operations`.

## Sensitive Actions Covered

The backend now records audit events for:

- User registration.
- User login.
- Refresh-token rotation.
- Logout/session revocation.
- Incident creation.
- Incident status updates.
- Notification broadcast requests.
- AI conversation completion.

## Event Outbox Coverage

The backend now writes outbox events for:

- Incident reported.
- Incident status changed.
- Notification broadcast requested.
- AI conversation completed.

Outbox rows are stored in SQL Server through the `IntegrationEventOutbox` table and remain `Pending` until a future publisher moves them to Pub/Sub or another broker.

## Health Endpoints

```text
GET /health/live
GET /health/ready
```

`/health/ready` returns JSON with:

- status
- correlationId
- totalDurationMs
- dependency checks

## API Foundation Status

Implemented API groups:

- `/api/v1/auth`
- `/api/v1/matches`
- `/api/v1/stadiums`
- `/api/v1/navigation`
- `/api/v1/ai`
- `/api/v1/incidents`
- `/api/v1/operations`
- `/api/v1/crowd`
- `/api/v1/notifications`

The API remains a modular monolith in this milestone. The Stage 3 architecture document defines the service boundaries and extraction path.

## Verification

Passing verification commands:

```powershell
dotnet build StadiumOps.slnx --no-restore -m:1 /nr:false
dotnet test StadiumOps.slnx --no-build -m:1 /nr:false
npm run build
npm run test:web:run
```

Current backend test coverage includes:

- unauthorized protected endpoint handling
- register and read seeded fan data
- privileged role self-registration rejection
- JSON readiness output
- OpenAPI document availability in development
- incident audit and outbox persistence

## Next Stage Entry

The next stage can build the deeper core modules on this foundation:

- route-selection service rules
- notification publisher/background worker
- Pub/Sub adapter
- operations realtime push messages
- transport, volunteer, sustainability, and analytics endpoints
- richer AI guardrails and retrieval
