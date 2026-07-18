# Cloud Setup

## Required Services

- Cloud Run for `StadiumOps.Api`.
- Cloud SQL for SQL Server.
- Vertex AI Gemini in the configured project/location.
- Firebase Cloud Messaging with a service-account JSON file.
- Google Maps JavaScript API browser key restricted to the deployed frontend origins.

## Frontend Deployment

The Netlify frontend should set:

```text
VITE_API_BASE_URL=https://<deployed-api-host>/api/v1
VITE_GOOGLE_MAPS_API_KEY=<restricted-browser-key>
```

If this value is absent, the static Netlify build uses an explicit browser-local demo adapter. This prevents the deployed site from calling `localhost` while keeping live cloud integrations unavailable until real backend configuration is provided.

If `VITE_GOOGLE_MAPS_API_KEY` is absent, the fan console uses the built-in stadium operations map so deployed demos still show POIs and route context. Configure a Google Maps JavaScript API key restricted to the Netlify domain to enable the live map.

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
