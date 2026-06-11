# ADR-0001 — PostgreSQL como banco primário (JSONB no MVP)

**Status:** Aceito — 2026-06-11

## Contexto

O backend precisa de: dados transacionais com integridade forte (ledger de
créditos, identidade), atributos flexíveis de produto no catálogo colaborativo
e operação dentro de uma VM OCI com 1GB de RAM no MVP. A meta de escala é
10.000+ usuários. Stack obrigatória: C#/.NET com EF Core.

## Decisão

PostgreSQL 16 é o banco primário de todos os módulos do servidor na Fase 1:
- Suporte de primeira classe no EF Core via Npgsql.
- **JSONB** cobre os atributos variáveis de produto sem precisar de um banco
  de documentos na Fase 1.
- Tuning low-memory comprovado (~250 MB) cabe no orçamento da VM
  (`docs/architecture/mvp-infrastructure.md`).
- Gratuito, sem licenciamento, disponível gerenciado em qualquer nuvem
  (caminho da Fase 2 sem troca de tecnologia).

Cada módulo usa um **schema próprio** (`identity`, `rewards`,
`shared_catalog`, `recognition`) com `DbContext` separado.

## Consequências

- (+) Um único processo de banco no MVP; backup/restore simples.
- (+) Migração para PostgreSQL gerenciado é transparente (connection string).
- (−) Consultas de documento em JSONB são menos ergonômicas que em MongoDB;
  aceitável até o volume do catálogo justificar o Passo 3 do plano de escala.

## Alternativas consideradas

- **SQL Server:** integração .NET excelente, mas Express limita 10 GB/1410 MB
  de buffer e as edições pagas não cabem no modelo gratuito do produto.
- **MySQL/MariaDB:** suporte EF Core e recursos JSON inferiores ao Postgres.
- **SQLite:** caberia na RAM, mas escrita concorrente limitada e migração
  futura mais custosa que começar direto no Postgres.
