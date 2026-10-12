param([string]$TerraformPath)
$ErrorActionPreference = 'Stop'
if (!$TerraformPath) {
    $command = Get-Command terraform -CommandType Application, ExternalScript -ErrorAction SilentlyContinue | Select-Object -First 1
    if (!$command) { throw 'Install the version in infra/.terraform-version and make terraform available on PATH.' }
    $TerraformPath = $command.Source
}

function Invoke-TerraformCheck([string[]]$Arguments) {
    & $TerraformPath @Arguments
    if ($LASTEXITCODE -ne 0) { throw "Terraform check failed with exit code ${LASTEXITCODE}: $($Arguments -join ' ')" }
}

# Backend-disabled checks use fresh metadata, even on a developer checkout that
# is already initialized against production. No SQL inputs or state are needed.
$previousDataDirectory = $env:TF_DATA_DIR
$temporaryParent = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
$validationDirectory = Join-Path $temporaryParent ('ropeparison-infrastructure-checks-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $validationDirectory | Out-Null
try {
    Invoke-TerraformCheck -Arguments @('fmt','-check','-recursive',$PSScriptRoot)
    $configurations = @(
        @{Name='application'; Directory=$PSScriptRoot},
        @{Name='state-bootstrap'; Directory=(Join-Path $PSScriptRoot 'state-bootstrap')}
    )
    foreach ($configuration in $configurations) {
        $env:TF_DATA_DIR = Join-Path $validationDirectory $configuration.Name
        Invoke-TerraformCheck -Arguments @("-chdir=$($configuration.Directory)",'init','-backend=false','-input=false','-lockfile=readonly','-no-color')
        Invoke-TerraformCheck -Arguments @("-chdir=$($configuration.Directory)",'validate','-no-color')
    }
    & (Join-Path $PSScriptRoot 'tests/Test-InfraHelpers.ps1')
    'Infrastructure checks passed; both backends stayed disabled.'
} finally {
    $env:TF_DATA_DIR = $previousDataDirectory
    # Only remove the newly created directory, using one shell and a checked
    # absolute target under the temporary parent. Never touch checkout metadata.
    $resolvedDirectory = [IO.Path]::GetFullPath($validationDirectory)
    $parentPrefix = $temporaryParent.TrimEnd([char[]]@([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar)) + [IO.Path]::DirectorySeparatorChar
    if (!$resolvedDirectory.StartsWith($parentPrefix, [StringComparison]::OrdinalIgnoreCase) -or
        (Split-Path -Leaf $resolvedDirectory) -notlike 'ropeparison-infrastructure-checks-*') {
        throw 'Refusing cleanup outside the infrastructure validation directory.'
    }
    Remove-Item -LiteralPath $resolvedDirectory -Recurse -Force
}
