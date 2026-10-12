param([string]$InfraDirectory = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = 'Stop'
. (Join-Path $InfraDirectory 'PlanReview.ps1')
$fixture = Join-Path ([IO.Path]::GetTempPath()) ('ropeparison-plan-tests-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixture | Out-Null
$checks = 0
function Assert-True([bool]$Condition) {
    if (!$Condition) { throw 'Plan review regression failed.' }
    $script:checks++
}
function Assert-Throws([scriptblock]$Action, [string]$Pattern) {
    $failure = $null
    try { & $Action | Out-Null } catch { $failure = $_.Exception.Message }
    Assert-True ($null -ne $failure -and $failure -like $Pattern)
}
try {
    $plan = @{complete=$true; errored=$false; resource_changes=@(@{mode='managed'; address='azurerm_linux_web_app.ropeparison'; change=@{actions=@('update')}})}
    Assert-True (@(Assert-ProductionPlan $plan).Count -eq 1)
    $plan.resource_changes[0].change.actions = @('no-op')
    Assert-True (@(Assert-ProductionPlan $plan).Count -eq 0)
    $plan.resource_changes[0].change.actions = @('delete','create')
    Assert-Throws { Assert-ProductionPlan $plan } '*deletion and replacement*'
    $plan.resource_changes[0].change.actions = @('update')
    $plan.resource_changes[0].change.importing = @{id='unexpected'}
    Assert-Throws { Assert-ProductionPlan $plan } '*Imports*'
    $plan.resource_changes[0].change.Remove('importing')
    $plan.resource_changes[0].previous_address = 'old'
    Assert-Throws { Assert-ProductionPlan $plan } '*moves*'
    $plan.resource_changes[0].Remove('previous_address')
    $plan.resource_changes[0].address = 'azurerm_role_assignment.elevated'
    Assert-Throws { Assert-ProductionPlan $plan } '*outside*'
    $plan.resource_changes[0].address = 'azurerm_linux_web_app.ropeparison'
    $plan.resource_changes[0].change.actions = @('forget')
    Assert-Throws { Assert-ProductionPlan $plan } '*Unsupported*'
    $plan.resource_changes[0].change.actions = @('update')
    $plan.complete = $false
    Assert-Throws { Assert-ProductionPlan $plan } '*complete*'
    $plan.complete = $true
    $plan.errored = $true
    Assert-Throws { Assert-ProductionPlan $plan } '*successful*'
    $plan.errored = $false
    $plan.deferred_changes = @(@{address='deferred'})
    Assert-Throws { Assert-ProductionPlan $plan } '*complete*'
    $plan.Remove('deferred_changes')

    $plan | ConvertTo-Json -Depth 10 | Set-Content (Join-Path $fixture 'plan.json')
    'fixture binary' | Set-Content (Join-Path $fixture 'production.tfplan')
    'fixture private log' | Set-Content (Join-Path $fixture 'plan.log')
    $hashes = @{}
    foreach ($name in @('production.tfplan','plan.json','plan.log')) { $hashes[$name] = (Get-FileHash (Join-Path $fixture $name)).Hash.ToLowerInvariant() }
    $manifest = @{
        repository='ropeparison/rope-parison'; commit=('a'*40); run_id='42'; run_attempt='1'
        terraform_version='1.16.5'; lock_hash=('b'*64); module='infra'; backend='tfproduction/production.tfstate'
        created_utc=[DateTimeOffset]::UtcNow.ToString('o'); hashes=$hashes
    }
    function Save-Manifest {
        $manifest | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $fixture 'manifest.json')
        (Get-FileHash (Join-Path $fixture 'manifest.json')).Hash.ToLowerInvariant()
    }
    $arguments = @{Directory=$fixture; ExpectedManifestHash=(Save-Manifest); Commit=('a'*40); RunId='42'; RunAttempt='1'; TerraformVersion='1.16.5'; LockHash=('b'*64)}
    Assert-PlanBundle @arguments
    Assert-True $true
    $arguments.ExpectedManifestHash = '0'*64
    Assert-Throws { Assert-PlanBundle @arguments } '*digest*'
    $arguments.ExpectedManifestHash = Save-Manifest
    foreach ($field in @('repository','commit','run_id','run_attempt','terraform_version','lock_hash','module','backend')) {
        $original = $manifest[$field]
        $manifest[$field] = 'mismatch'
        $arguments.ExpectedManifestHash = Save-Manifest
        Assert-Throws { Assert-PlanBundle @arguments } '*provenance*'
        $manifest[$field] = $original
    }
    $manifest.created_utc = [DateTimeOffset]::UtcNow.AddHours(-25).ToString('o')
    $arguments.ExpectedManifestHash = Save-Manifest
    Assert-Throws { Assert-PlanBundle @arguments } '*expired*'
    $manifest.created_utc = [DateTimeOffset]::UtcNow.AddHours(1).ToString('o')
    $arguments.ExpectedManifestHash = Save-Manifest
    Assert-Throws { Assert-PlanBundle @arguments } '*expired*'
    $manifest.created_utc = [DateTimeOffset]::UtcNow.ToString('o')
    $arguments.ExpectedManifestHash = Save-Manifest
    foreach ($name in @('production.tfplan','plan.json','plan.log')) {
        $original = [IO.File]::ReadAllBytes((Join-Path $fixture $name))
        'tampered' | Set-Content (Join-Path $fixture $name)
        Assert-Throws { Assert-PlanBundle @arguments } '*contents differ*'
        [IO.File]::WriteAllBytes((Join-Path $fixture $name), $original)
    }
    "$checks plan review and artifact integrity checks passed."
} finally {
    $target = [IO.Path]::GetFullPath($fixture)
    $parent = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd([char[]]@('/','\')) + [IO.Path]::DirectorySeparatorChar
    if (!$target.StartsWith($parent, [StringComparison]::OrdinalIgnoreCase) -or (Split-Path -Leaf $target) -notlike 'ropeparison-plan-tests-*') { throw 'Unsafe fixture cleanup path.' }
    Remove-Item -LiteralPath $target -Recurse -Force
}
