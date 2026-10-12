import {
  to = azurerm_service_plan.ropeparison
  id = "/subscriptions/ad052b7b-2015-4933-93eb-0b857cabccae/resourceGroups/RopeParison/providers/Microsoft.Web/serverFarms/ASP-RopeParison-99cf"
}

import {
  to = azurerm_linux_web_app.ropeparison
  id = "/subscriptions/ad052b7b-2015-4933-93eb-0b857cabccae/resourceGroups/RopeParison/providers/Microsoft.Web/sites/ropeparison"
}

import {
  to = azurerm_application_insights.ropeparison
  id = "/subscriptions/ad052b7b-2015-4933-93eb-0b857cabccae/resourceGroups/RopeParison/providers/Microsoft.Insights/components/ropeparison"
}

import {
  to = azurerm_monitor_smart_detector_alert_rule.ropeparison
  id = "/subscriptions/ad052b7b-2015-4933-93eb-0b857cabccae/resourceGroups/RopeParison/providers/Microsoft.AlertsManagement/smartDetectorAlertRules/Failure Anomalies - ropeparison"
}

import {
  to = azurerm_mssql_server.shared
  id = "/subscriptions/ad052b7b-2015-4933-93eb-0b857cabccae/resourceGroups/RopeParison/providers/Microsoft.Sql/servers/ropeparison"
}

import {
  to = azurerm_mssql_database.shared
  id = "/subscriptions/ad052b7b-2015-4933-93eb-0b857cabccae/resourceGroups/RopeParison/providers/Microsoft.Sql/servers/ropeparison/databases/RopeParisonData"
}

import {
  to = azurerm_mssql_firewall_rule.azure_services
  id = "/subscriptions/ad052b7b-2015-4933-93eb-0b857cabccae/resourceGroups/RopeParison/providers/Microsoft.Sql/servers/ropeparison/firewallRules/AllowAllWindowsAzureIps"
}

import {
  to = azurerm_mssql_firewall_rule.legacy_client
  id = "/subscriptions/ad052b7b-2015-4933-93eb-0b857cabccae/resourceGroups/RopeParison/providers/Microsoft.Sql/servers/ropeparison/firewallRules/ClientIPAddress_2024-1-28_21-37-20"
}

import {
  to = azurerm_mssql_firewall_rule.query_editor
  id = "/subscriptions/ad052b7b-2015-4933-93eb-0b857cabccae/resourceGroups/RopeParison/providers/Microsoft.Sql/servers/ropeparison/firewallRules/QueryEditorClientIPAddress_1789646070751"
}
