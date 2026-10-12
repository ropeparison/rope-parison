# Only previous versions are expired. Current state and snapshots are untouched.
# Reserve these prefixes for the two backend keys; Azure filters use prefixes,
# not exact filename matching. Age is measured from version creation.
resource "azurerm_storage_management_policy" "state_versions" {
  storage_account_id = azurerm_storage_account.state.id

  rule {
    name    = "expire-previous-state-versions"
    enabled = true
    filters {
      prefix_match = ["tfstate/production.tfstate", "tfstate/bootstrap.tfstate", "tfproduction/production.tfstate"]
      blob_types   = ["blockBlob"]
    }
    actions {
      version {
        delete_after_days_since_creation = 90
      }
    }
  }
  rule {
    name    = "expire-private-plan-bundles"
    enabled = true
    filters {
      prefix_match = ["tfplans/runs/"]
      blob_types   = ["blockBlob"]
    }
    actions {
      base_blob {
        delete_after_days_since_modification_greater_than = 7
      }
      version {
        delete_after_days_since_creation = 7
      }
    }
  }
}
