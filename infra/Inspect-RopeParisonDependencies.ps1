param(
    [string]$AzureCliPath,
    [string]$OutputDirectory = (Join-Path $PSScriptRoot 'private/inventory')
)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'AzureCli.ps1')
$azPath = Resolve-AzureCli -AzureCliPath $AzureCliPath
$subscription = 'ad052b7b-2015-4933-93eb-0b857cabccae'
$scope = @('--subscription', $subscription, '--resource-group', 'RopeParison')
$data = [ordered]@{}
$data.appSettingNames = Read-AzureJson -AzureCliPath $azPath -Arguments (@('webapp','config','appsettings','list','--name','ropeparison') + $scope + @('--query','[].{name:name,slotSetting:slotSetting}'))
$data.connectionStringNames = Read-AzureJson -AzureCliPath $azPath -Arguments (@('webapp','config','connection-string','list','--name','ropeparison') + $scope + @('--query','[].{name:name,type:type,slotSetting:slotSetting}'))
$data.slots = Read-AzureJson -AzureCliPath $azPath -Arguments (@('webapp','deployment','slot','list','--name','ropeparison') + $scope + @('--query','[].{id:id,name:name,state:state}'))
$data.hostnames = Read-AzureJson -AzureCliPath $azPath -Arguments (@('webapp','config','hostname','list','--webapp-name','ropeparison') + $scope + @('--query','[].{name:name,hostNameType:hostNameType,sslState:sslState}'))
$data.firewallRules = Read-AzureJson -AzureCliPath $azPath -Arguments (@('sql','server','firewall-rule','list','--server','ropeparison') + $scope + @('--query','[].{id:id,name:name,startIpAddress:startIpAddress,endIpAddress:endIpAddress}'))
$data.shortTermRetention = Read-AzureJson -AzureCliPath $azPath -Arguments (@('sql','db','str-policy','show','--server','ropeparison','--name','RopeParisonData') + $scope)
$data.longTermRetention = Read-AzureJson -AzureCliPath $azPath -Arguments (@('sql','db','ltr-policy','show','--server','ropeparison','--name','RopeParisonData') + $scope)
$inventoryPath = $OutputDirectory
$data.sqlServer = Read-AzureJson -AzureCliPath $azPath -Arguments (@('sql','server','show','--name','ropeparison') + $scope + @('--query','{id:id,name:name,location:location,administratorLogin:administratorLogin,version:version,minimalTlsVersion:minimalTlsVersion,publicNetworkAccess:publicNetworkAccess,identity:identity,administrators:administrators}'))
$resources = @(Read-AzureJson -AzureCliPath $azPath -Arguments (@('resource','list') + $scope + @('--query','[].{id:id,name:name,type:type}')))
$data.monitoring = @()
foreach ($resource in $resources | Where-Object { $_.type -in @('microsoft.insights/components','microsoft.alertsmanagement/smartDetectorAlertRules','microsoft.insights/actiongroups') }) {
    $item = Read-AzureJson -AzureCliPath $azPath -Arguments @('resource','show','--ids',$resource.id,'--subscription',$subscription,'--query','{id:id,name:name,type:type,kind:kind,workspaceId:properties.WorkspaceResourceId,retentionInDays:properties.RetentionInDays,ingestionMode:properties.IngestionMode,scope:properties.scope,actionGroups:properties.actionGroups,enabled:properties.enabled}')
    $data.monitoring += $item
}
$data.workspaces = @()
foreach ($workspaceId in @($data.monitoring.workspaceId | Where-Object { $_ } | Select-Object -Unique)) {
    $data.workspaces += Read-AzureJson -AzureCliPath $azPath -Arguments @('resource','show','--ids',$workspaceId,'--subscription',$subscription,'--query','{id:id,name:name,type:type,location:location,sku:properties.sku,retentionInDays:properties.retentionInDays}')
}
$data.existingStorageAccounts = Read-AzureJson -AzureCliPath $azPath -Arguments @('resource','list','--subscription',$subscription,'--resource-type','Microsoft.Storage/storageAccounts','--query','[].{id:id,name:name,resourceGroup:resourceGroup,location:location}')
$data.capturedAtUtc = [DateTime]::UtcNow.ToString('o')
New-Item -ItemType Directory -Force -Path $inventoryPath | Out-Null
$data | ConvertTo-Json -Depth 25 | Set-Content -LiteralPath (Join-Path $inventoryPath 'ropeparison-dependencies.json') -Encoding utf8
$data.sqlServer | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath (Join-Path $inventoryPath 'sql-server.json') -Encoding utf8
$data.firewallRules | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $inventoryPath 'sql-firewalls.json') -Encoding utf8
'Dependency inventory saved; app-setting values and connection-string values were excluded.'
