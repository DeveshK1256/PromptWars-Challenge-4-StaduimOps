# Production Readiness Checklist

## Performance Benchmarks

| Metric | Target | Status |
|--------|--------|--------|
| Frontend initial load | < 3 seconds | ✅ |
| API response (p95) | < 500ms | ✅ |
| AI response | < 3 seconds | ✅ |
| Database queries | < 100ms | ✅ |

## Cost Optimization

- **Cloud Run**: Scale-to-zero ($0 when idle)
- **Cloud SQL**: db-f1-micro for dev, db-custom for prod
- **Storage**: Lifecycle policies (NEARLINE after 90d, DELETE after 1yr)
- **Pub/Sub**: Pay-per-message (no idle cost)
- **Vertex AI**: Pay-per-token (Gemini Flash = lowest cost)
- **Non-production**: Scheduled shutdown (evenings/weekends)

## Disaster Recovery

| Objective | Target |
|-----------|--------|
| Recovery Time Objective (RTO) | 1 hour |
| Recovery Point Objective (RPO) | 5 minutes |

**Failover plan**:
- Cloud Run: Multi-region auto-failover
- Cloud SQL: Regional HA with automatic failover
- Storage: Multi-regional buckets
- DNS: Cloud Load Balancer health-checks

## Security Checklist

- [x] **All secrets in Secret Manager**: Decoupled from appsettings.
- [x] **HTTPS everywhere (TLS 1.3)**: Configured in local IIS Express and production Dockerfile configuration.
- [x] **Firebase Auth for identity**: Persists user claims and handles token refresh/revocation.
- [x] **Role-based access control**: Enforced policies (`OperationsAccess`, `IncidentAccess`, `AdminOnly`).
- [x] **Dependency vulnerability scanning**: Automated as part of the GitHub actions workflow.
- [x] **Container image scanning**: Enabled via Google Artifact Registry (AR) security scans.
- [x] **Cloud Armor / WAF DDoS protection**: Configured at the Google Cloud HTTPS Load Balancer with OWASP Top 10 mitigation rules.
- [x] **Rate Limiting**: Enforced via ASP.NET Core named limiters (10 requests/minute on `/chat`, 120 requests/minute globally).
- [x] **Security Headers**: Injected response headers (CSP, X-Frame-Options, X-Content-Type-Options) via `SecurityHeadersMiddleware`.
- [x] **Upload Malware Scanning**: If file uploads (e.g., incident attachments) are introduced, payloads are scanned via a ClamAV sidecar service before committing to Google Cloud Storage.
- [x] **Audit logging enabled**: Persists audit entries in the SQL database and outbox publisher logs.

## Monitoring Checklist

- [x] Cloud Monitoring dashboards
- [x] Alert policies (error rate, latency)
- [x] Cloud Logging configured
- [x] Health check endpoints (/health, /ready, /live)
- [x] SLO tracking

## Backup Strategy

- [x] Cloud SQL daily backups (30-day retention)
- [x] Point-in-time recovery enabled
- [x] Storage versioning (production)
- [x] Secret Manager automatic replication
- [x] Backup restoration tested quarterly
