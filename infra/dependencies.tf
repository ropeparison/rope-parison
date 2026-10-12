# Existing group and monitoring services remain shared references.
data "azurerm_resource_group" "existing" {
  name = "RopeParison"
}

data "azurerm_log_analytics_workspace" "shared" {
  name                = "DefaultWorkspace-ad052b7b-2015-4933-93eb-0b857cabccae-SUK"
  resource_group_name = "DefaultResourceGroup-SUK"
}

data "azurerm_monitor_action_group" "shared" {
  name                = "Application Insights Smart Detection"
  resource_group_name = data.azurerm_resource_group.existing.name
}
