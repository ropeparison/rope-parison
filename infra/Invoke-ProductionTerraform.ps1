param(
    [Parameter(Mandatory)][ValidateSet('Plan','Apply')][string]$Mode,
    [string]$BlobPrefix,
    [string]$ManifestHash
)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'PlanReview.ps1')
$private = Join-Path $PSScriptRoot 'private/workflow'
New-Item -ItemType Directory -Force -Path $private | Out-Null
$terraform = (Get-Command terraform -CommandType Application -ErrorAction Stop).Source
$version = (Get-Content (Join-Path $PSScriptRoot '.terraform-version') -Raw).Trim()
$actualVersion = (& $terraform version -json | ConvertFrom-Json).terraform_version
if ($LASTEXITCODE -ne 0 -or $actualVersion -cne $version) { throw 'Terraform CLI does not match the pinned version.' }
if ($env:GITHUB_REPOSITORY -cne 'ropeparison/rope-parison' -or $env:GITHUB_EVENT_NAME -cne 'workflow_dispatch' -or
    $env:GITHUB_SHA -cnotmatch '^[a-f0-9]{40}$' -or $env:GITHUB_RUN_ID -notmatch '^[0-9]+$' -or $env:GITHUB_RUN_ATTEMPT -notmatch '^[0-9]+$') {
    throw 'Credentialed infrastructure commands require the trusted manual workflow.'
}
if ($env:GITHUB_REF -cnotin @('refs/heads/main','refs/heads/major-update') -or
    ($Mode -eq 'Apply' -and $env:GITHUB_REF -cne 'refs/heads/main')) { throw 'Untrusted branch for this operation.' }
