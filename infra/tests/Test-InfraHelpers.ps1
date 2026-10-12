param([string]$InfraDirectory = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = 'Stop'
$fixture = Join-Path ([IO.Path]::GetTempPath()) ('ropeparison-infra-tests-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixture | Out-Null
$previousFixture = $env:ROPEPARISON_HELPER_FIXTURE
$env:ROPEPARISON_HELPER_FIXTURE = $fixture
$checks = 0
function Assert-True([bool]$Condition, [string]$Message) {
    if (!$Condition) { throw $Message }
    $script:checks++
}
function Assert-Throws([scriptblock]$Action, [string]$Pattern) {
    $failure = $null
    try { & $Action | Out-Null } catch { $failure = $_.Exception.Message }
    Assert-True ($null -ne $failure -and $failure -like $Pattern) "Expected failure matching $Pattern"
}
try {
    $base = '/subscriptions/ad052b7b-2015-4933-93eb-0b857cabccae/resourceGroups/RopeParison/providers/Microsoft.Sql/servers/ropeparison'
    $server = @{id=$base; name='ropeparison'; administrators=@{login='fixture@example.invalid'; sid='00000000-0000-0000-0000-000000000001'; tenantId='a2155124-112f-4b17-a715-0ca032e18b28'}}
    $rules = @(
        @{name='AllowAllWindowsAzureIps'; startIpAddress='0.0.0.0'; endIpAddress='0.0.0.0'},
        @{name='ClientIPAddress_2024-1-28_21-37-20'; startIpAddress='192.0.2.1'; endIpAddress='192.0.2.1'},
        @{name='QueryEditorClientIPAddress_1789646070751'; startIpAddress='192.0.2.2'; endIpAddress='192.0.2.2'}
    )
    $server | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $fixture 'sql-server.json')
    $rules | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $fixture 'sql-firewalls.json')
    $stub = Join-Path $fixture 'az-fixture.ps1'
    @'
param([Parameter(ValueFromRemainingArguments=$true)][string[]]$CliArguments)
$key = $CliArguments[0..2] -join ' '
if ($CliArguments[0] -ne 'account' -and $CliArguments -notcontains '--subscription') { throw 'Missing explicit subscription.' }
if ($key -eq 'sql server show') { Get-Content (Join-Path $env:ROPEPARISON_HELPER_FIXTURE 'sql-server.json') -Raw }
elseif ($key -eq 'sql server firewall-rule') { Get-Content (Join-Path $env:ROPEPARISON_HELPER_FIXTURE 'sql-firewalls.json') -Raw }
elseif (($CliArguments[0..1] -join ' ') -eq 'account list') { '[{"name":"RopeParisonSubscription","id":"ad052b7b-2015-4933-93eb-0b857cabccae","tenantId":"a2155124-112f-4b17-a715-0ca032e18b28"}]' }
elseif (($CliArguments[0..1] -join ' ') -eq 'group show') { '{"name":"RopeParison"}' }
else { '[]' }
exit 0
'@ | Set-Content $stub
    $cold = Join-Path $fixture 'cold/inventory'
    & (Join-Path $InfraDirectory 'Inspect-RopeParisonDependencies.ps1') -AzureCliPath $stub -OutputDirectory $cold | Out-Null
    Assert-True (Test-Path (Join-Path $cold 'sql-server.json')) 'Inspector failed without an existing top-level inventory.'
    Assert-True (!(Test-Path (Join-Path $cold 'azure-inventory.json'))) 'Inspector unexpectedly needed/created a top-level inventory.'
    & (Join-Path $InfraDirectory 'Inventory-Azure.ps1') -AzureCliPath $stub -OutputDirectory $cold | Out-Null
    Assert-True (Test-Path (Join-Path $cold 'azure-inventory.json')) 'Portable inventory invocation failed.'
    . (Join-Path $InfraDirectory 'AzureCli.ps1')
    Assert-Throws { Resolve-AzureCli -AzureCliPath (Join-Path $fixture 'missing-cli') } '*does not exist*'

    $desired = Join-Path $fixture 'desired.json'
    $writer = Join-Path $InfraDirectory 'Write-SqlBaselineInputs.ps1'
    $drift = Join-Path $InfraDirectory 'Test-SqlBaselineDrift.ps1'
    & $writer -InventoryDirectory $cold -OutputPath $desired | Out-Null
    & $drift -InventoryDirectory $cold -InputPath $desired | Out-Null
    $original = [IO.File]::ReadAllBytes($desired)
    $rules[1].startIpAddress = '192.0.2.3'
    $rules[1].endIpAddress = '192.0.2.3'
    $rules | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $cold 'sql-firewalls.json')
    Assert-Throws { & $writer -InventoryDirectory $cold -OutputPath $desired } '*Refusing to overwrite*'
    Assert-Throws { & $drift -InventoryDirectory $cold -InputPath $desired } '*sql_client_firewall_rules.legacy_client*'
    Assert-True ([Convert]::ToBase64String($original) -eq [Convert]::ToBase64String([IO.File]::ReadAllBytes($desired))) 'Reviewed inputs changed after a rejected capture or drift check.'
    $server.administrators.sid = '00000000-0000-0000-0000-000000000099'
    $server | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $cold 'sql-server.json')
    Assert-Throws { & $drift -InventoryDirectory $cold -InputPath $desired } '*sql_existing_administrator.object_id*'

    # Restore uses last-applied state, even when live metadata has changed.
    $recorded = Get-Content $desired -Raw | ConvertFrom-Json
    $resources = @(@{mode='managed'; type='azurerm_mssql_server'; name='shared'; instances=@(@{attributes=@{id=$base; name='ropeparison'; azuread_administrator=@(@{login_username=$recorded.sql_existing_administrator.login_username; object_id=$recorded.sql_existing_administrator.object_id; tenant_id=$recorded.sql_existing_administrator.tenant_id})}})})
    foreach ($name in @('azure_services','legacy_client','query_editor')) {
        $ruleName = switch($name) { 'azure_services' {'AllowAllWindowsAzureIps'} 'legacy_client' {'ClientIPAddress_2024-1-28_21-37-20'} 'query_editor' {'QueryEditorClientIPAddress_1789646070751'} }
        $ip = if ($name -eq 'azure_services') { '0.0.0.0' } else { $recorded.sql_client_firewall_rules.$name.start_ip_address }
        $resources += @{mode='managed'; type='azurerm_mssql_firewall_rule'; name=$name; instances=@(@{attributes=@{id="$base/firewallRules/$ruleName"; server_id=$base; name=$ruleName; start_ip_address=$ip; end_ip_address=$ip}})}
    }
    @{resources=$resources} | ConvertTo-Json -Depth 20 | Set-Content (Join-Path $fixture 'state.json')
    $terraform = Join-Path $fixture 'terraform-fixture.ps1'
    @'
