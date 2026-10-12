function Resolve-AzureCli {
    param([string]$AzureCliPath)
    if ($AzureCliPath) {
        if (!(Test-Path -LiteralPath $AzureCliPath -PathType Leaf)) { throw 'The supplied Azure CLI executable does not exist.' }
        return (Resolve-Path -LiteralPath $AzureCliPath).Path
    }
    $command = Get-Command az -CommandType Application, ExternalScript -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($command) { return $command.Source }
    foreach ($candidate in @(
        'C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin\az.cmd',
        'C:\Program Files (x86)\Microsoft SDKs\Azure\CLI2\wbin\az.cmd'
    )) {
        if (Test-Path -LiteralPath $candidate -PathType Leaf) { return $candidate }
    }
    throw 'Install Azure CLI, make az available on PATH, and sign in with az login.'
}

function Read-AzureJson {
    param([string]$AzureCliPath, [string[]]$Arguments)
    $result = & $AzureCliPath @Arguments --only-show-errors --output json
    if ($LASTEXITCODE -ne 0) { throw "Azure read failed: $($Arguments[0..1] -join ' ')" }
    return ($result -join [Environment]::NewLine) | ConvertFrom-Json
}