$account = az account show --output json --only-show-errors | ConvertFrom-Json
if ($LASTEXITCODE -ne 0 -or $account.id -cne 'ad052b7b-2015-4933-93eb-0b857cabccae' -or $account.tenantId -cne 'a2155124-112f-4b17-a715-0ca032e18b28') {
    throw 'Azure account does not match the reviewed subscription and tenant.'
}
$expectedPrefix = "runs/$($env:GITHUB_RUN_ID)/$($env:GITHUB_RUN_ATTEMPT)/$($env:GITHUB_SHA)"
$lockHash = (Get-FileHash (Join-Path $PSScriptRoot '.terraform.lock.hcl') -Algorithm SHA256).Hash.ToLowerInvariant()
function Invoke-PrivateTerraform([string[]]$Arguments, [string]$LogName, [int[]]$AllowedCodes = @(0)) {
    & $terraform "-chdir=$PSScriptRoot" @Arguments *> (Join-Path $private $LogName)
    if ($LASTEXITCODE -notin $AllowedCodes) { throw "Terraform failed; diagnostic file $LogName is private. No apply was inferred or replanned." }
}
function Invoke-PlanStorage([string]$Operation, [string]$Name) {
    az storage blob $Operation --account-name strpstatead052b7b2015 --container-name tfplans --name "$expectedPrefix/$Name" --file (Join-Path $private $Name) --auth-mode login --overwrite false --output none --only-show-errors 2> (Join-Path $private 'storage-error.log')
    if ($LASTEXITCODE -ne 0) { throw 'Private plan storage operation failed. Review the private storage diagnostic.' }
}
Invoke-PrivateTerraform -Arguments @('init','-input=false','-lockfile=readonly','-no-color','-backend-config=backend.azurerm.hcl') -LogName 'init.log'
if ($Mode -eq 'Plan') {
    if ([string]::IsNullOrWhiteSpace($env:ROPEPARISON_SQL_INPUTS_JSON)) { throw 'Protected reviewed SQL inputs are missing.' }
    # Never interpolate private input expressions into workflow source or logs.
    try { $inputs = $env:ROPEPARISON_SQL_INPUTS_JSON | ConvertFrom-Json -ErrorAction Stop } catch { throw 'Protected SQL inputs are not valid JSON.' }
    if (!$inputs.sql_existing_administrator.object_id -or !$inputs.sql_client_firewall_rules.legacy_client.start_ip_address) { throw 'Protected SQL inputs have an unexpected shape.' }
    $inputPath = Join-Path $private 'sql-baseline.tfvars.json'
    [IO.File]::WriteAllText($inputPath, $env:ROPEPARISON_SQL_INPUTS_JSON, [Text.UTF8Encoding]::new($false))
    $env:ROPEPARISON_SQL_INPUTS_JSON = $null
    $planPath = Join-Path $private 'production.tfplan'
    Invoke-PrivateTerraform -Arguments @('plan','-input=false','-lock-timeout=2m','-no-color','-detailed-exitcode',"-var-file=$inputPath","-out=$planPath") -LogName 'plan.log' -AllowedCodes @(0,2)
    & $terraform "-chdir=$PSScriptRoot" show -json $planPath > (Join-Path $private 'plan.json') 2> (Join-Path $private 'show-error.log')
    if ($LASTEXITCODE -ne 0) { throw 'Cannot inspect the saved plan.' }
    $changes = @(Assert-ProductionPlan -Plan (Get-Content (Join-Path $private 'plan.json') -Raw | ConvertFrom-Json))
    $hashes = @{}
    foreach ($name in @('production.tfplan','plan.json','plan.log')) { $hashes[$name] = (Get-FileHash (Join-Path $private $name) -Algorithm SHA256).Hash.ToLowerInvariant() }
    $manifest = @{
        repository=$env:GITHUB_REPOSITORY; commit=$env:GITHUB_SHA; run_id=$env:GITHUB_RUN_ID; run_attempt=$env:GITHUB_RUN_ATTEMPT
        terraform_version=$version; lock_hash=$lockHash; module='infra'; backend='tfproduction/production.tfstate'
        created_utc=[DateTimeOffset]::UtcNow.ToString('o'); hashes=$hashes
    }
    $manifest | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $private 'manifest.json') -Encoding utf8
    $ManifestHash = (Get-FileHash (Join-Path $private 'manifest.json') -Algorithm SHA256).Hash.ToLowerInvariant()
    foreach ($name in @('production.tfplan','plan.json','plan.log','manifest.json')) { Invoke-PlanStorage -Operation 'upload' -Name $name }
    "blob_prefix=$expectedPrefix", "manifest_hash=$ManifestHash" | Out-File $env:GITHUB_OUTPUT -Append -Encoding utf8
    @("## Production infrastructure plan", "Commit: $($env:GITHUB_SHA)", "Private bundle: tfplans/$expectedPrefix", "Manifest SHA256: $ManifestHash", "Changes: $($changes.Count)", "", $changes, "", 'Review the private plan before approving Terraform-Apply. Approval expires after 24 hours.') | Out-File $env:GITHUB_STEP_SUMMARY -Append -Encoding utf8
    "Saved private plan with $($changes.Count) managed changes."
} else {
    if ($BlobPrefix -cne $expectedPrefix) { throw 'Apply must use the plan from this exact run and attempt.' }
    foreach ($name in @('production.tfplan','plan.json','plan.log','manifest.json')) { Invoke-PlanStorage -Operation 'download' -Name $name }
    Assert-PlanBundle -Directory $private -ExpectedManifestHash $ManifestHash -Commit $env:GITHUB_SHA -RunId $env:GITHUB_RUN_ID -RunAttempt $env:GITHUB_RUN_ATTEMPT -TerraformVersion $version -LockHash $lockHash
    # No -auto-approve, -target, new variables, plan regeneration or fallback.
    Invoke-PrivateTerraform -Arguments @('apply','-input=false','-lock-timeout=2m','-no-color',(Join-Path $private 'production.tfplan')) -LogName 'apply.log'
    'The exact approved plan was applied successfully.'
}