param([Parameter(ValueFromRemainingArguments=$true)][string[]]$CliArguments)
if (($CliArguments -join ' ') -ne 'state pull') { throw 'Unexpected Terraform operation.' }
Get-Content (Join-Path $env:ROPEPARISON_HELPER_FIXTURE 'state.json') -Raw
exit 0
'@ | Set-Content $terraform
    $restored = Join-Path $fixture 'restored.json'
    $restore = Join-Path $InfraDirectory 'Restore-SqlBaselineInputs.ps1'
    & $restore -TerraformPath $terraform -OutputPath $restored | Out-Null
    Assert-True ((Get-Content $restored -Raw) -eq (Get-Content $desired -Raw)) 'Restore used live metadata instead of recorded desired settings.'
    Assert-Throws { & $restore -TerraformPath $terraform -OutputPath $restored } '*Refusing to overwrite*'
    $resources[0].instances[0].attributes.id = '/wrong-subscription/server'
    @{resources=$resources} | ConvertTo-Json -Depth 20 | Set-Content (Join-Path $fixture 'state.json')
    Assert-Throws { & $restore -TerraformPath $terraform -OutputPath (Join-Path $fixture 'wrong.json') } '*different SQL server*'

    $gate = Join-Path $InfraDirectory '../.github/scripts/Test-DeploymentRuntime.ps1'
    $config = Join-Path $fixture 'runtimeconfig.json'
    @{runtimeOptions=@{tfm='net10.0'; frameworks=@(@{name='Microsoft.NETCore.App'; version='10.0.0'},@{name='Microsoft.AspNetCore.App'; version='10.0.0'})}} | ConvertTo-Json -Depth 8 | Set-Content $config
    $hosting = Join-Path $fixture 'host.tf'
    'dotnet_version = "7.0"' | Set-Content $hosting
    Assert-Throws { & $gate -RuntimeConfigFile $config -TerraformFile $hosting -LiveLinuxFxVersion 'DOTNETCORE|7.0' } '*Terraform declares 7.0*'
    'dotnet_version = "10.0"' | Set-Content $hosting
    Assert-Throws { & $gate -RuntimeConfigFile $config -TerraformFile $hosting -LiveLinuxFxVersion 'DOTNETCORE|7.0' } '*live Azure hosting*'
    Assert-Throws { & $gate -RuntimeConfigFile $config -TerraformFile $hosting -LiveLinuxFxVersion '' } '*empty string*'
    & $gate -RuntimeConfigFile $config -TerraformFile $hosting -LiveLinuxFxVersion 'DOTNETCORE|10.0' | Out-Null
    Assert-True $true 'Matching runtimes did not pass.'
    "Passed $checks infrastructure helper and deployment checks using synthetic data; no Azure writes or requests."
} finally { $env:ROPEPARISON_HELPER_FIXTURE = $previousFixture }
