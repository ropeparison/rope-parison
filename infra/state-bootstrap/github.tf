# Bootstrap is owner-operated. CI cannot change its own roles or federation.
locals {
  subscription      = "/subscriptions/ad052b7b-2015-4933-93eb-0b857cabccae"
  application_group = "${local.subscription}/resourceGroups/RopeParison"
  workspace         = "${local.subscription}/resourceGroups/DefaultResourceGroup-SUK/providers/Microsoft.OperationalInsights/workspaces/DefaultWorkspace-ad052b7b-2015-4933-93eb-0b857cabccae-SUK"
  web_app           = "${local.application_group}/providers/Microsoft.Web/sites/ropeparison"
  identities = {
    plan   = "Terraform-Plan"
    apply  = "Terraform-Apply"
    deploy = "Production"
  }
  writable_resources = {
    hosting  = "${local.application_group}/providers/Microsoft.Web/serverFarms/ASP-RopeParison-99cf"
    app      = local.web_app
    insights = "${local.application_group}/providers/Microsoft.Insights/components/ropeparison"
    alert    = "${local.application_group}/providers/Microsoft.AlertsManagement/smartDetectorAlertRules/Failure Anomalies - ropeparison"
    sql      = "${local.application_group}/providers/Microsoft.Sql/servers/ropeparison"
  }
  read_scopes = { group = local.application_group, workspace = local.workspace }
  readers = merge(
    { for name, scope in local.read_scopes : "plan-${name}" => { identity = "plan", scope = scope } },
    { for name, scope in local.read_scopes : "apply-${name}" => { identity = "apply", scope = scope } }
  )
}

# CI state is isolated from bootstrap state, which contains the access grants.
resource "azurerm_storage_container" "production" {
  name                  = "tfproduction"
  storage_account_id    = azurerm_storage_account.state.id
  container_access_type = "private"
  lifecycle { prevent_destroy = true }
}

resource "azurerm_storage_container" "plans" {
  name                  = "tfplans"
  storage_account_id    = azurerm_storage_account.state.id
  container_access_type = "private"
  lifecycle { prevent_destroy = true }
}

resource "azurerm_user_assigned_identity" "github" {
  for_each            = local.identities
  name                = "ropeparison-github-${each.key}"
  resource_group_name = azurerm_resource_group.state.name
  location            = azurerm_resource_group.state.location
  lifecycle { prevent_destroy = true }
}

resource "azurerm_federated_identity_credential" "github" {
  for_each                  = local.identities
  name                      = "github-environment"
  user_assigned_identity_id = azurerm_user_assigned_identity.github[each.key].id
  issuer                    = "https://token.actions.githubusercontent.com"
  audience                  = ["api://AzureADTokenExchange"]
  subject                   = "repo:ropeparison/rope-parison:environment:${each.value}"
}

resource "azurerm_role_assignment" "read" {
  for_each             = local.readers
  scope                = each.value.scope
  role_definition_name = "Reader"
  principal_id         = azurerm_user_assigned_identity.github[each.value.identity].principal_id
  principal_type       = "ServicePrincipal"
}

# Reader excludes the POST operation the provider uses to refresh app settings.
resource "azurerm_role_definition" "plan_app_configuration" {
  name        = "RopeParison Terraform configuration reader"
  scope       = local.application_group
  description = "Read the existing RopeParison app configuration during Terraform refresh."
  permissions { actions = ["Microsoft.Web/sites/config/list/action"] }
  assignable_scopes = [local.application_group]
}

resource "azurerm_role_assignment" "plan_app_configuration" {
  scope              = local.web_app
  role_definition_id = azurerm_role_definition.plan_app_configuration.role_definition_resource_id
  principal_id       = azurerm_user_assigned_identity.github["plan"].principal_id
  principal_type     = "ServicePrincipal"
}

resource "azurerm_role_assignment" "production_state" {
  for_each             = toset(["plan", "apply"])
  scope                = "${azurerm_storage_account.state.id}/blobServices/default/containers/${azurerm_storage_container.production.name}"
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_user_assigned_identity.github[each.key].principal_id
  principal_type       = "ServicePrincipal"
}

resource "azurerm_role_assignment" "plans" {
  for_each             = { plan = "Storage Blob Data Contributor", apply = "Storage Blob Data Reader" }
  scope                = "${azurerm_storage_account.state.id}/blobServices/default/containers/${azurerm_storage_container.plans.name}"
  role_definition_name = each.value
  principal_id         = azurerm_user_assigned_identity.github[each.key].principal_id
  principal_type       = "ServicePrincipal"
}

# SQL scope includes its database/firewalls and is shared with SimpleBugTracker.
# Contributor cannot create role assignments; no group/subscription write grant.
resource "azurerm_role_assignment" "apply" {
  for_each             = local.writable_resources
  scope                = each.value
  role_definition_name = "Contributor"
  principal_id         = azurerm_user_assigned_identity.github["apply"].principal_id
  principal_type       = "ServicePrincipal"
}

resource "azurerm_role_assignment" "deploy" {
  scope                = local.web_app
  role_definition_name = "Website Contributor"
  principal_id         = azurerm_user_assigned_identity.github["deploy"].principal_id
  principal_type       = "ServicePrincipal"
}

output "github_client_ids" {
  value = { for key, identity in azurerm_user_assigned_identity.github : key => identity.client_id }
}
