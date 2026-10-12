terraform {
  required_version = "~> 1.16.0"
  backend "azurerm" {}
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
  }
}

provider "azurerm" {
  features {}
  subscription_id                 = "ad052b7b-2015-4933-93eb-0b857cabccae"
  tenant_id                       = "a2155124-112f-4b17-a715-0ca032e18b28"
  resource_provider_registrations = "none"
  storage_use_azuread             = true
}

resource "azurerm_resource_group" "state" {
  name     = "RopeParisonTerraformState"
  location = "uksouth"
  lifecycle {
    prevent_destroy = true
  }
}

resource "azurerm_storage_account" "state" {
  name                              = "strpstatead052b7b2015"
  resource_group_name               = azurerm_resource_group.state.name
  location                          = azurerm_resource_group.state.location
  account_kind                      = "StorageV2"
  account_tier                      = "Standard"
  account_replication_type          = "LRS"
  access_tier                       = "Hot"
  min_tls_version                   = "TLS1_2"
  https_traffic_only_enabled        = true
  allow_nested_items_to_be_public   = false
  shared_access_key_enabled         = false
  default_to_oauth_authentication   = true
  public_network_access_enabled     = true
  local_user_enabled                = false
  infrastructure_encryption_enabled = true

  blob_properties {
    versioning_enabled = true
    delete_retention_policy {
      days = 30
    }
    container_delete_retention_policy {
      days = 30
    }
  }

  lifecycle {
    prevent_destroy = true
  }
}

resource "azurerm_storage_container" "state" {
  name                  = "tfstate"
  storage_account_id    = azurerm_storage_account.state.id
  container_access_type = "private"
  lifecycle {
    prevent_destroy = true
  }
}

# This is the signed-in owner's object ID, not a new app/service principal.
resource "azurerm_role_assignment" "owner_state_data" {
  scope                = azurerm_storage_account.state.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = "0e8df5df-f9fe-44f2-863a-e8cd1eb954fc"
  principal_type       = "User"
}

output "backend_storage_account" {
  value = azurerm_storage_account.state.name
}

output "backend_container" {
  value = azurerm_storage_container.state.name
}
