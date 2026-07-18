# Deployment Guide

## Prerequisites

1. **Google Cloud Project** with billing enabled
2. **Firebase Project** linked to the GCP project
3. **Terraform** v1.5+ installed
4. **gcloud CLI** authenticated
5. **Node.js** 20+ and **npm** 10+
6. **.NET SDK** 9.0+

## Quick Start

### 1. Clone and Install

```bash
git clone https://github.com/DeveshK1256/PromptWars-Challenge-4-StaduimOps.git
cd PromptWars-Challenge-4-StaduimOps
npm ci
```

### 2. Enable Google Cloud APIs

```bash
gcloud services enable \
  run.googleapis.com \
  sqladmin.googleapis.com \
  aiplatform.googleapis.com \
  pubsub.googleapis.com \
  secretmanager.googleapis.com \
  firebasehosting.googleapis.com
```

### 3. Deploy Infrastructure (Terraform)

```bash
cd infra/terraform
terraform init
terraform plan -var="project_id=YOUR_PROJECT_ID"
terraform apply -var="project_id=YOUR_PROJECT_ID"
```

### 4. Configure Secrets

```bash
# Store secrets in Secret Manager
echo -n "YOUR_GEMINI_KEY" | gcloud secrets versions add stadium-ops-gemini-key-production --data-file=-
echo -n "YOUR_JWT_KEY" | gcloud secrets versions add stadium-ops-jwt-key-production --data-file=-
```

### 5. Deploy Backend to Cloud Run

```bash
# Build and push container
gcloud builds submit --tag gcr.io/YOUR_PROJECT_ID/stadium-ops-api

# Deploy
gcloud run deploy stadium-ops-api \
  --image gcr.io/YOUR_PROJECT_ID/stadium-ops-api \
  --region us-central1 \
  --allow-unauthenticated
```

### 6. Deploy Frontend

```bash
# Set environment variables
export VITE_API_BASE_URL=https://stadium-ops-api-xxxxx.run.app
export VITE_FIREBASE_API_KEY=your-key
export VITE_FIREBASE_PROJECT_ID=your-project

# Build and deploy
npm run build
firebase deploy --only hosting
```

## Environment Variables

| Variable | Description | Required |
|----------|-------------|----------|
| `VITE_API_BASE_URL` | Cloud Run API URL (or `demo` for demo mode) | Yes |
| `VITE_FIREBASE_API_KEY` | Firebase API key | For real auth |
| `VITE_FIREBASE_AUTH_DOMAIN` | Firebase auth domain | For real auth |
| `VITE_FIREBASE_PROJECT_ID` | Firebase project ID | For real auth |
| `VITE_FIREBASE_APP_ID` | Firebase app ID | For real auth |
| `VITE_GOOGLE_MAPS_KEY` | Google Maps API key | For live maps |

## Environments

| Environment | Purpose | URL |
|-------------|---------|-----|
| Development | Local dev server | `http://localhost:5173` |
| Staging | Pre-production testing | `https://staging.stadium-ops.web.app` |
| Production | Live deployment | `https://stadium-ops.web.app` |
