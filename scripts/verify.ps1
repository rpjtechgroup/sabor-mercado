# Build gate for local development and pre-push checks.
$ErrorActionPreference = "Stop"
Set-Location (Join-Path $PSScriptRoot "..")

Write-Host "==> dotnet build (Release)"
dotnet build SaborMercado.sln -c Release
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "VERIFY_OK"
