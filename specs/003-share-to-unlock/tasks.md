# Tasks: Share-to-Unlock (003)

## Fase 1 — Core (concluída)

- [X] T001 Módulos Identity, SharedCatalog, Rewards + Api JWT
- [X] T002 Endpoints auth, price-observations, credits, unlocks
- [X] T003 PWA /conta, compartilhar
- [X] T004 Testes integração share-to-unlock

## Fase 2 — Hardening (concluída)

- [X] T005 PseudonymId em PriceObservation (não userId)
- [X] T006 Refresh token persistido + POST /auth/refresh
- [X] T007 DELETE /auth/me (exclusão de conta)
- [X] T008 Fila IndexedDB pendingShares + flush offline
- [X] T009 GET /shared-products + UI /catalogo-colaborativo
- [X] T010 Desbloqueios "Em breve" para features sem implementação
- [X] T011 spec.md, ADR-0006 SQLite bootstrap

## Fase 3 — Produção e premium (concluída)

- [X] T012 PostgreSQL bootstrap (`DatabaseBootstrap`) + EF Migrations (Identity, SharedCatalog, Rewards)
- [X] T013 Anti-fraude z-score em `ContributionService` (`PRICE_OUTLIER`)
- [X] T014 Idempotency-Key em `POST /price-observations` + flush offline
- [X] T015 Premium: `GET /shared-products/{id}/markets`, `/estatisticas`, `/listas-inteligentes`, `/comparar-mercados/{id}`

## Fase 4 — Conformidade e produção (concluída)

- [X] T016 Atualizar `spec.md` e `api-standards.md` (Fase 3 + novos endpoints)
- [X] T017 `GET /readyz` com health checks EF (Identity, SharedCatalog, Rewards)
- [X] T018 Idempotency-Key no share online + conflito `409 IDEMPOTENCY_CONFLICT`
- [X] T019 Migrations PostgreSQL (`uuid`, design-time factories Npgsql)
- [X] T020 Testes: `PRICE_OUTLIER`, `PREMIUM_REQUIRED`, `MigrateAsync` (Testcontainers)
- [X] T021 Limite de leitura em z-score (100) e comparação de mercados (500)
