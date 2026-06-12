# Rotina única de deploy (Windows): publicar, enviar e aplicar na VM.
param(
    [string]$VmHost = $env:VM_HOST,
    [string]$VmUser = $env:VM_USER,
    [string]$KeyPath = $env:VM_KEY,
    [string]$PublicBase = $env:PUBLIC_BASE
)

$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot

if (-not $VmHost) { $VmHost = "204.216.162.78" }
if (-not $VmUser) { $VmUser = "ubuntu" }
if (-not $KeyPath) { $KeyPath = Join-Path $Root "chaves\204.216.162.78.key" }
if (-not $PublicBase) { $PublicBase = "https://rpjtechgroup.ddns.net/mercado" }

if (-not (Test-Path $KeyPath)) {
    throw "Chave SSH não encontrada: $KeyPath"
}

$TempKey = Join-Path $env:TEMP "sabor-deploy-$(Get-Random).key"
Get-Content $KeyPath -Raw | Set-Content $TempKey -NoNewline
icacls $TempKey /inheritance:r /grant:r "${env:USERNAME}:(R)" | Out-Null

try {
    Write-Host "==> publish + pack"
    dotnet publish (Join-Path $Root "src\SaborMercado.Api\SaborMercado.Api.csproj") -c Release -o (Join-Path $Root "publish\api")
    dotnet publish (Join-Path $Root "src\SaborMercado.Web\SaborMercado.Web.csproj") -c Release -o (Join-Path $Root "publish\web")

    $packRoot = Join-Path $Root "publish\sabormercado-publish"
    Remove-Item -Recurse -Force $packRoot -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path (Join-Path $packRoot "api") -Force | Out-Null
    New-Item -ItemType Directory -Path (Join-Path $packRoot "wwwroot") -Force | Out-Null
    Copy-Item -Recurse (Join-Path $Root "publish\api\*") (Join-Path $packRoot "api")
    Copy-Item -Recurse (Join-Path $Root "publish\web\wwwroot\*") (Join-Path $packRoot "wwwroot")

    Push-Location $Root
    tar -czf sabormercado-publish.tar.gz -C publish sabormercado-publish
    tar -czf sabormercado-scripts.tar.gz scripts/deploy-sabormercado-vm.sh scripts/nginx-rpjtechgroup.conf scripts/patch-iptables-http.sh
    Pop-Location

    Write-Host "==> upload"
    scp -i $TempKey sabormercado-publish.tar.gz sabormercado-scripts.tar.gz "${VmUser}@${VmHost}:/home/ubuntu/deploy/"

    Write-Host "==> deploy remoto"
    $remote = @"
set -e
cd /home/ubuntu/deploy
tar -xzf sabormercado-scripts.tar.gz
set -a
source sabormercado.secrets
set +a
PUBLIC_BASE=$PublicBase bash scripts/deploy-sabormercado-vm.sh sabormercado-publish.tar.gz
"@
    ssh -i $TempKey "${VmUser}@${VmHost}" $remote
    Write-Host "DEPLOY_OK $PublicBase/"
}
finally {
    Remove-Item $TempKey -Force -ErrorAction SilentlyContinue
}
