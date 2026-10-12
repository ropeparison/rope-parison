param(
    [string]$InventoryDirectory = (Join-Path $PSScriptRoot 'private/inventory'),
    [string]$OutputPath = (Join-Path $PSScriptRoot 'private/sql-baseline.tfvars.json')
)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'SqlBaselineInputs.ps1')
Assert-NewSqlBaselineDestination -OutputPath $OutputPath
$server = Get-Content -LiteralPath (Join-Path $InventoryDirectory 'sql-server.json') -Raw | ConvertFrom-Json
$rules = @(Get-Content -LiteralPath (Join-Path $InventoryDirectory 'sql-firewalls.json') -Raw | ConvertFrom-Json)
$inputs = ConvertTo-SqlBaselineInputs -Server $server -Rules $rules
Save-NewSqlBaselineInputs -Inputs $inputs -OutputPath $OutputPath
'Initial SQL inputs captured. Review before adoption; ordinary plans must retain these desired inputs.'
