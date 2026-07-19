# ADR 002: Stateless JWT Token Revocation via In-Memory Blacklisting Middleware

## Context and Problem
Stateless JWT access tokens cannot be easily revoked before their expiration time. In a stadium operations environment, when an operator logs out or an account is flagged for suspicious activity, the token must be instantly invalidated to prevent security bypass.

## Decision
We implemented a stateless JWT revocation mechanism:
1. When a user requests `/api/v1/auth/logout`, their current JWT signature is extracted.
2. The signature is registered in `IMemoryCache` with a TTL matching the token's remaining lifespan.
3. In `JwtBearerEvents.OnTokenValidated`, a middleware checks incoming JWTs against the blacklist and fails validation (returning `401 Unauthorized`) if a match is found.

## Consequences
* **Instant Revocation:** Access tokens are instantly invalidated on logout.
* **Low Latency:** Checking memory cache signatures is extremely fast (`< 1ms`) and prevents database round-trips.
* **Horizontal Scaling:** While currently in-memory, this mechanism is architected to utilize Redis in multi-instance production environments.
