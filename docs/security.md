# 🛡️ Security Report & STRIDE Threat Modeling

This document lists the threat modeling profiles (STRIDE) and security control implementations for the Smart Stadium & Tournament Operations Platform.

---

## 🔍 STRIDE Threat Model

| Threat | Description | Mitigation Controls in StadiumOps |
| :--- | :--- | :--- |
| **Spoofing Identity** | Attacker logs in or intercepts requests to act as a manager. | • Strict ASP.NET Identity with password strength rules.<br>• Secure JWT tokens with signature validation. |
| **Tampering** | Attacker injects malicious data (XSS, SQLi, malware files). | • Input Sanitizer strips out HTML/XSS tags.<br>• EF Core parameterization prevents SQL Injection.<br>• Magic bytes scanner blocks executable uploads (ELF/MZ). |
| **Repudiation** | Attacker claims they did not perform an action (e.g. updating incident). | • Injected `IAuditWriter` records every state modification (user ID, action, resource, IP address, and correlation ID). |
| **Information Disclosure** | Unprivileged users gain access to sensitive incident lists. | • RBAC policies partition endpoints. Only `IncidentAccess` operators can access /incidents. |
| **Denial of Service** | Attackers flood endpoints or trigger large prompt loops. | • Global rate limiter restricts users/anonymous requests.<br>• Named `"ai-chat"` rate limits protect LLM compute.<br>• Strict prompt length budget (< 2000 chars) prevents buffer overload. |
| **Elevation of Privilege** | Attacker requests administrative roles during registration. | • Validation guards block registration with privileged roles. Operations/Admin roles must be assigned explicitly by supervisors. |

---

## 🧪 Pen-Test Scenarios & Integration Verification

Every security control is verified through automated integration test suites:

### 1. SQL Injection Parameterization
* **Scenario:** Attacker submits a payload like `'; DROP TABLE Users; --` in incident creation fields.
* **Verification:** Covered by `SqlInjection_AttemptOnIncidentCreation_IsMitigatedByParameterization` in `SecurityIntegrationTests.cs`. Asserts the SQL commands are parsed strictly as query parameter values rather than database commands.

### 2. CORS Verification
* **Scenario:** Hostile origins (e.g. `http://evilattacker.com`) attempt to request API endpoints.
* **Verification:** Covered by `Cors_DisallowedOrigin_HeadersDoNotReflectEvilOrigin` in `SecurityIntegrationTests.cs`. Asserts the CORS middleware does not output allowed header headers for unauthorized domains.

### 3. Rate Limiting Protection
* **Scenario:** Bots flood the GenAI chat gateway with concurrent requests.
* **Verification:** Covered by `RateLimiting_AiChatEndpoint_ExceedingLimitReturns429TooManyRequests` in `SecurityIntegrationTests.cs`. Asserts that the 11th request receives `429 Too Many Requests`.

### 4. JWT Stateless Logout Invalidation
* **Scenario:** Attacker steals a logged-out user's access token and attempts to access profile endpoints.
* **Verification:** Covered by `Logout_ShouldBlacklistJwtAccessToken` in `LogoutTokenInvalidationTests.cs`. Asserts that access is immediately rejected post-logout.

### 5. Malware Upload Scanning
* **Scenario:** User uploads a malicious binary renamed to `test.png` or `game.exe`.
* **Verification:** Covered by `MalwareScannerTests.cs` (verifying extension blacklists and checking magic headers like ELF/MZ signatures).
