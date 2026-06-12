#!/usr/bin/env bash
# Rotina única: publicar, enviar e aplicar na VM.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

VM_HOST="${VM_HOST:-204.216.162.78}"
VM_USER="${VM_USER:-ubuntu}"
VM_KEY="${VM_KEY:-$ROOT/chaves/204.216.162.78.key}"
DEPLOY_DIR="${VM_DEPLOY_DIR:-/home/ubuntu/deploy}"
PUBLIC_BASE="${PUBLIC_BASE:-https://rpjtechgroup.ddns.net/mercado}"

if [ ! -f "$VM_KEY" ]; then
  echo "Chave SSH não encontrada: $VM_KEY" >&2
  echo "Defina VM_KEY ou coloque a chave em chaves/204.216.162.78.key" >&2
  exit 1
fi

bash "$ROOT/scripts/pack-publish.sh"

echo "==> upload -> ${VM_USER}@${VM_HOST}:${DEPLOY_DIR}"
scp -i "$VM_KEY" -o StrictHostKeyChecking=accept-new \
  sabormercado-publish.tar.gz sabormercado-scripts.tar.gz \
  "${VM_USER}@${VM_HOST}:${DEPLOY_DIR}/"

echo "==> deploy remoto"
ssh -i "$VM_KEY" -o StrictHostKeyChecking=accept-new "${VM_USER}@${VM_HOST}" bash -s <<REMOTE
set -euo pipefail
cd ${DEPLOY_DIR}
tar -xzf sabormercado-scripts.tar.gz
set -a
source sabormercado.secrets
set +a
PUBLIC_BASE=${PUBLIC_BASE} bash scripts/deploy-sabormercado-vm.sh sabormercado-publish.tar.gz
REMOTE

echo "DEPLOY_OK ${PUBLIC_BASE}/"
