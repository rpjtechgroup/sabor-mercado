# Feature Specification: Comparador Excel e Compra Mensal

**Feature Branch**: `005-monthly-comparator`  
**Created**: 2026-06-11  
**Status**: Implemented

**Input**: Comparador estilo planilha (matriz por mercado + histórico com delta), último preço no catálogo, padrão de compra mensal com prefill do carrinho; orçamento só em compras esporádicas.

## User Scenarios

### US1 — Matriz comparador (P1)

Usuário vê último preço por produto em cada mercado e identifica o mais barato.

### US2 — Histórico com delta (P1)

Tabela Mercado | Produto | Peso/Volume | Valor | Δ vs compra anterior do mesmo produto.

### US3 — Catálogo com último preço (P2)

Lista de produtos exibe sempre o último preço registrado.

### US4 — Padrão mensal (P1)

Usuário cadastra lista mensal; ao iniciar compra mensal, carrinho é pré-preenchido com último preço do catálogo, sem meta de orçamento.

### US5 — Compra esporádica (P1)

Compra esporádica mantém campo de orçamento e alertas F3/F4.

## Functional Requirements

- **FR-001**: `MarketPriceMatrixService` com matriz produto × mercado (90 dias).
- **FR-002**: `PurchaseLineRow.DeltaVsPrevious` e tabela Excel no histórico.
- **FR-003**: `ShoppingPattern` em IndexedDB v3.
- **FR-004**: `SessionKind` Monthly/Sporadic; orçamento só em Sporadic.
- **FR-005**: `/compra-mensal` para CRUD do padrão.
