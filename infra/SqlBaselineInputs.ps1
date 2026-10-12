function ConvertTo-SqlBaselineInputs {
    param($Server, [object[]]$Rules)
    $expectedId = '/subscriptions/ad052b7b-2015-4933-93eb-0b857cabccae/resourceGroups/RopeParison/providers/Microsoft.Sql/servers/ropeparison'
    if ($Server.id -ine $expectedId -or $Server.name -ne 'ropeparison' -or
        !$Server.administrators.sid -or !$Server.administrators.login -or
        $Server.administrators.tenantId -ne 'a2155124-112f-4b17-a715-0ca032e18b28') {
        throw 'Missing/mismatched SQL server or Entra administrator metadata.'
    }
    $names = [ordered]@{
        legacy_client = 'ClientIPAddress_2024-1-28_21-37-20'
        query_editor = 'QueryEditorClientIPAddress_1789646070751'
    }
    $azureRule = @($Rules | Where-Object name -eq 'AllowAllWindowsAzureIps')
    if ($Rules.Count -ne 3 -or $azureRule.Count -ne 1 -or
        $azureRule[0].startIpAddress -ne '0.0.0.0' -or $azureRule[0].endIpAddress -ne '0.0.0.0') {
        throw 'SQL firewall inventory differs from this baseline. Review declarations before proceeding.'
    }
    $clients = [ordered]@{}
    foreach ($key in $names.Keys) {
        $match = @($Rules | Where-Object name -eq $names[$key])
        if ($match.Count -ne 1 -or !$match[0].startIpAddress -or !$match[0].endIpAddress) {
            throw "Missing/mismatched firewall metadata: $key"
        }
        $clients[$key] = [ordered]@{start_ip_address=$match[0].startIpAddress; end_ip_address=$match[0].endIpAddress}
    }
    return [ordered]@{
        sql_existing_administrator = [ordered]@{
            login_username = $Server.administrators.login
            object_id = $Server.administrators.sid
            tenant_id = $Server.administrators.tenantId
        }
        sql_client_firewall_rules = $clients
    }
}

function Save-NewSqlBaselineInputs {
    param($Inputs, [string]$OutputPath)
    $directory = Split-Path -Parent ([IO.Path]::GetFullPath($OutputPath))
    New-Item -ItemType Directory -Force -Path $directory | Out-Null
    # CreateNew prevents accidental overwrite and concurrent initialization.
    $stream = [IO.File]::Open($OutputPath, [IO.FileMode]::CreateNew, [IO.FileAccess]::Write, [IO.FileShare]::None)
    $writer = [IO.StreamWriter]::new($stream, [Text.UTF8Encoding]::new($false))
    try { $writer.WriteLine(($Inputs | ConvertTo-Json -Depth 8)) }
    finally { $writer.Dispose() }
}

function Assert-NewSqlBaselineDestination {
    param([string]$OutputPath)
    if (Test-Path -LiteralPath $OutputPath) {
        throw 'Reviewed SQL inputs already exist. Refusing to overwrite them. Compare inventory separately and review intentional edits.'
    }
}
