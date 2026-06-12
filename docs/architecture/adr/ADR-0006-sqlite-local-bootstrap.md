# ADR-0006 — SQLite para bootstrap local e testes (emenda transitória à ADR-0001)

**Status:** Aceito — 2026-06-11

## Contexto

A ADR-0001 define PostgreSQL como banco primário do servidor no MVP. Durante o
desenvolvimento local e testes de integração (`WebApplicationFactory`), exigir
PostgreSQL aumenta fricção e complexidade de CI.

Os módulos Identity, SharedCatalog e Rewards usam `EnsureCreated` com SQLite
separado por módulo (`sabormercado-identity.db`, `-shared.db`, `-rewards.db`).

## Decisão

1. **Desenvolvimento e testes automatizados** podem usar SQLite por módulo.
2. **Produção MVP** continua planejada para PostgreSQL (ADR-0001); connection
   strings por módulo já permitem migração independente.
3. `EnsureCreated` é aceitável apenas em dev/teste; produção usará EF Migrations
   quando PostgreSQL for ligado.

## Consequências

- (+) Testes de integração rápidos sem container Postgres.
- (+) Um arquivo SQLite por módulo evita o bug de `EnsureCreated` em banco compartilhado.
- (−) Divergência temporária de provider (SQLite vs Npgsql) até migração de deploy.
- (−) Algumas queries usam avaliação em memória por limitações do SQLite
  (`DateTimeOffset` em ORDER BY).

## Alternativas consideradas

- **PostgreSQL em Docker no dev:** alinhado à ADR-0001, mas mais pesado para CI local.
- **Um único SQLite compartilhado:** rejeitado — `EnsureCreated` não adiciona tabelas de outros contextos.
