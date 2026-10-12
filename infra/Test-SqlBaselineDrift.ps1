param(
    [string]$InventoryDirectory = (Join-Path $PSScriptRoot 'private/inventory'),
    [string]$InputPath = (Join-Path $PSScriptRoot 'private/sql-baseline.tfvars.json')
)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'SqlBaselineInputs.ps1')
$desired = Get-Content -LiteralPath $InputPath -Raw | ConvertFrom-Json
$server = Get-Content -LiteralPath (Join-Path $InventoryDirectory 'sql-server.json') -Raw | ConvertFrom-Json
$rules = @(Get-Content -LiteralPath (Join-Path $InventoryDirectory 'sql-firewalls.json') -Raw | ConvertFrom-Json)
$observed = ConvertTo-SqlBaselineInputs -Server $server -Rules $rules
$differences = @()
foreach ($field in @('login_username','object_id','tenant_id')) {
    if ($desired.sql_existing_administrator.$field -cne $observed.sql_existing_administrator[$field]) {
        $differences += "sql_existing_administrator.$field"
    }
}
foreach ($rule in @('legacy_client','query_editor')) {
    foreach ($field in @('start_ip_address','end_ip_address')) {
        if ($desired.sql_client_firewall_rules.$rule.$field -cne $observed.sql_client_firewall_rules[$rule][$field]) {
            $differences += "sql_client_firewall_rules.$rule.$field"
        }
    }
}
if ($differences.Count) { throw "SQL drift detected in: $($differences -join ', '). Desired inputs were not changed." }
'Observed SQL administrator and firewall inputs match the reviewed desired inputs.'
