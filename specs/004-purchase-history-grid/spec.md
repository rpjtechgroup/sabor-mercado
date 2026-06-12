# Feature Specification: Histórico de Compras e Autocomplete

**Feature Branch**: `004-purchase-history-grid`  
**Created**: 2026-06-11  
**Status**: Implemented

**Input**: Controle detalhado de compras dos últimos 3 meses em grid com comparação de preços, catálogo alimentado automaticamente e autocomplete no carrinho.

Regras canônicas: Constitution I (offline-first), F2/F5 em `docs/business/vision.md`

## User Scenarios

### US1 — Histórico por compra (P1)

Usuário vê lista de compras finalizadas (3 meses) e abre o grid de uma compra.

**Acceptance**:
- Given compras finalizadas nos últimos 3 meses, When abre `/historico`, Then vê lista com data, mercado e total.
- Given uma compra selecionada, When expande detalhe, Then vê grid com mercado, produto, quantidade, unidade e preço em ordem alfabética.

### US2 — Histórico consolidado (P1)

Usuário compara todas as linhas dos últimos 3 meses em um único grid.

**Acceptance**:
- Given múltiplas compras, When aba "Consolidado", Then todas as linhas aparecem ordenadas alfabeticamente por produto.

### US3 — Cores de tendência de preço (P1)

Preço mais barato que a compra anterior do mesmo produto fica verde; mais caro fica vermelho; primeira ocorrência fica neutra.

### US4 — Catálogo automático (P2)

Produtos adicionados ao carrinho alimentam o catálogo pessoal.

**Acceptance**:
- Given item manual ou OCR, When adiciona ao carrinho, Then produto é criado/reutilizado no catálogo e preço registrado.

### US5 — Autocomplete (P2)

Ao adicionar item, usuário busca produtos já cadastrados com sugestões e último preço.

## Functional Requirements

- **FR-001**: Grid com colunas Mercado, Produto, Quantidade, Unidade, Preço.
- **FR-002**: Janela de 3 meses (`FinishedAt` UTC).
- **FR-003**: `PriceTrend` determinístico (None/Cheaper/MoreExpensive).
- **FR-004**: `ProductId` opcional em `CartItem`; fallback por snapshot.
- **FR-005**: `CatalogService.EnsureProductAsync` + autocomplete.
- **FR-006**: 100% offline; sem paywall.

## Success Criteria

- **SC-001**: Testes de `PurchaseHistoryService` cobrem filtro, ordenação e cores.
- **SC-002**: Adicionar item manual cria produto no catálogo.
- **SC-003**: Autocomplete preenche último preço conhecido.
