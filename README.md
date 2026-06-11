# Sabor Mercado

PWA gratuito e offline-first para controle de compras de mercado: adicione
produtos ao carrinho virtual **fotografando a etiqueta da prateleira** (OCR
via modelos gratuitos do Google), defina uma **meta de orçamento** e receba
**alertas preventivos** antes de estourar no caixa. Dados ficam no
dispositivo; compartilhar observações de preço anonimizadas desbloqueia
funcionalidades premium (**share-to-unlock**).

## Stack

| Camada      | Tecnologia                                          |
|-------------|------------------------------------------------------|
| Frontend    | Blazor WebAssembly PWA (IndexedDB/localStorage)      |
| Backend     | ASP.NET Core Minimal APIs — monólito modular         |
| Banco       | PostgreSQL 16 (JSONB) — Fase 2+: + Redis + MongoDB   |
| IA / OCR    | Google Gemini (free tier) via proxy no servidor      |
| Infra MVP   | VM OCI 1GB RAM (Caddy + systemd)                     |

## Documentação (fontes de verdade)

| Documento | Conteúdo |
|-----------|----------|
| [`.specify/memory/constitution.md`](.specify/memory/constitution.md) | Princípios inegociáveis do projeto |
| **Negócio** | |
| [`docs/business/vision.md`](docs/business/vision.md) | Visão de negócio e fluxos F1–F5 (foto, quantidade, orçamento, alertas, CRUD) |
| [`docs/business/share-to-unlock.md`](docs/business/share-to-unlock.md) | Modelo de créditos e desbloqueios premium |
| **Domínio** | |
| [`docs/domain/domain-model.md`](docs/domain/domain-model.md) | Bounded contexts, entidades, linguagem ubíqua |
| [`docs/domain/status-messages.md`](docs/domain/status-messages.md) | Catálogo determinístico de mensagens e alertas |
| **Arquitetura** | |
| [`docs/architecture/overview.md`](docs/architecture/overview.md) | Visão geral, diagramas, estrutura da solução |
| [`docs/architecture/mvp-infrastructure.md`](docs/architecture/mvp-infrastructure.md) | VM OCI 1GB: orçamento de memória e deploy |
| [`docs/architecture/scale-migration-plan.md`](docs/architecture/scale-migration-plan.md) | Migração incremental para 10.000+ usuários |
| [`docs/architecture/ocr-integration.md`](docs/architecture/ocr-integration.md) | Integração Gemini, rate-limit e fallback manual |
| [`docs/architecture/adr/`](docs/architecture/adr/README.md) | Decisões arquiteturais (ADRs) |
| **Padrões** | |
| [`docs/standards/backend-standards.md`](docs/standards/backend-standards.md) | C#/.NET, módulos, memória, testes |
| [`docs/standards/frontend-standards.md`](docs/standards/frontend-standards.md) | Blazor PWA, offline-first, interop |
| [`docs/standards/api-standards.md`](docs/standards/api-standards.md) | Contratos HTTP, erros, versionamento |
| [`docs/standards/data-standards.md`](docs/standards/data-standards.md) | PostgreSQL/EF Core e IndexedDB |
| [`docs/standards/documentation-standards.md`](docs/standards/documentation-standards.md) | Como escrever specs e docs (SDD) |

## Desenvolvimento — Spec-Driven Development

O projeto usa o [GitHub Spec Kit](https://github.com/github/spec-kit).
Toda feature segue o fluxo (skills disponíveis no Cursor):

```
/speckit-specify  →  /speckit-plan  →  /speckit-tasks  →  /speckit-implement
```

Opcionais de qualidade: `/speckit-clarify` (antes do plan) e
`/speckit-analyze` (antes do implement). Specs ficam em `specs/`; princípios
em `.specify/memory/constitution.md`; rules do agente em `.cursor/rules/`.

## Fases

1. **Fase 1 (MVP):** monólito modular em VM OCI 1GB — PostgreSQL + cache
   in-process. Capacidade: milhares de usuários (fluxo principal roda no
   cliente).
2. **Fase 2+:** PostgreSQL gerenciado, Redis, MongoDB, N instâncias da API,
   workers assíncronos — ver plano de migração.
