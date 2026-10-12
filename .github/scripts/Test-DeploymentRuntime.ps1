param(
    [Parameter(Mandatory=$true)][string]$RuntimeConfigFile,
    [Parameter(Mandatory=$true)][string]$LiveLinuxFxVersion,
    [string]$TerraformFile = (Join-Path $PSScriptRoot '../../infra/main.tf')
)
$ErrorActionPreference = 'Stop'
$runtime = (Get-Content -LiteralPath $RuntimeConfigFile -Raw | ConvertFrom-Json).runtimeOptions
$frameworks = @($runtime.framework) + @($runtime.frameworks) | Where-Object { $_ }
$aspnet = @($frameworks | Where-Object name -eq 'Microsoft.AspNetCore.App')
$netcore = @($frameworks | Where-Object name -eq 'Microsoft.NETCore.App')
if ($aspnet.Count -ne 1 -or $netcore.Count -ne 1) { throw 'Expected a framework-dependent ASP.NET Core artifact.' }
$required = [version]$aspnet[0].version
$core = [version]$netcore[0].version
if ($required.Major -ne $core.Major -or $required.Minor -ne $core.Minor) { throw 'Artifact framework versions disagree.' }
$target = "$($required.Major).$($required.Minor)"
if ($runtime.tfm -ne "net$target") { throw 'Artifact target framework and runtime versions disagree.' }
$stackDeclarations = [regex]::Matches((Get-Content -LiteralPath $TerraformFile -Raw), '(?m)^\s*dotnet_version\s*=\s*"([0-9]+\.[0-9]+)"\s*$')
if ($stackDeclarations.Count -ne 1) { throw 'Expected exactly one explicit Terraform .NET hosting stack.' }
$declared = $stackDeclarations[0].Groups[1].Value
if ($declared -ne $target) { throw "Deployment blocked: artifact requires .NET $target but Terraform declares $declared. Coordinate the production runtime release first." }
if ($LiveLinuxFxVersion.Trim() -notmatch '^DOTNETCORE\|([0-9]+\.[0-9]+)$' -or $Matches[1] -ne $target) {
    throw "Deployment blocked: live Azure hosting does not match the artifact's .NET $target runtime."
}
"Deployment runtime verified: artifact, Terraform and live Azure hosting all use .NET $target."
