# Operational Runbook

## Health Checks

```bash
# Cloud Run API
curl https://stadium-ops-api-xxxx.run.app/health
curl https://stadium-ops-api-xxxx.run.app/ready
curl https://stadium-ops-api-xxxx.run.app/live
```

## Incident Severity Levels

| Level | Description | Response Time | Escalation |
|-------|-------------|---------------|------------|
| **P1** | Critical outage — all users affected | 15 min | Immediate page to on-call |
| **P2** | Major functionality degraded | 30 min | Notify team lead |
| **P3** | Minor feature degradation | 4 hours | Next business day |
| **P4** | Cosmetic issue | 24 hours | Backlog |

## Common Issues

### Cloud Run — High Latency
1. Check Cloud Monitoring dashboard for request latency
2. Review Cloud Logging for slow database queries
3. Check Cloud SQL CPU utilization
4. Scale up Cloud Run instances if needed:
   ```bash
   gcloud run services update stadium-ops-api --min-instances=2
   ```

### Database — Connection Failures
1. Check Cloud SQL instance status
2. Verify connection string in Secret Manager
3. Check if IP allowlist is correct
4. Review connection pool settings

### Vertex AI — Timeouts
1. Check Vertex AI quota dashboard
2. Review prompt size (reduce if >8K tokens)
3. Switch to `gemini-2.0-flash` for lower latency
4. Implement circuit breaker fallback

## Backup & Recovery

### Database Backups
- **Automated**: Daily at 03:00 UTC (30-day retention)
- **Point-in-time recovery**: Enabled in production
- **Manual backup**: `gcloud sql backups create --instance=stadium-ops-db-production`

### Recovery Procedure
```bash
# Restore from backup
gcloud sql backups restore BACKUP_ID --restore-instance=stadium-ops-db-production

# Verify
gcloud sql instances describe stadium-ops-db-production
```

### Disaster Recovery
- **RTO**: 1 hour (Cloud Run auto-recovers, DB failover)
- **RPO**: 5 minutes (point-in-time recovery)
- **Multi-region**: Cloud SQL failover replica in us-east1

## Scaling

```bash
# Scale Cloud Run
gcloud run services update stadium-ops-api \
  --min-instances=5 \
  --max-instances=200

# Scale Cloud SQL
gcloud sql instances patch stadium-ops-db-production \
  --tier=db-custom-4-16384
```
