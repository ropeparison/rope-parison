param(
    [string]$TerraformPath,
    [string]$OutputPath = (Join-Path $PSScriptRoot 'private/sql-baseline.tfvars.json')
)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'SqlBaselineInputs.ps1')
Assert-NewSqlBaselineDestination -OutputPath $OutputPath
if (!$TerraformPath) {
    $command = Get-Command terraform -CommandType Application, ExternalScript -ErrorAction SilentlyContinue | Select-Object -First 1
    if (!$command) { throw 'Install Terraform and make terraform available on PATH.' }
    $TerraformPath = $command.Source
}
# Read initialized production state, not live SQL metadata. Do not print or save
# the full state, which can contain unrelated application secrets.
Push-Location $PSScriptRoot
try {
    $rawState = & $TerraformPath state pull
    if ($LASTEXITCODE -ne 0) { throw 'Cannot read production state. Initialize its backend and sign in first.' }
    $state = ($rawState -join [Environment]::NewLine) | ConvertFrom-Json
} finally { Pop-Location }
function Get-StateAttributes([string]$Type, [string]$Name) {
    $resources = @($state.resources | Where-Object { $_.mode -eq 'managed' -and !$_.module -and $_.type -eq $Type -and $_.name -eq $Name })
    if ($resources.Count -ne 1 -or $resources[0].instances.Count -ne 1) { throw "Missing/mismatched production state resource: $Type.$Name" }
    return $resources[0].instances[0].attributes
}
$server = Get-StateAttributes 'azurerm_mssql_server' 'shared'
if ($server.azuread_administrator.Count -ne 1) { throw 'Missing/mismatched state administrator.' }
$administrator = $server.azuread_administrator[0]
$rules = foreach ($name in @('azure_services','legacy_client','query_editor')) {
    $rule = Get-StateAttributes 'azurerm_mssql_firewall_rule' $name
    if ($rule.server_id -ine $server.id) { throw 'Firewall belongs to a different SQL server.' }
    @{name=$rule.name; startIpAddress=$rule.start_ip_address; endIpAddress=$rule.end_ip_address}
}
$metadata = @{
    id=$server.id; name=$server.name
    administrators=@{login=$administrator.login_username; sid=$administrator.object_id; tenantId=$administrator.tenant_id}
}
$inputs = ConvertTo-SqlBaselineInputs -Server $metadata -Rules $rules
Save-NewSqlBaselineInputs -Inputs $inputs -OutputPath $OutputPath
'SQL inputs restored from recorded production state. Review before use; existing desired inputs were not overwritten.'
