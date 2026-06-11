# Infraestrutura do MVP — VM OCI (1GB RAM + 1GB swap)

> Limites rígidos (Constitution, Restrições Técnicas). Qualquer mudança no
> backend que aumente o consumo de memória deve ser validada contra o
> orçamento abaixo.

## Topologia

Uma única VM OCI (shape free tier, 1 OCPU, 1GB RAM, 1GB swap) rodando:

| Processo            | Papel                                            |
|---------------------|--------------------------------------------------|
| Caddy               | TLS automático, servir o PWA (estáticos), proxy `/api` |
| SaborMercado.Api    | Monólito modular ASP.NET Core                    |
| PostgreSQL 16       | Banco único com 4 schemas (1 por módulo)         |

O PWA Blazor WASM executa no dispositivo do usuário — **custo zero de RAM no
servidor** além de servir arquivos estáticos (com `Accept-Encoding: br`,
Caddy serve os `.br` pré-comprimidos do publish).

## Orçamento de memória (limite rígido)

| Componente              | Limite (RSS) | Configuração que garante                         |
|-------------------------|--------------|--------------------------------------------------|
| SO + sshd + journald    | ~180 MB      | Ubuntu Minimal / Oracle Linux minimal            |
| Caddy                   | 50 MB        | `MemoryMax=50M` (systemd)                        |
| SaborMercado.Api        | 350 MB       | Workstation GC + `GCHeapHardLimit` (abaixo)      |
| PostgreSQL 16           | 250 MB       | tuning low-memory (abaixo)                       |
| Folga / picos           | ~190 MB      | swap de 1GB cobre rajadas, não uso contínuo      |

### Configuração da API (.csproj / runtimeconfig)

```xml
<PropertyGroup>
  <ServerGarbageCollection>false</ServerGarbageCollection>
  <ConcurrentGarbageCollection>false</ConcurrentGarbageCollection>
  <InvariantGlobalization>false</InvariantGlobalization>
  <TieredPgo>true</TieredPgo>
</PropertyGroup>
```

Variáveis de ambiente:

```bash
DOTNET_GCHeapHardLimit=0x12C00000   # 300 MB
ASPNETCORE_URLS=http://127.0.0.1:5000
```

Diretrizes de código que sustentam o limite:
- `IMemoryCache` com `SizeLimit` configurado (sem Redis na Fase 1).
- Streaming de upload de imagem (sem buffer completo em memória;
  limite de payload 4 MB).
- Sem bibliotecas de processamento de imagem no servidor — a imagem segue
  direto para o Gemini; redimensionamento/compressão acontece **no cliente**
  antes do upload (canvas → JPEG ~1024px).
- `AddDbContextPool` com pool pequeno (`poolSize: 32`).

### Tuning do PostgreSQL (postgresql.conf)

```conf
shared_buffers = 128MB
work_mem = 4MB
maintenance_work_mem = 32MB
effective_cache_size = 256MB
max_connections = 40
wal_level = replica          # já preparado para réplica na Fase 2
```

## Deploy

Processos nativos gerenciados por **systemd** (não usar Docker na Fase 1: o
daemon consome ~80–100 MB que não cabem no orçamento). Unidades:

- `caddy.service` (pacote oficial) — `MemoryMax=50M`
- `sabormercado-api.service` — `MemoryMax=350M`, `Restart=always`
- `postgresql.service` (pacote oficial)

Publicação da API: `dotnet publish -c Release` self-contained ou
framework-dependent (preferir framework-dependent + runtime instalado, menor
disco). PWA: `dotnet publish` do projeto Web; Caddy serve `wwwroot`.

Pipeline (GitHub Actions): build + testes → `rsync` dos artefatos →
`systemctl restart sabormercado-api`. Migrações EF Core aplicadas via bundle
(`dotnet ef migrations bundle`) antes do restart.

## Capacidade esperada da Fase 1

Com o fluxo principal rodando no cliente, o servidor atende apenas OCR,
compartilhamentos e login:

- ~40 req/s sustentadas na API (rotas leves) — suficiente para milhares de
  usuários ativos, pois cada usuário gera poucas chamadas por sessão de compra.
- Gargalo real: quota do Gemini free tier (ver `ocr-integration.md`) — não a VM.

## Gatilhos para iniciar a Fase 2

Migrar quando **qualquer** um ocorrer (medido por 7 dias consecutivos):
1. RSS da API > 300 MB sustentado ou OOM-kills.
2. p95 das rotas `/api/*` > 800 ms.
3. PostgreSQL > 70% do tempo de CPU da VM.
4. > 2.000 usuários ativos/dia ou > 10.000 cadastrados.

Plano completo: [`scale-migration-plan.md`](scale-migration-plan.md).
