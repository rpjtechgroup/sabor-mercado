# Feature Specification: Catálogo Inicial e Preenchimento por Voz

**Feature Branch**: `007-starter-catalog-voice`  
**Created**: 2026-06-16  
**Status**: Implemented

**Input**: Carga inicial curada de comércios e produtos via JSON (serviço + fallback offline), importação idempotente no IndexedDB, e preenchimento por voz com ditado + parser determinístico PT-BR nos formulários principais.

## User Scenarios

### US1 — Catálogo sugerido na primeira visita (P1)

Usuário abre o app com catálogo vazio e recebe automaticamente comércios e produtos sugeridos (sem preços), prontos para usar no carrinho e na compra mensal.

### US2 — Importação manual de sugestões (P2)

Usuário com dados próprios pode importar sugestões sob demanda, sem duplicar itens já importados.

### US3 — Ditado no carrinho (P1)

Usuário fala o item (nome, preço, quantidade) e o formulário é pré-preenchido para revisão antes de salvar.

### US4 — Ditado na busca de produto (P1)

Usuário usa microfone no campo de busca/autocomplete para localizar produtos no catálogo.

## Functional Requirements

- **FR-001**: Arquivo canônico `data/starter-catalog.pt-BR.json` com redes BR e ~50–100 itens sem preços.
- **FR-002**: DTOs compartilhados em `SaborMercado.Shared.StarterCatalog`.
- **FR-003**: `GET /api/v1/starter-catalog` sem autenticação, servindo o mesmo JSON.
- **FR-004**: Fallback offline via `wwwroot/data/starter-catalog.pt-BR.json`.
- **FR-005**: `StarterKey` em `Store` e `Product` para importação idempotente.
- **FR-006**: `StarterCatalogBootstrapService` com auto-import em catálogo vazio.
- **FR-007**: `SpeechRecognitionInterop` + `VoiceInputButton` com fallback manual.
- **FR-008**: `VoiceUtteranceParser` determinístico PT-BR (sem LLM).
- **FR-009**: Integração em `ProductAutocomplete` e `CartItemForm`.

## Constraints

- [Constitution I](../../.specify/memory/constitution.md): fluxo principal offline; backend opcional.
- [Constitution II](../../.specify/memory/constitution.md): voz é acelerador; digitação sempre disponível.
