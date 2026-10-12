param([string]$PlanJson = (Join-Path $PSScriptRoot 'private/adoption-plan.json'))
$ErrorActionPreference = 'Stop'
$plan = Get-Content -LiteralPath $PlanJson -Raw | ConvertFrom-Json
if ($plan.errored -or $plan.complete -eq $false) { throw 'Refusing an errored or incomplete plan.' }
$base = '/subscriptions/ad052b7b-2015-4933-93eb-0b857cabccae/resourceGroups/RopeParison/providers'
$expected = @{
    'azurerm_service_plan.ropeparison' = "$base/Microsoft.Web/serverFarms/ASP-RopeParison-99cf"
    'azurerm_linux_web_app.ropeparison' = "$base/Microsoft.Web/sites/ropeparison"
    'azurerm_application_insights.ropeparison' = "$base/Microsoft.Insights/components/ropeparison"
    'azurerm_monitor_smart_detector_alert_rule.ropeparison' = "$base/Microsoft.AlertsManagement/smartDetectorAlertRules/Failure Anomalies - ropeparison"
    'azurerm_mssql_server.shared' = "$base/Microsoft.Sql/servers/ropeparison"
    'azurerm_mssql_database.shared' = "$base/Microsoft.Sql/servers/ropeparison/databases/RopeParisonData"
    'azurerm_mssql_firewall_rule.azure_services' = "$base/Microsoft.Sql/servers/ropeparison/firewallRules/AllowAllWindowsAzureIps"
    'azurerm_mssql_firewall_rule.legacy_client' = "$base/Microsoft.Sql/servers/ropeparison/firewallRules/ClientIPAddress_2024-1-28_21-37-20"
    'azurerm_mssql_firewall_rule.query_editor' = "$base/Microsoft.Sql/servers/ropeparison/firewallRules/QueryEditorClientIPAddress_1789646070751"
}
$changes = @($plan.resource_changes | Where-Object { $_.mode -eq 'managed' })
if ($changes.Count -ne $expected.Count) { throw 'Unexpected number of managed resources in adoption plan.' }
foreach ($change in $changes) {
    if (!$expected.ContainsKey($change.address)) { throw "Unexpected managed resource: $($change.address)" }
    if (($change.change.actions -join ',') -ne 'no-op') { throw "Infrastructure change proposed: $($change.address)" }
    if ($change.change.importing.id -ine $expected[$change.address]) { throw "Missing or mismatched import: $($change.address)" }
}
'Verified: exactly nine expected imports, with no infrastructure changes.'
