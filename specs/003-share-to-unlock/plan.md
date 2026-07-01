# Implementation Plan: Share-to-Unlock (003)

**Branch**: `003-share-to-unlock` | **Date**: 2026-06-11

## Summary

Conta leve (JWT), compartilhamento opt-in de observações de preço anonimizadas,
créditos determinísticos e desbloqueio de funcionalidades premium no PWA.

Regras canônicas: `docs/business/share-to-unlock.md`

## Stack

| Camada | Tecnologia |
|--------|------------|
| API | ASP.NET Core Minimal APIs, JWT Bearer |
| Módulos | Identity, SharedCatalog, Rewards |
| Persistência dev | SQLite (um arquivo por módulo, `EnsureCreated`) |
| Persistência prod | PostgreSQL (`Database:Provider`, `MigrateAsync`) |
| PWA | Blazor WASM, token em localStorage, `ApiBaseUrl` em appsettings |

## Endpoints

- `POST /api/v1/auth/register`, `/login`, `GET /api/v1/auth/me`
- `POST /api/v1/price-observations` (auth)
- `GET /api/v1/credits`, `POST /api/v1/unlocks` (auth)

## PWA

- `/conta` — login, créditos, desbloqueios, exclusão de conta, fila offline
- `/catalogo-colaborativo` — busca premium (requer desbloqueio)
- `/configuracoes` — link para conta + Gemini key
- `ProductDetail` — compartilhar preço (opt-in, enfileira se offline)

## Fase 2 (hardening)

- `contributorPseudonymId` no SharedCatalog
- Refresh token + `POST /auth/refresh`, `DELETE /auth/me`
- IndexedDB `pendingShares` (schema v2)
- `GET /shared-products` com checagem premium
- ADR-0006 SQLite bootstrap

## Fase 3 (produção e premium)

- `SaborMercado.Infrastructure` — `DatabaseBootstrap` (Sqlite vs PostgreSQL)
- EF Migrations por módulo; `appsettings.Production.json` com `Database:Provider = PostgreSQL`
- Anti-fraude z-score (`PriceOutlierValidator`, `PRICE_OUTLIER`)
- `Idempotency-Key` em contribuições + fila offline
- Premium: `GET /shared-products/{id}/markets`, `/estatisticas`, `/listas-inteligentes`, `/comparar-mercados/{id}`
- `dotnet-ef` como ferramenta local (`.config/dotnet-tools.json`)

## Fase 4 (conformidade e produção)

- `spec.md` e `api-standards.md` sincronizados com implementação
- `GET /readyz` — health checks EF (Identity, SharedCatalog, Rewards)
- Idempotency-Key no share online; conflito → `409 IDEMPOTENCY_CONFLICT`
- Migrations Npgsql (`uuid`, schemas por módulo) + design-time factories
- Testes: `PRICE_OUTLIER`, `PREMIUM_REQUIRED`, `MigrateAsync` (Testcontainers)
- Limite de leitura: z-score (100 amostras), comparação de mercados (500)

## Comandos

```powershell
dotnet run --project src/SaborMercado.Api    # :5280
dotnet run --project src/SaborMercado.Web    # :5052
dotnet test
```

## Estrutura

```
src/SaborMercado.Modules.Identity/
src/SaborMercado.Modules.SharedCatalog/
src/SaborMercado.Modules.Rewards/
src/SaborMercado.Web/Features/Account/
src/SaborMercado.Web/Contracts/
```
