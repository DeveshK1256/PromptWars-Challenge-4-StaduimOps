# API Documentation

## Base URL

- **Production**: `https://stadium-ops-api-xxxx.run.app/api/v1`
- **Demo Mode**: Set `VITE_API_BASE_URL=demo` (uses client-side demo data)

## Authentication

All API requests require a Bearer token from Firebase Auth:

```http
Authorization: Bearer <firebase-id-token>
```

### POST /auth/register
Register a new user.

```json
{
  "name": "John Smith",
  "email": "john@example.com",
  "password": "securepassword123",
  "preferredLanguage": "en",
  "accessibilityPreference": "",
  "requestedRole": "RegisteredFan"
}
```

### POST /auth/login
```json
{ "email": "john@example.com", "password": "securepassword123", "deviceName": "web-browser" }
```

**Response:**
```json
{
  "accessToken": "eyJhbG...",
  "refreshToken": "rf_...",
  "accessTokenExpiresAt": "2026-07-12T00:00:00Z",
  "user": {
    "id": "user-uuid",
    "name": "John Smith",
    "email": "john@example.com",
    "roles": ["RegisteredFan"],
    "preferredLanguage": "en"
  }
}
```

---

## Stadiums

### GET /stadiums
Returns all tournament venues.

### GET /stadiums/{id}/points-of-interest
Returns POIs (food, restrooms, gates) for a stadium.

### GET /stadiums/{id}/crowd-zones
Returns real-time crowd density zones.

---

## Matches

### GET /matches/today
Returns today's match with teams, venue, kickoff time.

---

## Navigation

### POST /navigation/route
```json
{
  "stadiumId": "stadium-uuid",
  "fromLocation": "Metro Shuttle Drop",
  "toLocation": "Gate C",
  "accessibilityRequired": true
}
```

---

## Incidents

### GET /incidents
Returns incident triage queue (role-gated).

### POST /incidents
Report a new incident.
```json
{
  "category": "Medical",
  "severity": "High",
  "location": "Section 204",
  "description": "Fan needs medical attention"
}
```

### PUT /incidents/{id}/status
Update incident status (Volunteer role required).

---

## AI Assistant

### POST /ai/chat
```json
{ "message": "What food options are near Gate A?", "stadiumId": "stadium-uuid" }
```

### POST /ai/translate
```json
{ "text": "Where is the nearest restroom?", "fromLanguage": "en", "toLanguage": "es" }
```

---

## Transport

### GET /transport/routes
Returns transit options to the stadium.

### GET /transport/parking
Returns parking availability.

---

## Sustainability

### GET /sustainability/{stadiumId}/metrics
Returns energy, water, waste, recycling metrics.

---

## Health Checks

| Endpoint | Purpose |
|----------|---------|
| `GET /health` | Overall health status |
| `GET /ready` | Readiness probe (DB + AI connected) |
| `GET /live` | Liveness probe |
