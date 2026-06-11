# ADR-0003 — Monólito modular antes de serviços

**Status:** Aceito — 2026-06-11

## Contexto

A meta de longo prazo é uma arquitetura distribuída para 10.000+ usuários,
mas o MVP precisa caber em uma VM OCI de 1GB de RAM. Microsserviços desde o
início são inviáveis nesse hardware e desnecessários nesse volume.

## Decisão

Um único processo ASP.NET Core (`SaborMercado.Api`) composto por módulos que
seguem exatamente os bounded contexts de `docs/domain/domain-model.md`
(Recognition, SharedCatalog, Rewards, Identity), com regras rígidas:

1. Um projeto .NET por módulo; módulos não se referenciam.
2. Comunicação entre módulos apenas por contratos em `SaborMercado.Shared`
   ou eventos in-process.
3. Um `DbContext` e um schema PostgreSQL por módulo.
4. Configuração/DI de cada módulo encapsulada em
   `AddXxxModule(IServiceCollection)`.

Assim, "extrair um serviço" na Fase 2+ é mover o projeto do módulo para outro
host e trocar o transporte do contrato — sem reescrita de regra de negócio.

## Consequências

- (+) Cabe no orçamento de memória do MVP (um processo, ~350 MB).
- (+) Deploy e observabilidade simples na Fase 1.
- (+) Caminho de extração documentado em `scale-migration-plan.md`.
- (−) Disciplina de fronteiras depende de review (reforçada por rules do
  Cursor e checagem de referências entre projetos no build).

## Alternativas consideradas

- **Microsserviços desde o início:** overhead de RAM e operação incompatível
  com a VM de 1GB; complexidade injustificada para o volume inicial.
- **Monólito sem módulos:** inviabiliza a migração planejada sem reescrita.
