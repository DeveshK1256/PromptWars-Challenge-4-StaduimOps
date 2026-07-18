# Smart Stadium Operations — Google Cloud Architecture

## System Architecture

```mermaid
graph TB
    Internet((Internet)) --> CLB[Cloud Load Balancer]
    CLB --> FH[Firebase Hosting<br/>React Frontend]
    CLB --> CDN[Cloud CDN<br/>Static Assets]
    
    FH --> CR[Cloud Run<br/>ASP.NET Core API]
    
    CR --> Auth[Firebase Auth<br/>Google Sign-In + Email]
    CR --> AI[Vertex AI<br/>Gemini 2.0 Flash]
    CR --> Nav[Google Maps Platform<br/>Routes + Places API]
    CR --> DB[(Cloud SQL<br/>PostgreSQL 15)]
    CR --> PS[Pub/Sub<br/>Event Streaming]
    CR --> CS[Cloud Storage<br/>Assets + Reports]
    
    PS --> FCM[Firebase Cloud Messaging<br/>Push Notifications]
    PS --> BQ[BigQuery<br/>Analytics]
    
    CR --> SM[Secret Manager]
    CR --> CL[Cloud Logging]
    CR --> CM[Cloud Monitoring]
    
    style CR fill:#0f766e,color:#fff
    style AI fill:#b7791f,color:#fff
    style DB fill:#1d4ed8,color:#fff
    style FH fill:#f59e0b,color:#000
```

## Data Flow

```mermaid
sequenceDiagram
    participant Fan as Fan (Browser)
    participant FH as Firebase Hosting
    participant CR as Cloud Run API
    participant VA as Vertex AI Gemini
    participant DB as Cloud SQL
    participant PS as Pub/Sub
    participant FCM as Firebase FCM

    Fan->>FH: Load React App
    FH-->>Fan: SPA + Static Assets
    Fan->>CR: POST /api/v1/auth/login
    CR->>DB: Validate credentials
    CR-->>Fan: JWT Token
    
    Fan->>CR: POST /api/v1/ai/chat
    CR->>VA: Gemini prompt + context
    VA-->>CR: AI response
    CR->>DB: Log conversation
    CR-->>Fan: Assistant reply
    
    Fan->>CR: POST /api/v1/incidents
    CR->>DB: Create incident
    CR->>PS: Publish incident.created
    PS->>FCM: Push notification
    FCM-->>Fan: Alert delivered
```

## Service Responsibilities

| Service | Role | Scaling |
|---------|------|---------|
| **Cloud Run** | API Gateway, business logic | Auto-scale 0→100 instances |
| **Vertex AI** | AI assistant, translation, crowd intelligence | Managed, pay-per-request |
| **Cloud SQL** | Persistent data (users, matches, incidents) | Vertical + read replicas |
| **Pub/Sub** | Event streaming, async processing | Unlimited throughput |
| **Firebase Auth** | Identity, Google Sign-In, email/password | Managed, unlimited users |
| **Firebase Hosting** | Frontend CDN, global edge | Auto-scale, global CDN |
| **Cloud Storage** | Stadium maps, reports, AI artifacts | Unlimited storage |
| **Firebase FCM** | Push notifications to fans | Managed, millions/sec |
| **Google Maps** | Navigation, directions, places | Pay-per-request |
| **BigQuery** | Analytics, crowd patterns | Serverless analytics |
| **Secret Manager** | API keys, JWT secrets, DB passwords | Managed |
| **Cloud Monitoring** | Dashboards, alerting, SLOs | Managed |

## Security Architecture

- **Authentication**: Firebase Auth (Google Sign-In + Email/Password)
- **Authorization**: Role-based (RegisteredFan, Volunteer, StadiumOperator, Admin)
- **API Security**: JWT validation on every Cloud Run request
- **Secrets**: All credentials in Secret Manager (never in code)
- **Network**: HTTPS everywhere, Cloud Armor DDoS protection
- **Data**: Encryption at rest (Cloud SQL, Storage) + in transit (TLS 1.3)
