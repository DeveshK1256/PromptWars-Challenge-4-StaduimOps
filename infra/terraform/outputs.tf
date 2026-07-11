output "api_url" {
  description = "Cloud Run API URL"
  value       = google_cloud_run_v2_service.api.uri
}

output "db_connection_name" {
  description = "Cloud SQL connection name"
  value       = google_sql_database_instance.main.connection_name
}

output "assets_bucket" {
  description = "Cloud Storage assets bucket"
  value       = google_storage_bucket.assets.name
}

output "pubsub_topics" {
  description = "Pub/Sub topic names"
  value       = { for k, v in google_pubsub_topic.events : k => v.name }
}
