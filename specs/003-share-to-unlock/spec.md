# Feature Specification: Share-to-Unlock

**Feature Branch**: `003-share-to-unlock`  
**Created**: 2026-06-11  
**Status**: Implemented (fases 1–4)

**Input**: Conta leve, compartilhamento opt-in de preços, créditos e desbloqueio premium.

Regras canônicas: `docs/business/share-to-unlock.md`

## User Scenarios

### US1 — Conta e créditos (P1)

Usuário cria conta, compartilha preço anonimizado e vê créditos acumulados.

**Acceptance**:
- Given usuário logado, When compartilha preço válido, Then recebe créditos conforme tabela canônica.

### US2 — Compartilhamento offline (P2)

Sem rede, compartilhamento é enfileirado e enviado ao reconectar.

**Acceptance**:
- Given offline e logado, When compartilha, Then item vai para `pendingShares` no IndexedDB.
- Given online novamente, When abre conta ou tenta enviar, Then fila é processada com `Idempotency-Key` estável por item.

### US3 — Catálogo colaborativo premium (P2)

Com desbloqueio ativo, usuário busca preços do catálogo colaborativo.

**Acceptance**:
- Given `collaborative-price-history` ativo, When busca por nome, Then vê resultados com último preço.
- Given sem desbloqueio, When busca, Then API retorna 403 `PREMIUM_REQUIRED`.

### US4 — Anti-fraude e idempotência (P2)

Contribuições duplicadas ou com preço implausível são rejeitadas sem crédito.

**Acceptance**:
- Given produto com ≥5 observações históricas, When envia preço com |z|>3, Then 422 `PRICE_OUTLIER`.
- Given mesma `Idempotency-Key` e payload idêntico, When reenvia, Then mesma resposta sem duplicar crédito.
- Given mesma `Idempotency-Key` e payload diferente, When reenvia, Then 409 `IDEMPOTENCY_CONFLICT`.

### US5 — Premium completo (P3)

Usuário com desbloqueios ativos acessa comparação de mercados, estatísticas e listas inteligentes.

**Acceptance**:
- Given `market-comparison` ativo, When consulta `/shared-products/{id}/markets`, Then vê preços por mercado.
- Given `advanced-stats` ou `smart-lists` desbloqueados, When abre páginas premium, Then vê dados locais agregados.

## Functional Requirements

- **FR-001**: Auth JWT (register/login/refresh/delete) no módulo Identity.
- **FR-002**: Observações anonimizadas com `contributorPseudonymId` (não e-mail).
- **FR-003**: Créditos determinísticos no Rewards (ledger append-only).
- **FR-004**: Desbloqueios premium conforme `share-to-unlock.md`.
- **FR-005**: PWA `/conta`, compartilhar em produto.
- **FR-006**: Fila `pendingShares` para envio offline.
- **FR-007**: Fluxo principal (F1–F5) permanece gratuito e offline.
- **FR-008**: Anti-fraude z-score e `Idempotency-Key` em contribuições.
- **FR-009**: `GET /readyz` com checagem de banco nos módulos de conta/share/rewards.
- **FR-010**: Migrations EF para PostgreSQL em produção (`Database:Provider`).

## Success Criteria

- **SC-001**: Fluxo register → share → credits passa em teste de integração.
- **SC-002**: Carrinho funciona com API offline.
- **SC-003**: Pseudônimo usado no SharedCatalog, não `userId` da conta.
- **SC-004**: `PRICE_OUTLIER` e `PREMIUM_REQUIRED` cobertos por testes de integração.
- **SC-005**: `MigrateAsync` validado contra PostgreSQL (Testcontainers).

## Assumptions

- SQLite em dev/teste (`EnsureCreated`, ADR-0006); PostgreSQL em produção (`MigrateAsync`).
- Migrations geradas com provider Npgsql via design-time factories.
- Premium local (stats/listas) lê IndexedDB; premium colaborativo exige API + desbloqueio.
