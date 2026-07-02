# Feature Specification: Lembretes para a Próxima Compra

**Feature Branch**: `008-shopping-reminders`  
**Created**: 2026-06-16  
**Status**: Implemented

**Input**: Lembretes offline criados a qualquer momento (produtos do catálogo) que pré-preenchem o carrinho ao iniciar compra esporádica.

## User Scenarios

### US1 — Registrar falta em casa (P1)

Usuário adiciona produto do catálogo com quantidade quando percebe que algo está acabando.

### US2 — Prefill na compra esporádica (P1)

Ao iniciar compra esporádica, lembretes entram no carrinho com último preço conhecido (ou zero se sem histórico).

### US3 — Abandonar restaura lembretes (P2)

Se o usuário abandona a compra, lembretes consumidos voltam para a lista.

### US4 — Lista mensal inalterada (P1)

Compra mensal continua usando apenas `ShoppingPattern`; lembretes não são consumidos.

## Functional Requirements

- **FR-001**: `ShoppingReminder` em IndexedDB v5 (`shoppingReminders`).
- **FR-002**: `ShoppingReminderService` com CRUD, deduplicação e consume/restore.
- **FR-003**: `PrefillFromRemindersAsync` só em `SessionKind.Sporadic`.
- **FR-004**: Página `/lembretes` para gerenciar lembretes.
- **FR-005**: Links em `SessionStart` e `ShoppingPage`.
