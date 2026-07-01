# Feature Specification: Comparador Excel e Compra Mensal

**Feature Branch**: `005-monthly-comparator`  
**Created**: 2026-06-11  
**Status**: Implemented (comparador `/historico` atualizado em 006 — ver nota abaixo)

**Input**: Comparador estilo planilha (matriz por mercado + histórico com delta), último preço no catálogo, padrão de compra mensal com prefill do carrinho; orçamento só em compras esporádicas.

> **Supersede (006-comparator-single-grid):** a página `/historico` passou a exibir um **único grid** (Produto | Comércio | Melhor | Pior | Quando) com colunas reordenáveis por drag-and-drop. As abas Comparador / Histórico / Por compra e a matriz produto × mercado foram removidas. Compra mensal, catálogo e orçamento esporádico **não** foram alterados.

## User Scenarios

### US1 — Comparador por produto (P1)

Usuário vê melhor e pior preço por produto nos últimos 90 dias, com o comércio onde pagou mais barato e a data dessa compra.

### US1b — Matriz comparador (legado, removido)

~~Usuário vê último preço por produto em cada mercado e identifica o mais barato.~~ Substituído por US1.

### US2 — Histórico com delta (legado, removido)

~~Tabela Mercado | Produto | Peso/Volume | Valor | Δ vs compra anterior do mesmo produto.~~ Substituído por US1.

### US3 — Catálogo com último preço (P2)

Lista de produtos exibe sempre o último preço registrado.

### US4 — Padrão mensal (P1)

Usuário cadastra lista mensal; ao iniciar compra mensal, carrinho é pré-preenchido com último preço do catálogo, sem meta de orçamento.

### US5 — Compra esporádica (P1)

Compra esporádica mantém campo de orçamento e alertas F3/F4.

## Functional Requirements

- **FR-001**: `ProductPriceComparisonService` com grid Produto | Comércio | Melhor | Pior | Quando (90 dias); ordem de colunas persistida em localStorage.
- **FR-002**: ~~`PurchaseLineRow.DeltaVsPrevious` e tabela Excel no histórico.~~ Removido em 006.
- **FR-003**: `ShoppingPattern` em IndexedDB v3.
- **FR-004**: `SessionKind` Monthly/Sporadic; orçamento só em Sporadic.
- **FR-005**: `/compra-mensal` para CRUD do padrão.
