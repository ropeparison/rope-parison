# Baseline adoption preserves deployed settings. Runtime improvements are separate.
resource "azurerm_service_plan" "ropeparison" {
  name                = "ASP-RopeParison-99cf"
  resource_group_name = data.azurerm_resource_group.existing.name
  location            = "uksouth"
  os_type             = "Linux"
  sku_name            = "F1"
  worker_count        = 1

  lifecycle {
    prevent_destroy = true
  }
}

resource "azurerm_linux_web_app" "ropeparison" {
  name                                           = "ropeparison"
  resource_group_name                            = data.azurerm_resource_group.existing.name
  location                                       = "uksouth"
  service_plan_id                                = azurerm_service_plan.ropeparison.id
  https_only                                     = true
  enabled                                        = true
  client_affinity_enabled                        = false
  client_certificate_enabled                     = false
  client_certificate_mode                        = "Required"
  ftp_publish_basic_authentication_enabled       = true
  webdeploy_publish_basic_authentication_enabled = true
  public_network_access_enabled                  = true

  site_config {
    always_on                   = false
    app_command_line            = "dotnet RopeParison.dll"
    ftps_state                  = "FtpsOnly"
    http2_enabled               = false
    minimum_tls_version         = "1.2"
    scm_minimum_tls_version     = "1.2"
    scm_use_main_ip_restriction = false
    use_32_bit_worker           = true
    websockets_enabled          = false
    worker_count                = 1
    application_stack {
      dotnet_version = "7.0"
    }
  }

  lifecycle {
    prevent_destroy = true
    # Preserve existing secrets and authentication during baseline adoption.
    # Azure's SystemAssigned sentinel is not a user-assigned identity resource ID.
    ignore_changes = [
      app_settings,
      connection_string,
      key_vault_reference_identity_id,
      auth_settings,
      auth_settings_v2,
      tags,
      # Azure leaves these defaults unset; provider defaults would otherwise
      # propose an unnecessary network configuration update during import.
      site_config[0].ip_restriction_default_action,
      site_config[0].scm_ip_restriction_default_action,
    ]
  }
}

resource "azurerm_application_insights" "ropeparison" {
  name                                 = "ropeparison"
  resource_group_name                  = data.azurerm_resource_group.existing.name
  location                             = "uksouth"
  application_type                     = "web"
  workspace_id                         = data.azurerm_log_analytics_workspace.shared.id
  retention_in_days                    = 90
  sampling_percentage                  = 0
  daily_data_cap_in_gb                 = 100
  daily_data_cap_notifications_enabled = true
  force_customer_storage_for_profiler  = false
  internet_ingestion_enabled           = true
  internet_query_enabled               = true
  ip_masking_enabled                   = true
  local_authentication_enabled         = true

  lifecycle {
    prevent_destroy = true
  }
}

resource "azurerm_monitor_smart_detector_alert_rule" "ropeparison" {
  name                = "Failure Anomalies - ropeparison"
  resource_group_name = data.azurerm_resource_group.existing.name
  description         = "Failure Anomalies notifies you of an unusual rise in the rate of failed HTTP requests or dependency calls."
  detector_type       = "FailureAnomaliesDetector"
  enabled             = true
  frequency           = "PT1M"
  severity            = "Sev3"
  scope_resource_ids  = [azurerm_application_insights.ropeparison.id]
  action_group {
    ids = [data.azurerm_monitor_action_group.shared.id]
  }

  lifecycle {
    prevent_destroy = true
  }
}
