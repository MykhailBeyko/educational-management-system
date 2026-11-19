param([string]$Url = 'http://localhost:5252')
$ErrorActionPreference = 'Stop'
$workspacePath = Split-Path -Parent $PSScriptRoot
$securityPath = Join-Path $workspacePath '_future_steps/Security'
if (-not (Test-Path -LiteralPath (Join-Path $securityPath 'Security/Security.csproj'))) {
    git clone https://github.com/negativchik09/Security.git $securityPath
    if ($LASTEXITCODE -ne 0) { throw 'Could not download Security API.' }
}
# The upstream demo key is shorter than the 32 bytes required by its JWT library.
# Override configuration locally; no upstream source changes are needed.
if (-not $env:Jwt__Key) { $env:Jwt__Key = 'LocalStudySecurityDevelopmentKey_32bytes_minimum' }
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:DOTNET_ENVIRONMENT = 'Development'
# Restricted Windows profiles may not permit the EventLog logger to write.
$env:Logging__EventLog__LogLevel__Default = 'None'
Push-Location (Join-Path $securityPath 'Security')
try {
    Write-Host "Swagger: $Url/swagger/index.html"
    dotnet run --project Security.csproj --no-launch-profile --urls $Url
    if ($LASTEXITCODE -ne 0) { throw 'Security API exited with an error.' }
}
finally { Pop-Location }
