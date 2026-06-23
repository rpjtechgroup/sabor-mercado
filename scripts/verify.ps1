# Build + test gate for local development and pre-push checks.
$ErrorActionPreference = "Stop"
Set-Location (Join-Path $PSScriptRoot "..")

Write-Host "==> dotnet build (Release)"
dotnet build SaborMercado.sln -c Release

Write-Host "==> dotnet test (Release)"
dotnet test SaborMercado.sln -c Release --no-build --verbosity minimal
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "VERIFY_OK"
