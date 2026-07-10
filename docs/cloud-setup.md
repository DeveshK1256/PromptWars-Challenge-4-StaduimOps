# Cloud Setup

## Required Services

- Cloud Run for `StadiumOps.Api`.
- Cloud SQL for SQL Server.
- Vertex AI Gemini in the configured project/location.
- Firebase Cloud Messaging with a service-account JSON file.
- Google Maps JavaScript API browser key restricted to the deployed frontend origins.

## Local Secrets

Use environment variables, user secrets, or a local `.env` file ignored by Git. Do not commit secrets.

```powershell
dotnet user-secrets init --project src\StadiumOps.Api\StadiumOps.Api.csproj
dotnet user-secrets set "Jwt:SigningKey" "<long-random-secret>" --project src\StadiumOps.Api\StadiumOps.Api.csproj
dotnet user-secrets set "ConnectionStrings:StadiumOps" "<sql-server-connection-string>" --project src\StadiumOps.Api\StadiumOps.Api.csproj
dotnet user-secrets set "VertexAI:ProjectId" "<gcp-project-id>" --project src\StadiumOps.Api\StadiumOps.Api.csproj
dotnet user-secrets set "Firebase:ServiceAccountPath" "<absolute-path-to-service-account-json>" --project src\StadiumOps.Api\StadiumOps.Api.csproj
```

## Live Integration Tests

Live integration tests should be gated by `RUN_LIVE_INTEGRATION_TESTS=true` and must fail loudly when SQL Server, Vertex AI, Firebase, or Maps settings are absent. This scaffold includes the configuration boundary; additional live tests can be added once credentials are available.

## Notes

`gcloud` and `firebase` CLIs were not installed in the local environment during scaffold creation, so deployment provisioning is documented rather than automated.

## Demo Seed Data

Local development and automated tests use explicit demo seed data only. Seeded stadium, match, route, crowd, transport, sustainability, and role records are scaffolding records for product verification and must not be treated as live tournament data.
