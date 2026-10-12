function Assert-ProductionPlan {
    param($Plan)
    $allowed = @(
        'azurerm_service_plan.ropeparison', 'azurerm_linux_web_app.ropeparison',
        'azurerm_application_insights.ropeparison', 'azurerm_monitor_smart_detector_alert_rule.ropeparison',
        'azurerm_mssql_server.shared', 'azurerm_mssql_database.shared',
        'azurerm_mssql_firewall_rule.azure_services', 'azurerm_mssql_firewall_rule.legacy_client',
        'azurerm_mssql_firewall_rule.query_editor'
    )
    if ($Plan.errored -or $Plan.complete -ne $true -or @($Plan.deferred_changes).Where({$null -ne $_}).Count) {
        throw 'Only complete, successful plans can be reviewed for apply.'
    }
    $changes = @()
    foreach ($resource in $Plan.resource_changes) {
        if ($resource.mode -eq 'data') { continue }
        if ($resource.mode -ne 'managed' -or $resource.address -cnotin $allowed) { throw 'Plan includes a resource outside the adopted production scope.' }
        if ($resource.change.importing -or $resource.change.actions -contains 'delete' -or $resource.previous_address) {
            throw 'Imports, moves, deletion and replacement need a separate owner-reviewed procedure.'
        }
        $actions = @($resource.change.actions)
        if ($actions.Count -ne 1 -or $actions[0] -cnotin @('no-op','create','update')) { throw 'Unsupported plan action.' }
        if ($actions[0] -ne 'no-op') { $changes += "$($resource.address): $($actions[0])" }
    }
    return $changes
}

function Assert-PlanBundle {
    param([string]$Directory, [string]$ExpectedManifestHash, [string]$Commit, [string]$RunId, [string]$RunAttempt, [string]$TerraformVersion, [string]$LockHash)
    if ($ExpectedManifestHash -cnotmatch '^[a-f0-9]{64}$' -or
        (Get-FileHash (Join-Path $Directory 'manifest.json') -Algorithm SHA256).Hash.ToLowerInvariant() -cne $ExpectedManifestHash) {
        throw 'Reviewed manifest digest does not match.'
    }
    $manifest = Get-Content (Join-Path $Directory 'manifest.json') -Raw | ConvertFrom-Json
    if ($manifest.repository -cne 'ropeparison/rope-parison' -or $manifest.commit -cne $Commit -or
        $manifest.run_id -cne $RunId -or $manifest.run_attempt -cne $RunAttempt -or
        $manifest.terraform_version -cne $TerraformVersion -or $manifest.lock_hash -cne $LockHash -or
        $manifest.module -cne 'infra' -or $manifest.backend -cne 'tfproduction/production.tfstate') {
        throw 'Plan provenance does not match this workflow run and configuration.'
    }
    # ConvertFrom-Json may deserialize ISO timestamps as DateTime. Casting keeps
    # its UTC kind; calling Parse on its culture-formatted string would lose it.
    $created = [DateTimeOffset]$manifest.created_utc
    $age = [DateTimeOffset]::UtcNow - $created
    if ($age.TotalHours -gt 24 -or $age.TotalMinutes -lt -5) { throw 'Plan approval expired; start a new plan run.' }
    foreach ($name in @('production.tfplan','plan.json','plan.log')) {
        if ($manifest.hashes.$name -cnotmatch '^[a-f0-9]{64}$' -or
            (Get-FileHash (Join-Path $Directory $name) -Algorithm SHA256).Hash.ToLowerInvariant() -cne $manifest.hashes.$name) {
            throw 'Private plan bundle contents differ from the reviewed manifest.'
        }
    }
    Assert-ProductionPlan -Plan (Get-Content (Join-Path $Directory 'plan.json') -Raw | ConvertFrom-Json) | Out-Null
}
