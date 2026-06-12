# Feature Specification: Modo Foto Inteligente (OCR)

**Feature Branch**: `002-smart-photo-ocr`  
**Created**: 2026-06-11  
**Status**: Implemented

> Regras canônicas: [`docs/business/vision.md`](../../docs/business/vision.md) (F1),
> [`docs/architecture/ocr-integration.md`](../../docs/architecture/ocr-integration.md),
> [`docs/domain/status-messages.md`](../../docs/domain/status-messages.md) (códigos
> `ITEM_ADDED_OCR*`, `OCR_UNAVAILABLE`).

## User Story — Modo Foto (Priority: P1)

O usuário fotografa a etiqueta da prateleira; o backend (proxy Gemini) extrai
nome, marca, volume, preço e adiciona ao carrinho. Se `confidence ≥ 0,8`, o item
entra direto com `ITEM_ADDED_OCR`; se menor, abre revisão com
`ITEM_ADDED_OCR_REVIEW`. Se a API falhar, `OCR_UNAVAILABLE` e formulário manual
pré-preenchido.

**Independent Test**: sessão ativa → foto → item no carrinho; API fora → fallback
manual sem perder a sessão.

## Acceptance Scenarios

1. **Given** sessão ativa e API disponível, **When** foto com confidence ≥ 0,8,
   **Then** item entra no carrinho com `source: Ocr` e mensagem `ITEM_ADDED_OCR`.
2. **Given** confidence < 0,8, **When** usuário confirma no formulário,
   **Then** item entra com `ITEM_ADDED_OCR_REVIEW`.
3. **Given** API retorna 503, **When** usuário fotografa,
   **Then** `OCR_UNAVAILABLE` e formulário manual aberto.
4. **Given** sem sessão ativa, **When** abre /foto,
   **Then** orientação para iniciar compra.

## Fora de escopo

- Autenticação de usuário (rate-limit por IP).
- PostgreSQL em produção para `recognition_logs` (SQLite no MVP local).
