# Implementation Plan: Confiança Comunitária e Gamificação (006)

**Branch**: `006-community-trust` | **Date**: 2026-06-12

## Summary

Votação em observações compartilhadas, reputação por pseudônimo, denúncias com
restrição automática e conquistas na conta — moderação autônoma da comunidade.

Regras canônicas: [`docs/business/community-trust.md`](../../docs/business/community-trust.md)

## Stack

| Camada | Tecnologia |
|--------|------------|
| API | ASP.NET Core Minimal APIs, JWT Bearer |
| Módulos | SharedCatalog (votos, denúncias, trust), Rewards (conquistas) |
| Persistência | PostgreSQL / SQLite via `DatabaseBootstrap` |
| PWA | Blazor WASM — detalhe colaborativo + conquistas em `/conta` |

## Endpoints novos

- `GET /api/v1/shared-products/{id}/observations` — listagem com trust e votos (premium)
- `POST /api/v1/price-observations/{id}/vote` — upvote/downvote
- `DELETE /api/v1/price-observations/{id}/vote` — remover voto
- `POST /api/v1/contributor-reports` — denúncia (`202 Accepted`)
- `GET /api/v1/achievements` — conquistas do usuário

## Serviços

- `TrustScoreCalculator` — fórmula determinística + ocultação (`netScore ≤ −3`)
- `ContributorTrustService` — agregado por pseudônimo
- `ObservationVoteService` — votos + hooks de trust/achievements
- `ContributorReportService` — denúncias + restrição (≥3 denunciantes / 7 dias)
- `SharedObservationQueryService` — listagem premium
- `AchievementService` — desbloqueio automático (6 badges)

## PWA

- `/catalogo-colaborativo/{productId}` — observações, votos, denúncia, badge de confiança
- `/conta` — seção de conquistas
- `ProductDetail` — mensagem `CONTRIBUTOR_RESTRICTED` ao compartilhar

## Fora do escopo

- Moderação humana / painel admin
- Créditos bônus por conquista (fase 2)
- Votação offline

## Comandos

```powershell
dotnet run --project src/SaborMercado.Api    # :5280
dotnet run --project src/SaborMercado.Web    # :5052
dotnet test
```
