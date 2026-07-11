# Google Cloud Services — Detailed Usage

## Artificial Intelligence

### Vertex AI — Gemini 2.0 Flash
- **Stadium AI Assistant**: Natural language Q&A about venues, schedules, facilities
- **Multilingual Translation**: Real-time translation across 9 FIFA WC languages
- **Crowd Intelligence**: Density prediction and flow optimization
- **Sustainability Insights**: Energy/water/waste recommendations
- **Incident Classification**: Auto-categorize and prioritize incidents
- **Decision Support**: Real-time operational recommendations

### Vertex AI Embeddings
- Knowledge base indexing for grounded AI responses
- Semantic search across stadium documentation

---

## Authentication — Firebase Auth

| Method | Use Case |
|--------|----------|
| Google Sign-In | One-tap login for fans |
| Email/Password | Standard registration |
| Email Verification | Account validation |
| Password Reset | Self-service recovery |

**Roles**: RegisteredFan, Volunteer, StadiumOperator, IncidentResponder, Admin

---

## Hosting

### Firebase Hosting (Frontend)
- Global CDN edge network
- Automatic HTTPS/SSL
- SPA routing with redirects
- Zero-config deployment

### Cloud Run (Backend)
- Auto-scaling 0→100 instances
- Serverless container execution
- 99.95% SLA
- $0 when idle

---

## Database

### Cloud SQL — PostgreSQL 15 (Primary)
- Matches, stadiums, users, incidents, volunteer tasks
- Automated backups (daily)
- High availability with failover
- 99.95% SLA

### BigQuery (Analytics)
- Crowd density patterns over time
- Fan engagement metrics
- Transportation usage analytics
- Sustainability trend reporting

### Firestore (Real-time)
- Live chat messages
- Real-time crowd updates
- Notification delivery status

---

## Storage — Cloud Storage
- Stadium maps and floor plans
- AI-generated reports
- Incident photographs
- Sustainability data exports
- Multi-regional redundancy

---

## Notifications — Firebase Cloud Messaging
- Match start/end alerts
- Emergency notifications (P1 incidents)
- Volunteer task assignments
- Transportation delay alerts
- Gate opening announcements

---

## Maps — Google Maps Platform
- **Maps SDK**: Interactive stadium maps
- **Directions API**: Transit/driving/walking routes to venue
- **Places API**: Nearby restaurants, hotels, transport
- **Geocoding API**: Address resolution

---

## Event Streaming — Pub/Sub

| Topic | Events |
|-------|--------|
| `user.registered` | New fan/volunteer registration |
| `ai.request` | AI assistant query |
| `incident.created` | New incident reported |
| `incident.resolved` | Incident marked resolved |
| `volunteer.assigned` | Task assignment |
| `notification.sent` | Push notification dispatched |
| `crowd.updated` | Zone density change |
| `transport.delayed` | Transit delay detected |

---

## Observability

### Cloud Logging
- API request/response logs
- AI prompt/completion logs
- Authentication events
- Error tracking with stack traces

### Cloud Monitoring
- API latency (p50, p95, p99)
- AI response time
- Database query performance
- Active user count
- Error rate dashboards
- SLO tracking

---

## Security — Secret Manager

| Secret | Purpose |
|--------|---------|
| `jwt-signing-key` | API token signing |
| `db-connection-string` | Cloud SQL credentials |
| `gemini-api-key` | Vertex AI access |
| `firebase-admin-key` | Firebase Admin SDK |
| `maps-api-key` | Google Maps Platform |
| `fcm-server-key` | Push notification sending |
