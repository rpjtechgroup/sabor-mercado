#!/usr/bin/env bash
set -euo pipefail

RULES=/etc/iptables/rules.v4
cp "$RULES" "${RULES}.bak.$(date +%Y%m%d%H%M%S)"

if ! grep -q 'INPUT -p tcp -m state --state NEW -m tcp --dport 80 -j ACCEPT' "$RULES"; then
  python3 <<'PY'
from pathlib import Path
p = Path("/etc/iptables/rules.v4")
lines = p.read_text().splitlines()
out = []
for line in lines:
    out.append(line)
    if "--dport 22 -j ACCEPT" in line and "INPUT" in line:
        out.append("-A INPUT -p tcp -m state --state NEW -m tcp --dport 80 -j ACCEPT")
        out.append("-A INPUT -p tcp -m state --state NEW -m tcp --dport 443 -j ACCEPT")
p.write_text("\n".join(out) + "\n")
PY
fi

iptables-restore < "$RULES"
echo "OK: ports 80/443 allowed in INPUT"
iptables -S INPUT | grep -E 'dport (80|443|22)'
