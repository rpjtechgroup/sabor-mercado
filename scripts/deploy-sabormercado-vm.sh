#!/usr/bin/env bash
# Deploy Sabor Mercado under /home/ubuntu/sabormercado (nginx: /mercado/).
set -euo pipefail

APP_ROOT="/home/ubuntu/sabormercado"
API_PUBLISH="$APP_ROOT/api"
WEB_ROOT="$APP_ROOT/wwwroot"
API_PORT="5280"
PUBLIC_BASE="${PUBLIC_BASE:-https://rpjtechgroup.ddns.net/mercado}"
ARCHIVE="${1:-/home/ubuntu/deploy/sabormercado-publish.tar.gz}"

DB_NAME="sabormercado"
DB_USER="sabormercado"
DB_PASS="${SABOR_DB_PASS:?Set SABOR_DB_PASS}"
JWT_KEY="${SABOR_JWT_KEY:?Set SABOR_JWT_KEY}"
SMTP_USER="${SABOR_SMTP_USER:-rpjtechgroup@gmail.com}"
SMTP_PASSWORD="${SABOR_SMTP_PASSWORD:?Set SABOR_SMTP_PASSWORD}"
GOOGLE_CLIENT_ID="${SABOR_GOOGLE_CLIENT_ID:?Set SABOR_GOOGLE_CLIENT_ID}"

echo "==> iptables: allow HTTP/HTTPS (OCI image blocks by default)"
if [ -x /tmp/patch-iptables-http.sh ]; then
  sudo /tmp/patch-iptables-http.sh
elif [ -f "$(dirname "$0")/patch-iptables-http.sh" ]; then
  sudo bash "$(dirname "$0")/patch-iptables-http.sh"
fi

echo "==> Swap (1GB) if missing"
if ! swapon --show | grep -q '/swapfile'; then
  sudo fallocate -l 1G /swapfile || sudo dd if=/dev/zero of=/swapfile bs=1M count=1024
  sudo chmod 600 /swapfile
  sudo mkswap /swapfile
  sudo swapon /swapfile
  grep -q '/swapfile' /etc/fstab || echo '/swapfile none swap sw 0 0' | sudo tee -a /etc/fstab
fi

echo "==> .NET 8 runtime"
if ! dotnet --list-runtimes 2>/dev/null | grep -q 'Microsoft.AspNetCore.App 8.'; then
  sudo apt-get update -y
  sudo DEBIAN_FRONTEND=noninteractive apt-get install -y aspnetcore-runtime-8.0
fi

echo "==> PostgreSQL"
if ! command -v psql >/dev/null 2>&1; then
  sudo apt-get update -y
  sudo DEBIAN_FRONTEND=noninteractive apt-get install -y postgresql postgresql-contrib
fi
sudo systemctl enable postgresql
sudo systemctl start postgresql

sudo -u postgres psql -v ON_ERROR_STOP=1 <<SQL
DO \$\$
BEGIN
  IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = '${DB_USER}') THEN
    CREATE ROLE ${DB_USER} LOGIN PASSWORD '${DB_PASS}';
  ELSE
    ALTER ROLE ${DB_USER} WITH PASSWORD '${DB_PASS}';
  END IF;
END\$\$;
SELECT 'CREATE DATABASE ${DB_NAME} OWNER ${DB_USER}'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = '${DB_NAME}')\gexec
GRANT ALL PRIVILEGES ON DATABASE ${DB_NAME} TO ${DB_USER};
SQL

CONN="Host=127.0.0.1;Port=5432;Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASS}"

echo "==> Extract publish archive -> ${APP_ROOT}"
EXTRACT_DIR="$(mktemp -d)"
mkdir -p "$API_PUBLISH" "$WEB_ROOT"
tar -xzf "$ARCHIVE" -C "$EXTRACT_DIR"
rsync -a --delete "$EXTRACT_DIR/sabormercado-publish/api/" "$API_PUBLISH/"
WEB_SRC="$EXTRACT_DIR/sabormercado-publish/wwwroot/wwwroot"
if [ ! -d "$WEB_SRC" ]; then
  WEB_SRC="$EXTRACT_DIR/sabormercado-publish/wwwroot"
