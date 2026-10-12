param(
    [string]$AzureCliPath,
    [string]$Subscription = 'RopeParisonSubscription',
    [string]$ResourceGroup = 'RopeParison',
    [string]$OutputDirectory = (Join-Path $PSScriptRoot 'private/inventory')
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'AzureCli.ps1')
$azPath = Resolve-AzureCli -AzureCliPath $AzureCliPath

# Every Azure operation is a list/show; subscription is explicit for each request.
# Do not collect app-setting values, connection strings, keys or publishing profiles.
$accounts = @(Read-AzureJson -AzureCliPath $azPath -Arguments @('account','list','--query','[].{id:id,name:name,tenantId:tenantId}'))
$subscriptionMatches = @($accounts | Where-Object { $_.name -eq $Subscription -or $_.id -eq $Subscription })
if ($subscriptionMatches.Count -ne 1) { throw 'Expected exactly one matching subscription. Sign in and provide its subscription ID if the name is ambiguous.' }
$account = $subscriptionMatches[0]
$scope = @('--subscription', $account.id, '--resource-group', $ResourceGroup)
$group = Read-AzureJson -AzureCliPath $azPath -Arguments (@('group','show') + $scope + @('--query','{id:id,name:name,location:location,tags:tags}'))
$resources = @(Read-AzureJson -AzureCliPath $azPath -Arguments (@('resource','list') + $scope + @('--query','[].{id:id,name:name,type:type,location:location,kind:kind,tags:tags}')))
$details = @()
foreach ($resource in $resources) {
    $entry = [ordered]@{ id = $resource.id; type = $resource.type }
    switch ($resource.type.ToLowerInvariant()) {
        'microsoft.web/sites' {
            $entry.host = Read-AzureJson -AzureCliPath $azPath -Arguments (@('webapp','show','--name',$resource.name) + $scope + @('--query','{id:id,name:name,kind:kind,serverFarmId:serverFarmId,httpsOnly:httpsOnly,hostNames:hostNames,identity:identity,state:state,publicNetworkAccess:publicNetworkAccess}'))
            $entry.config = Read-AzureJson -AzureCliPath $azPath -Arguments (@('webapp','config','show','--name',$resource.name) + $scope + @('--query','{linuxFxVersion:linuxFxVersion,windowsFxVersion:windowsFxVersion,netFrameworkVersion:netFrameworkVersion,alwaysOn:alwaysOn,webSocketsEnabled:webSocketsEnabled,http20Enabled:http20Enabled,minTlsVersion:minTlsVersion,ftpsState:ftpsState,healthCheckPath:healthCheckPath,numberOfWorkers:numberOfWorkers}'))
        }
        'microsoft.web/serverfarms' {
            $entry.plan = Read-AzureJson -AzureCliPath $azPath -Arguments (@('appservice','plan','show','--name',$resource.name) + $scope + @('--query','{id:id,name:name,kind:kind,sku:sku,reserved:reserved,maximumNumberOfWorkers:maximumNumberOfWorkers}'))
        }
        'microsoft.sql/servers' {
            $entry.server = Read-AzureJson -AzureCliPath $azPath -Arguments (@('sql','server','show','--name',$resource.name) + $scope + @('--query','{id:id,name:name,fullyQualifiedDomainName:fullyQualifiedDomainName,version:version,minimalTlsVersion:minimalTlsVersion,publicNetworkAccess:publicNetworkAccess,identity:identity}'))
            $entry.databases = Read-AzureJson -AzureCliPath $azPath -Arguments (@('sql','db','list','--server',$resource.name) + $scope + @('--query','[].{id:id,name:name,sku:sku,maxSizeBytes:maxSizeBytes,zoneRedundant:zoneRedundant,currentServiceObjectiveName:currentServiceObjectiveName,requestedBackupStorageRedundancy:requestedBackupStorageRedundancy}'))
        }
    }
    $details += [pscustomobject]$entry
}

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$inventory = [ordered]@{
    capturedAtUtc = [DateTime]::UtcNow.ToString('o')
    subscription = $account
    resourceGroup = $group
    resources = $resources
    details = $details
}
$path = Join-Path $OutputDirectory 'azure-inventory.json'
$inventory | ConvertTo-Json -Depth 30 | Set-Content -LiteralPath $path -Encoding utf8
Write-Output "Inventoried $($resources.Count) resources. Saved: $path"
Write-Output 'Supporting/shared resources, domains, access rules, diagnostics, backup retention and cost still require review before Terraform adoption.'
