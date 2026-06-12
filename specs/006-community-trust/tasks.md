# Tasks: Confiança Comunitária e Gamificação (006)

## Fase 1 — Persistência e API

- [X] T001 EF migrations: `observation_votes`, `contributor_trust`, `contributor_reports`, colunas de voto em `shared_price_observations`, `rewards_user_achievements`
- [X] T002 Registrar `MapCommunityEndpoints` + `MapSharedObservationEndpoints` em `SharedCatalogModule`
- [X] T003 `GET /api/v1/achievements` em `RewardsEndpoints`
- [X] T004 Wire `ContributionService`: `EnsureCanContribute`, `IncrementAcceptedContribution`, `EvaluateAfterContribution`
- [X] T005 Mapear `CommunityException` em `ContributionEndpoints`
- [X] T006 Corrigir timing de restrição por denúncia (`SaveChanges` antes de contar denunciantes)

## Fase 2 — Testes

- [X] T007 Testes integração: voto, auto-voto, ocultação, denúncia, restrição, conquistas

## Fase 3 — PWA

- [X] T008 `CommunityService` + contratos Web
- [X] T009 `/catalogo-colaborativo/{productId}` — observações, votos, denúncia
- [X] T010 Seção conquistas em `/conta` + CSS community
- [X] T011 Link no catálogo colaborativo; `CONTRIBUTOR_RESTRICTED` no share

## Fase 4 — Documentação

- [X] T012 Atualizar `api-standards.md` e `domain-model.md`
