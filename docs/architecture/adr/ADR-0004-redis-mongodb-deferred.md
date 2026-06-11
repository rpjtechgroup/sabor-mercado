# ADR-0004 — Redis e MongoDB adiados para a Fase 2

**Status:** Aceito — 2026-06-11

## Contexto

A análise de stack recomendou PostgreSQL + Redis (cache/rate-limit) + MongoDB
(catálogo colaborativo) para a escala de 10.000+ usuários. Porém o MVP roda
em uma VM com 1GB de RAM: Redis (~50 MB) e principalmente MongoDB (~300 MB+)
não cabem no orçamento de memória junto com API + PostgreSQL.

## Decisão

Na Fase 1, Redis e MongoDB **não são implantados**. Suas responsabilidades
são cobertas por equivalentes in-process, atrás de abstrações que permitem a
troca por configuração na Fase 2:

| Responsabilidade        | Fase 1 (in-process)           | Fase 2 (distribuído)        |
|-------------------------|-------------------------------|-----------------------------|
| Cache                   | `IMemoryCache` c/ `SizeLimit` | Redis via `IDistributedCache` |
| Rate-limit              | `System.Threading.RateLimiting` | Redis rate-limiter        |
| Catálogo colaborativo   | PostgreSQL JSONB (repositório atrás de interface) | MongoDB (`MongoSharedCatalogRepository`) |

Gatilhos objetivos de migração e passos em
`docs/architecture/scale-migration-plan.md` (Passos 2 e 3).

## Consequências

- (+) MVP cabe na VM; zero infra adicional para operar.
- (+) Código de negócio não muda na migração (somente implementações de
  infraestrutura e configuração).
- (−) Cache e rate-limit não são compartilhados entre instâncias — aceitável
  porque a Fase 1 tem instância única por definição.

## Alternativas consideradas

- **Redis/Mongo desde o início (na mesma VM):** estouraria o orçamento de
  memória e degradaria tudo via swap.
- **Tiers gratuitos externos (Atlas M0, Upstash):** latência e limites de
  conexão imprevisíveis no caminho crítico do MVP; adicionam operação sem
  necessidade no volume inicial. Reavaliar no Passo 2/3 da migração.
