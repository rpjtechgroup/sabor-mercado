# Data Model — 001 Núcleo Offline de Compras

> Phase 1. Deriva de [`docs/domain/domain-model.md`](../../docs/domain/domain-model.md)
> (contextos Shopping e Catalog) e de
> [`docs/standards/data-standards.md`](../../docs/standards/data-standards.md).
> Identificadores em inglês; persistência em IndexedDB (JSON camelCase).

## Convenções

- IDs: `Guid` no formato UUID v7 (ordenável) via helper `Domain/Ids.cs`
  (`Guid.CreateVersion7()` só existe a partir do .NET 9; o app usa .NET 8 LTS).
- Timestamps: `DateTimeOffset` UTC; dinheiro: `decimal`.
- Todo objeto persistido carrega `int SchemaVersion` (atual: 1).

## Shopping (store: `shoppingSessions`, `cartItems`)

### ShoppingSession (raiz)

| Campo | Tipo | Regras |
|-------|------|--------|
| `Id` | Guid | PK |
| `MarketName` | string? | opcional |
| `BudgetAmount` | decimal? | opcional; > 0 quando informado |
| `StartedAt` | DateTimeOffset | obrigatório |
| `FinishedAt` | DateTimeOffset? | preenchido ao finalizar/abandonar |
| `Status` | enum `SessionStatus` | `Active` \| `Finished` \| `Abandoned` |
| `AlertState` | BudgetAlertState | embutido (1:1, persiste junto) |
| `SchemaVersion` | int | =1 |

Transições: `Active → Finished` (encerrar) e `Active → Abandoned` (abandonar).
Invariante: no máximo 1 sessão `Active` por vez.

### CartItem (store `cartItems`, índice `sessionId`)

| Campo | Tipo | Regras |
|-------|------|--------|
| `Id` | Guid | PK |
| `SessionId` | Guid | FK lógica p/ sessão |
| `ProductSnapshot` | ProductSnapshot | embutido (nome obrigatório; marca, quantityValue, quantityUnit opcionais) |
| `UnitPrice` | decimal | ≥ 0 |
| `Quantity` | int | > 0 (invariante do domínio) |
| `Source` | enum `CartItemSource` | `Ocr` \| `Manual` \| `Catalog` (esta feature emite `Manual`/`Catalog`) |
| `AddedAt` | DateTimeOffset | obrigatório |
| `SchemaVersion` | int | =1 |

Derivados (não persistidos): `Subtotal = UnitPrice × Quantity`;
total da sessão `T = Σ Subtotal`.

### BudgetAlertState (embutido em ShoppingSession)

Estado do avaliador de mensagens (controle de emissão única, rearme e cooldown
— regras de emissão do [catálogo](../../docs/domain/status-messages.md)):

| Campo | Tipo | Uso |
|-------|------|-----|
| `EmittedCodes` | set de string | códigos `BUDGET_*` de cruzamento já emitidos (emissão única + rearme) |
| `LastPercentUsed` | decimal | `P` após a última mutação (detecção de cruzamento) |
| `LastPaceEmissionAt` | DateTimeOffset? | cooldown temporal de `PACE_*` |
| `ItemCountAtLastPaceEmission` | int? | cooldown por contagem de itens de `PACE_*` |

## Catalog (stores: `products`, `priceRecords`)

### Product (raiz)

| Campo | Tipo | Regras |
|-------|------|--------|
| `Id` | Guid | PK |
| `Name` | string | obrigatório, não vazio |
| `Brand` | string? | opcional |
| `QuantityValue` | decimal? | opcional; > 0 quando informado |
| `QuantityUnit` | enum? `QuantityUnit` | `g` \| `kg` \| `ml` \| `l` \| `un` |
| `Ean` | string? | opcional |
| `Category` | string? | opcional |
| `Notes` | string? | opcional |
| `CreatedAt` | DateTimeOffset | obrigatório |
| `SchemaVersion` | int | =1 |

### PriceRecord (store `priceRecords`, índice `productId`)

| Campo | Tipo | Regras |
|-------|------|--------|
| `Id` | Guid | PK (chave do store) |
| `ProductId` | Guid | FK lógica |
| `Price` | decimal | ≥ 0 |
| `MarketName` | string? | opcional |
| `ObservedAt` | DateTimeOffset | obrigatório |
| `Source` | enum `PriceSource` | `Ocr` \| `Manual` (esta feature emite `Manual`) |
| `SchemaVersion` | int | =1 |

Exclusão de `Product` remove em cascata seus `PriceRecord` (decisão na spec).
"Último preço conhecido" = `PriceRecord` com maior `ObservedAt` do produto.

## Status (não persistido — tipos do avaliador)

- `StatusCode`: constantes string estáveis, exatamente os códigos do catálogo.
- `StatusMessage`: `Code` + `Args` (dicionário para placeholders) + `Severity`
  derivada da faixa.
- `BudgetRange`: `Ok` (`budget-ok`) \| `Warn` (`budget-warn`) \| `High`
  (`budget-high`) \| `Over` (`budget-over`) — faixas de `P` do catálogo.
- `CartMutation`: `SessionStarted(budget?)` \| `ItemAdded` \| `ItemUpdated` \|
  `ItemRemoved` \| `SessionFinished` — discrimina os gatilhos.

## Preferências (localStorage — somente itens leves)

| Chave | Tipo | Uso |
|-------|------|-----|
| `saborMercado.preferences.budgetDefault` | decimal? | sugestão de meta ao iniciar sessão |
| `saborMercado.preferences.schemaVersion` | int | versão das preferências |

Proibido dado de domínio em localStorage (data-standards).
