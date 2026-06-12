#!/usr/bin/env bash
# Publica API + PWA e gera sabormercado-publish.tar.gz na raiz do repositório.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

echo "==> dotnet publish"
dotnet publish src/SaborMercado.Api/SaborMercado.Api.csproj -c Release -o publish/api
dotnet publish src/SaborMercado.Web/SaborMercado.Web.csproj -c Release -o publish/web

echo "==> pack"
rm -rf publish/sabormercado-publish
mkdir -p publish/sabormercado-publish/api publish/sabormercado-publish/wwwroot
cp -a publish/api/. publish/sabormercado-publish/api/
cp -a publish/web/wwwroot/. publish/sabormercado-publish/wwwroot/
tar -czf sabormercado-publish.tar.gz -C publish sabormercado-publish
tar -czf sabormercado-scripts.tar.gz \
  scripts/deploy-sabormercado-vm.sh \
  scripts/nginx-rpjtechgroup.conf \
  scripts/patch-iptables-http.sh

echo "PACK_OK sabormercado-publish.tar.gz"
