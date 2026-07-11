# Secret Manager
resource "google_secret_manager_secret" "db_connection" {
  secret_id = "stadium-ops-db-connection-${var.environment}"

  replication {
    auto {}
  }

  depends_on = [google_project_service.apis]
}

resource "google_secret_manager_secret" "gemini_key" {
  secret_id = "stadium-ops-gemini-key-${var.environment}"

  replication {
    auto {}
  }
}

resource "google_secret_manager_secret" "jwt_key" {
  secret_id = "stadium-ops-jwt-key-${var.environment}"

  replication {
    auto {}
  }
}

resource "google_secret_manager_secret" "maps_key" {
  secret_id = "stadium-ops-maps-key-${var.environment}"

  replication {
    auto {}
  }
}

resource "google_secret_manager_secret" "firebase_admin" {
  secret_id = "stadium-ops-firebase-admin-${var.environment}"

  replication {
    auto {}
  }
}

# Store DB connection string
resource "google_secret_manager_secret_version" "db_connection" {
  secret      = google_secret_manager_secret.db_connection.id
  secret_data = "Host=/cloudsql/${google_sql_database_instance.main.connection_name};Database=${google_sql_database.app.name};Username=${google_sql_user.app.name};Password=${random_password.db_password.result}"
}
