# Pub/Sub Topics
resource "google_pubsub_topic" "events" {
  for_each = toset([
    "user-registered",
    "ai-request",
    "incident-created",
    "incident-resolved",
    "volunteer-assigned",
    "notification-sent",
    "crowd-updated",
    "transport-delayed",
  ])

  name = "stadium-ops-${each.value}-${var.environment}"

  message_retention_duration = "86400s" # 24 hours

  depends_on = [google_project_service.apis]
}

# Pub/Sub Subscriptions
resource "google_pubsub_subscription" "notification_push" {
  name  = "stadium-ops-notification-push-${var.environment}"
  topic = google_pubsub_topic.events["incident-created"].id

  ack_deadline_seconds = 20

  push_config {
    push_endpoint = "${google_cloud_run_v2_service.api.uri}/api/v1/webhooks/pubsub/incident"
  }

  retry_policy {
    minimum_backoff = "10s"
    maximum_backoff = "600s"
  }

  expiration_policy {
    ttl = ""
  }
}

resource "google_pubsub_subscription" "crowd_analytics" {
  name  = "stadium-ops-crowd-analytics-${var.environment}"
  topic = google_pubsub_topic.events["crowd-updated"].id

  bigquery_config {
    table          = "${var.project_id}.${google_bigquery_dataset.analytics.dataset_id}.crowd_events"
    write_metadata = true
  }
}

# BigQuery for analytics
resource "google_bigquery_dataset" "analytics" {
  dataset_id = "stadium_ops_analytics_${var.environment}"
  location   = var.region

  default_table_expiration_ms = 7776000000 # 90 days

  depends_on = [google_project_service.apis]
}
