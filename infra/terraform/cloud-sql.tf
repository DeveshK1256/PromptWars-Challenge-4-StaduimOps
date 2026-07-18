# Cloud SQL — PostgreSQL 15
resource "google_sql_database_instance" "main" {
  name             = "stadium-ops-db-${var.environment}"
  database_version = "POSTGRES_15"
  region           = var.region

  settings {
    tier              = var.db_tier
    availability_type = var.environment == "production" ? "REGIONAL" : "ZONAL"

    backup_configuration {
      enabled                        = true
      point_in_time_recovery_enabled = var.environment == "production"
      start_time                     = "03:00"
      transaction_log_retention_days = 7

      backup_retention_settings {
        retained_backups = 30
      }
    }

    ip_configuration {
      ipv4_enabled = true
      require_ssl  = true
    }

    database_flags {
      name  = "log_min_duration_statement"
      value = "1000"
    }

    insights_config {
      query_insights_enabled  = true
      record_application_tags = true
    }
  }

  deletion_protection = var.environment == "production"
  depends_on          = [google_project_service.apis]
}

resource "google_sql_database" "app" {
  name     = "stadium_ops"
  instance = google_sql_database_instance.main.name
}

resource "google_sql_user" "app" {
  name     = "stadium_ops_app"
  instance = google_sql_database_instance.main.name
  password = random_password.db_password.result
}

resource "random_password" "db_password" {
  length  = 32
  special = true
}
