# Shared foundation: RopeParison and SimpleBugTracker use these existing resources.
# Other apps must reference them rather than managing duplicate declarations.
variable "sql_existing_administrator" {
  description = "Current Entra SQL administrator, captured into ignored private inputs."
  type = object({
    login_username = string
    object_id      = string
    tenant_id      = string
  })
}

variable "sql_client_firewall_rules" {
  description = "Existing client IP ranges; preserve them through private inventory inputs."
  type = object({
    legacy_client = object({
      start_ip_address = string
      end_ip_address   = string
    })
    query_editor = object({
      start_ip_address = string
      end_ip_address   = string
    })
  })
}

resource "azurerm_mssql_server" "shared" {
  name                          = "ropeparison"
  resource_group_name           = data.azurerm_resource_group.existing.name
  location                      = "uksouth"
  version                       = "12.0"
  administrator_login           = "ropeparisonadmin"
  minimum_tls_version           = "1.2"
  public_network_access_enabled = true

  azuread_administrator {
    login_username              = var.sql_existing_administrator.login_username
    object_id                   = var.sql_existing_administrator.object_id
    tenant_id                   = var.sql_existing_administrator.tenant_id
    azuread_authentication_only = false
  }

  lifecycle {
    prevent_destroy = true
    # Existing password is intentionally unknown and is not rotated by adoption.
    ignore_changes = [administrator_login_password, administrator_login_password_wo_version]
  }
}

resource "azurerm_mssql_database" "shared" {
  name                                = "RopeParisonData"
  server_id                           = azurerm_mssql_server.shared.id
  collation                           = "SQL_Latin1_General_CP1_CI_AS"
  max_size_gb                         = 2
  sku_name                            = "Basic"
  storage_account_type                = "Zone"
  maintenance_configuration_name      = "SQL_Default"
  read_scale                          = false
  zone_redundant                      = false
  transparent_data_encryption_enabled = true

  short_term_retention_policy {
    retention_days           = 7
    backup_interval_in_hours = 24
  }

  long_term_retention_policy {
    weekly_retention          = "PT0S"
    monthly_retention         = "PT0S"
    yearly_retention          = "PT0S"
    immutable_backups_enabled = false
  }

  lifecycle {
    prevent_destroy = true
  }
}

resource "azurerm_mssql_firewall_rule" "azure_services" {
  name             = "AllowAllWindowsAzureIps"
  server_id        = azurerm_mssql_server.shared.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
  lifecycle {
    prevent_destroy = true
  }
}

resource "azurerm_mssql_firewall_rule" "legacy_client" {
  name             = "ClientIPAddress_2024-1-28_21-37-20"
  server_id        = azurerm_mssql_server.shared.id
  start_ip_address = var.sql_client_firewall_rules.legacy_client.start_ip_address
  end_ip_address   = var.sql_client_firewall_rules.legacy_client.end_ip_address
  lifecycle {
    prevent_destroy = true
  }
}

resource "azurerm_mssql_firewall_rule" "query_editor" {
  name             = "QueryEditorClientIPAddress_1789646070751"
  server_id        = azurerm_mssql_server.shared.id
  start_ip_address = var.sql_client_firewall_rules.query_editor.start_ip_address
  end_ip_address   = var.sql_client_firewall_rules.query_editor.end_ip_address
  lifecycle {
    prevent_destroy = true
  }
}
