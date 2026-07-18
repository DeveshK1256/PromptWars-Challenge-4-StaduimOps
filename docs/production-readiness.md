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

- [x] All secrets in Secret Manager
- [x] HTTPS everywhere (TLS 1.3)
- [x] Firebase Auth for identity
- [x] Role-based access control
- [x] Dependency vulnerability scanning
- [x] Container image scanning
- [x] Cloud Armor DDoS protection
- [x] Audit logging enabled

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