fi
rsync -a --delete "$WEB_SRC/" "$WEB_ROOT/"
rm -rf "$EXTRACT_DIR"

echo "==> Production config (API)"
cat > "$API_PUBLISH/appsettings.Production.json" <<JSON
{
  "Database": { "Provider": "PostgreSQL" },
  "ConnectionStrings": {
    "Identity": "${CONN}",
    "SharedCatalog": "${CONN}",
    "Rewards": "${CONN}",
    "Recognition": "${CONN}"
  },
  "Jwt": {
    "Issuer": "sabor-mercado",
    "Audience": "sabor-mercado-pwa",
    "SigningKey": "${JWT_KEY}",
    "AccessTokenMinutes": 60,
    "RefreshTokenDays": 30
  },
  "Cors": {
    "Origins": [ "${PUBLIC_BASE}", "https://rpjtechgroup.ddns.net", "http://rpjtechgroup.ddns.net" ]
  },
  "Email": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "UseStartTls": true,
    "UserName": "${SMTP_USER}",
    "Password": "${SMTP_PASSWORD}",
    "FromAddress": "${SMTP_USER}",
    "FromName": "Sabor Mercado",
    "SupportToAddress": "${SMTP_USER}"
  },
  "GoogleAuth": {
    "ClientId": "${GOOGLE_CLIENT_ID}"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
JSON

echo "==> PWA base href + ApiBaseUrl"
sed -i 's|<base href="/" />|<base href="/mercado/" />|' "$WEB_ROOT/index.html"
cat > "$WEB_ROOT/appsettings.json" <<JSON
{
  "GeminiModel": "gemini-2.5-flash",
  "GeminiModelFallbacks": "gemini-2.5-flash-lite,gemini-3.1-flash-lite,gemini-3-flash,gemini-3.5-flash",
  "ApiBaseUrl": "${PUBLIC_BASE}",
  "GoogleClientId": "${GOOGLE_CLIENT_ID}"
}
JSON

# nginx (www-data) precisa atravessar /home/ubuntu
chmod o+x /home/ubuntu
chmod -R o+rX "$WEB_ROOT"

echo "==> systemd"
sudo tee /etc/systemd/system/sabormercado-api.service >/dev/null <<SERVICE
[Unit]
Description=Sabor Mercado API
After=network.target postgresql.service
Wants=postgresql.service

[Service]
WorkingDirectory=${API_PUBLISH}
ExecStart=/usr/bin/dotnet ${API_PUBLISH}/SaborMercado.Api.dll
Restart=always
RestartSec=5
User=ubuntu
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://127.0.0.1:${API_PORT}
Environment=DOTNET_GCHeapHardLimit=0x12C00000
MemoryMax=350M

[Install]
WantedBy=multi-user.target
SERVICE

sudo systemctl daemon-reload
sudo systemctl enable sabormercado-api
sudo systemctl restart sabormercado-api

echo "==> nginx (/mercado)"
NGINX_CONF="$(dirname "$0")/nginx-rpjtechgroup.conf"
if [ -f "$NGINX_CONF" ]; then
  sudo cp "$NGINX_CONF" /etc/nginx/sites-available/rpjtechgroup
  sudo ln -sf /etc/nginx/sites-available/rpjtechgroup /etc/nginx/sites-enabled/rpjtechgroup
fi
sudo nginx -t
sudo systemctl reload nginx

echo "==> Wait for API (healthz)"
API_READY=0
for attempt in $(seq 1 30); do
  if curl -fsS "http://127.0.0.1:${API_PORT}/healthz" >/dev/null 2>&1; then
    echo "API healthy (attempt ${attempt})"
    API_READY=1
    break
  fi
  sleep 2
done
if [ "$API_READY" -ne 1 ]; then
  echo "ERROR: API did not respond on 127.0.0.1:${API_PORT}/healthz" >&2
  sudo systemctl status sabormercado-api --no-pager || true
  sudo journalctl -u sabormercado-api -n 80 --no-pager || true
  exit 1
fi

curl -fsSI "http://127.0.0.1/mercado/" | head -n 1
echo "DEPLOY_OK ${PUBLIC_BASE}/"
echo "APP_ROOT=${APP_ROOT}"
