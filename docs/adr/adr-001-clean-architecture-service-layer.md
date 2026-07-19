# ADR 001: Separation of Business Logic via Application Service Layer and Repository Pattern

## Context and Problem
Initially, the minimal API endpoints in the API project interacted directly with `StadiumOpsDbContext` and were responsible for business rules (such as computing incident prioritizations, assigning teams, and handling event outbox insertions). This violates Clean Architecture principles, increases coupling between the presentation layer and data access, and complicates unit testing of pure business rules.

## Decision
We decided to extract all database operations and business rules from the endpoint handlers into:
1. **Repository Pattern:** Under `IIncidentRepository` and `IncidentRepository` to handle queries and commits.
2. **Service Layer Abstraction:** Under `IIncidentService` and `IncidentService` in the `StadiumOps.Application` core, containing business logic rules, audit writing, and integration outbox publication.

## Consequences
* **Decoupling:** Minimal API endpoints are now only responsible for routing, HTTP requests/responses, authorization checks, and telemetry logging.
* **Cohesion:** Incident-specific rules (prioritizing severity and assigning teams) are centralized in `IncidentService`.
* **Testing:** We can now test the incident creation and status workflows by mocking interfaces rather than needing an active database provider.
