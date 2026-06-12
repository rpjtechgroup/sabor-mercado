#!/usr/bin/env bash
# Configura nginx + HTTPS + rebuild imobiliária em /imobiliaria/
set -euo pipefail

DOMAIN="${DOMAIN:-rpjtechgroup.ddns.net}"
PUBLIC_MERCADO="${PUBLIC_MERCADO:-https://${DOMAIN}/mercado}"
PUBLIC_IMOB="${PUBLIC_IMOB:-https://${DOMAIN}/imobiliaria}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
NGINX_SITE="/etc/nginx/sites-available/rpjtechgroup"

echo "==> nginx site (${DOMAIN})"
sudo cp "${SCRIPT_DIR}/nginx-rpjtechgroup.conf" "$NGINX_SITE"
sudo ln -sf "$NGINX_SITE" /etc/nginx/sites-enabled/rpjtechgroup
sudo rm -f /etc/nginx/sites-enabled/imobiliaria
sudo rm -f /etc/nginx/snippets/sabormercado.conf
sudo nginx -t
sudo systemctl reload nginx

echo "==> Imobiliária: base /imobiliaria/ + API ${PUBLIC_IMOB}"
if [ -f "${SCRIPT_DIR}/fix-vm-routes.sh" ]; then
  bash "${SCRIPT_DIR}/fix-vm-routes.sh"
else
  IMOB_FRONT="/home/ubuntu/imobiliaria/src/Front"
  if [ -d "$IMOB_FRONT" ]; then
    if ! grep -q 'base: "/imobiliaria/"' "$IMOB_FRONT/vite.config.ts"; then
      sed -i 's/export default defineConfig(({ mode }) => ({/export default defineConfig(({ mode }) => ({\n  base: "\/imobiliaria\/",/' "$IMOB_FRONT/vite.config.ts"
    fi
    if ! grep -q 'basename={import.meta.env.BASE_URL}' "$IMOB_FRONT/src/App.tsx"; then
      sed -i 's/<BrowserRouter>/<BrowserRouter basename={import.meta.env.BASE_URL}>/' "$IMOB_FRONT/src/App.tsx"
    fi
    printf 'VITE_DOTNET_API_URL=%s\n' "$PUBLIC_IMOB" > "$IMOB_FRONT/.env.production"
    cd "$IMOB_FRONT"
    npm install --legacy-peer-deps
    npm run build
    mkdir -p /home/ubuntu/imobiliaria/wwwroot
    rsync -a --delete dist/ /home/ubuntu/imobiliaria/wwwroot/
  fi
fi

echo "==> Sabor Mercado: ApiBaseUrl ${PUBLIC_MERCADO}"
MERCADO_WWW="/home/ubuntu/sabormercado/wwwroot"
if [ -f "$MERCADO_WWW/index.html" ]; then
  sed -i 's|<base href="/" />|<base href="/mercado/" />|; s|<base href="/mercado/" />|<base href="/mercado/" />|' "$MERCADO_WWW/index.html"
  cat > "$MERCADO_WWW/appsettings.json" <<JSON
{
  "GeminiModel": "gemini-2.0-flash",
  "ApiBaseUrl": "${PUBLIC_MERCADO}"
}
JSON
  chmod o+x /home/ubuntu
  chmod -R o+rX "$MERCADO_WWW"
fi

API_DIR="/home/ubuntu/sabormercado/api"
if [ -f "$API_DIR/appsettings.Production.json" ] && [ -f /home/ubuntu/deploy/sabormercado.secrets ]; then
  set -a
  # shellcheck disable=SC1091
  source /home/ubuntu/deploy/sabormercado.secrets
  set +a
  CONN=$(python3 -c "import json; print(json.load(open('${API_DIR}/appsettings.Production.json'))['ConnectionStrings']['Identity'])")
  cat > "$API_DIR/appsettings.Production.json" <<JSON
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
    "SigningKey": "${SABOR_JWT_KEY}",
    "AccessTokenMinutes": 60,
    "RefreshTokenDays": 30
  },
  "Cors": {
    "Origins": [ "${PUBLIC_MERCADO}", "https://${DOMAIN}", "http://${DOMAIN}" ]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
JSON
  sudo systemctl restart sabormercado-api
fi

echo "==> HTTPS (Let's Encrypt)"
if ! command -v certbot >/dev/null 2>&1; then
  sudo apt-get update -y
  sudo DEBIAN_FRONTEND=noninteractive apt-get install -y certbot python3-certbot-nginx
fi
if [ ! -d "/etc/letsencrypt/live/${DOMAIN}" ]; then
  sudo certbot --nginx -d "$DOMAIN" --non-interactive --agree-tos --register-unsafely-without-email --redirect
else
  sudo certbot renew --quiet || true
  sudo systemctl reload nginx
fi

echo "SETUP_OK"
echo "  Mercado:    ${PUBLIC_MERCADO}/"
echo "  Imobiliária: ${PUBLIC_IMOB}/"
