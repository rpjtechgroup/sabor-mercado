# Architecture Decision Records (ADRs)

Toda decisão arquitetural relevante é registrada aqui (Constitution VII).

## Formato

Arquivo `ADR-NNNN-titulo-curto-kebab.md` com as seções:
**Status** (Proposto | Aceito | Substituído por ADR-X), **Contexto**,
**Decisão**, **Consequências**, **Alternativas consideradas**.

## Índice

| ADR | Título | Status |
|-----|--------|--------|
| [0001](ADR-0001-postgresql-primary-database.md) | PostgreSQL como banco primário (JSONB no MVP) | Aceito |
| [0002](ADR-0002-blazor-wasm-pwa.md) | Blazor WebAssembly como PWA | Aceito |
| [0003](ADR-0003-modular-monolith-first.md) | Monólito modular antes de serviços | Aceito |
| [0004](ADR-0004-redis-mongodb-deferred.md) | Redis e MongoDB adiados para a Fase 2 | Aceito |
| [0006](ADR-0006-sqlite-local-bootstrap.md) | SQLite para bootstrap local/testes (emenda ADR-0001) | Aceito |
| [0005](ADR-0005-gemini-ocr-server-proxy.md) | OCR via Gemini free tier com proxy no servidor | Aceito |
