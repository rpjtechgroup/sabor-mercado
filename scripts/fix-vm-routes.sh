#!/usr/bin/env bash
# Corrige paths na VM: move imobiliária para /home/ubuntu, nginx e rebuild SPA.
set -euo pipefail

DOMAIN="${DOMAIN:-rpjtechgroup.ddns.net}"
PUBLIC_MERCADO="${PUBLIC_MERCADO:-https://${DOMAIN}/mercado}"
PUBLIC_IMOB="${PUBLIC_IMOB:-https://${DOMAIN}/imobiliaria}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "==> Mover imobiliária para /home/ubuntu/imobiliaria"
if [ -d /opt/imobiliaria ] && [ ! -d /home/ubuntu/imobiliaria/api ]; then
  sudo mkdir -p /home/ubuntu/imobiliaria
  sudo rsync -a /opt/imobiliaria/ /home/ubuntu/imobiliaria/
  sudo chown -R ubuntu:ubuntu /home/ubuntu/imobiliaria
fi

if [ -d /var/www/imobiliaria ] && [ ! -d /home/ubuntu/imobiliaria/wwwroot ]; then
  sudo mkdir -p /home/ubuntu/imobiliaria
  sudo rsync -a /var/www/imobiliaria/ /home/ubuntu/imobiliaria/wwwroot/
  sudo chown -R ubuntu:ubuntu /home/ubuntu/imobiliaria/wwwroot
elif [ -d /var/www/imobiliaria ] && [ -d /home/ubuntu/imobiliaria/wwwroot ]; then
  sudo rsync -a /var/www/imobiliaria/ /home/ubuntu/imobiliaria/wwwroot/
  sudo chown -R ubuntu:ubuntu /home/ubuntu/imobiliaria/wwwroot
fi

echo "==> .NET runtimes (8 = mercado, 10 = imobiliária net9 com roll-forward)"
sudo DEBIAN_FRONTEND=noninteractive apt-get install -y aspnetcore-runtime-8.0 aspnetcore-runtime-10.0

echo "==> systemd imobiliaria-api"
sudo tee /etc/systemd/system/imobiliaria-api.service >/dev/null <<'UNIT'
[Unit]
Description=Imobiliaria API
After=network.target mysql.service

[Service]
WorkingDirectory=/home/ubuntu/imobiliaria/api
ExecStart=/usr/bin/dotnet /home/ubuntu/imobiliaria/api/Agenda.Api.dll
Restart=always
RestartSec=5
KillSignal=SIGINT
SyslogIdentifier=imobiliaria-api
User=ubuntu
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://127.0.0.1:5097
Environment=DOTNET_ROLL_FORWARD=LatestMajor

[Install]
WantedBy=multi-user.target
UNIT
sudo systemctl daemon-reload
sudo systemctl restart imobiliaria-api

echo "==> Rebuild imobiliária (base /imobiliaria/)"
IMOB_FRONT="/home/ubuntu/imobiliaria/src/Front"
if [ -d "$IMOB_FRONT" ]; then
  if ! grep -q 'base: "/imobiliaria/"' "$IMOB_FRONT/vite.config.ts" 2>/dev/null; then
    sed -i 's/export default defineConfig(({ mode }) => ({/export default defineConfig(({ mode }) => ({\n  base: "\/imobiliaria\/",/' "$IMOB_FRONT/vite.config.ts"
  fi
  if ! grep -q 'basename={import.meta.env.BASE_URL}' "$IMOB_FRONT/src/App.tsx" 2>/dev/null; then
    sed -i 's/<BrowserRouter>/<BrowserRouter basename={import.meta.env.BASE_URL}>/' "$IMOB_FRONT/src/App.tsx"
  fi
  printf 'VITE_DOTNET_API_URL=%s\n' "$PUBLIC_IMOB" > "$IMOB_FRONT/.env.production"
  cd "$IMOB_FRONT"
  npm install --legacy-peer-deps
  npm run build
  mkdir -p /home/ubuntu/imobiliaria/wwwroot
  rsync -a --delete dist/ /home/ubuntu/imobiliaria/wwwroot/
fi

echo "==> Permissões nginx"
chmod o+x /home/ubuntu
chmod -R o+rX /home/ubuntu/sabormercado/wwwroot
chmod -R o+rX /home/ubuntu/imobiliaria/wwwroot

echo "==> nginx"
sudo cp "${SCRIPT_DIR}/nginx-rpjtechgroup.conf" /etc/nginx/sites-available/rpjtechgroup
sudo ln -sf /etc/nginx/sites-available/rpjtechgroup /etc/nginx/sites-enabled/rpjtechgroup
sudo rm -f /etc/nginx/sites-enabled/default /etc/nginx/sites-enabled/imobiliaria
sudo nginx -t
sudo systemctl reload nginx

echo "==> Sabor Mercado appsettings"
MERCADO_WWW="/home/ubuntu/sabormercado/wwwroot"
if [ -f "$MERCADO_WWW/index.html" ]; then
  sed -i 's|<base href="/" />|<base href="/mercado/" />|' "$MERCADO_WWW/index.html"
  cat > "$MERCADO_WWW/appsettings.json" <<JSON
{
  "GeminiModel": "gemini-2.0-flash",
  "ApiBaseUrl": "${PUBLIC_MERCADO}"
}
JSON
fi

echo "FIX_OK"
