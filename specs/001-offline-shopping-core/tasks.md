# Tasks: Núcleo Offline de Compras (Shopping + Catalog)

**Input**: Design documents from `/specs/001-offline-shopping-core/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/status-evaluator.md, quickstart.md

**Tests**: INCLUÍDOS — a Constitution (Workflow) e a spec (SC-003) exigem testes; cobertura prioritária é o avaliador de status e os cálculos do carrinho.

**Organization**: Tarefas agrupadas por user story para permitir implementação e teste independentes.

## Format: `[ID] [P?] [Story] Description`

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Solution .NET, projeto Blazor WASM PWA e projeto de testes

- [X] T001 Create `SaborMercado.sln` at repo root, `Directory.Build.props` (net8.0, Nullable, ImplicitUsings, `TreatWarningsAsErrors=true`) and `Directory.Packages.props` (Central Package Management) per backend-standards
- [X] T002 Scaffold Blazor WASM standalone PWA project in `src/SaborMercado.Web` (`dotnet new blazorwasm --pwa`), add to solution, remove template boilerplate (Counter/Weather/Bootstrap sample), set `pt-BR` culture (`BlazorWebAssemblyLoadAllGlobalizationData` + `CultureInfo` default in `Program.cs`), localize `wwwroot/manifest.webmanifest`
- [X] T003 Scaffold xUnit test project in `tests/SaborMercado.Web.Tests` with bUnit package, reference `src/SaborMercado.Web`, add to solution; verify `dotnet build` and `dotnet test` pass empty
- [X] T004 [P] Create feature folder structure in `src/SaborMercado.Web`: `Domain/Shopping`, `Domain/Catalog`, `Domain/Status`, `Features/Shopping`, `Features/Catalog`, `Shared`, `Storage`, `Interop`, `Resources`; mobile-first base layout and app CSS (`Layout/MainLayout.razor`, `wwwroot/css/app.css`) with bottom navigation (Compra | Catálogo) and large touch targets

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Domínio, avaliador de status (alvo nº 1 de testes), persistência e componentes compartilhados usados por todas as stories

**⚠️ CRITICAL**: nenhuma user story começa antes desta fase terminar

- [X] T005 [P] Create Shopping domain models in `src/SaborMercado.Web/Domain/Shopping/`: `ShoppingSession.cs`, `CartItem.cs` (+`ProductSnapshot`), `BudgetAlertState.cs`, `SessionStatus.cs`, `CartItemSource.cs` per data-model.md (decimal money, `Guid.CreateVersion7()`, `SchemaVersion`)
- [X] T006 [P] Create Catalog domain models in `src/SaborMercado.Web/Domain/Catalog/`: `Product.cs`, `PriceRecord.cs`, `QuantityUnit.cs`, `PriceSource.cs` per data-model.md
- [X] T007 [P] Create status types in `src/SaborMercado.Web/Domain/Status/`: `StatusCodes.cs` (todos os códigos do catálogo como constantes), `StatusMessage.cs`, `BudgetRange.cs` + `BudgetRanges.FromPercent` (faixas 60/85/100%), `CartSnapshot.cs`, `CartMutation.cs`, `EvaluationInput.cs`/`EvaluationResult.cs` per contracts/status-evaluator.md
- [X] T008 Implement pure evaluator `src/SaborMercado.Web/Domain/Status/StatusMessageEvaluator.cs`: gatilhos, prioridade, emissão única por sessão, rearme ao recuar de faixa, cooldown PACE (5 itens OU 5 min), projeção `E` (regras 1 e 2, teto 3×T), regra "sem B" — exatamente `docs/domain/status-messages.md` (depends on T005, T007)
- [X] T009 Write evaluator unit tests in `tests/SaborMercado.Web.Tests/Domain/StatusMessageEvaluatorTests.cs`: todos os códigos emissíveis, cruzamentos de cada limiar (50/75/90/100), `BUDGET_EXCEEDED` por novo item estourado, prioridade em mutação múltipla, emissão única, rearme, cooldowns, sessão sem meta, `SESSION_FINISHED` (3 variações), projeções (nomes rastreáveis, ex.: `BudgetAlert_Crosses75Percent_EmitsBudgetWarn75`) (depends on T008)
- [X] T010 [P] Write `BudgetRanges` boundary tests in `tests/SaborMercado.Web.Tests/Domain/BudgetRangesTests.cs` (0%, 59,99%, 60%, 84,99%, 85%, 99,99%, 100%, >100%)
- [X] T011 [P] Create IndexedDB JS module `src/SaborMercado.Web/wwwroot/js/indexedDb.js` (ES module: `open` com upgrade criando stores `shoppingSessions`/`cartItems`/`products`/`priceRecords` + índices, `put`, `get`, `getAll`, `getAllByIndex`, `delete`, `deleteByIndex`, promise-based) per research.md R2/R3
- [X] T012 Create typed interop wrapper `src/SaborMercado.Web/Interop/IndexedDbInterop.cs` (lazy `IJSObjectReference` import, JSON camelCase, `IAsyncDisposable`) — único ponto de contato com JS (depends on T011)
- [X] T013 Create storage layer in `src/SaborMercado.Web/Storage/`: `IEntityStore<T>`/`IndexedDbStore<T>` per-store wrappers (sessions, cartItems, products, priceRecords), `PreferencesStore.cs` (localStorage só preferências: `budgetDefault`), `StorageSchema.cs` (nomes de stores, db version, `schemaVersion` atual) (depends on T012, T005, T006)
- [X] T014 [P] Create money helpers `src/SaborMercado.Web/Shared/MoneyFormat.cs` (format/parse `pt-BR`, 2 casas, vírgula decimal) + tests in `tests/SaborMercado.Web.Tests/Domain/MoneyFormatTests.cs`
- [X] T015 [P] Create i18n resources `src/SaborMercado.Web/Resources/StatusMessages.resx` (chave = código do catálogo, textos pt-BR exatos, variações `SESSION_FINISHED_UNDER/OVER`) + localizer setup in `Program.cs`; renderer `Shared/StatusMessageLocalizer.cs` que injeta `Args` nos placeholders. *Nota de execução: `UiStrings.resx` não foi criado — frontend-standards só exige recurso i18n para mensagens de status; rótulos gerais da UI estão em PT-BR direto no markup.*
- [X] T016 Create shared components in `src/SaborMercado.Web/Shared/`: `StatusBanner.razor` (exibe `StatusMessage` localizado, auto-dismiss, sem texto hardcoded), `MoneyInput.razor` (máscara vírgula, `inputmode="decimal"`) (depends on T014, T015)

**Checkpoint**: domínio + avaliador testados, storage pronto — user stories podem começar

---

## Phase 3: User Story 1 - Sessão de compra com carrinho e orçamento (Priority: P1) 🎯 MVP

**Goal**: iniciar/encerrar/abandonar sessão com meta opcional; carrinho manual com `+1`/`+5`/digitar, edição e remoção; total/restante instantâneos; persistência imediata e restauração offline

**Independent Test**: cenário 1 do quickstart.md em modo avião (iniciar sessão R$ 300 → adicionar/ajustar/remover itens → encerrar → resumo; F5 restaura tudo)

### Tests for User Story 1

- [X] T017 [P] [US1] Write cart calculation + session rules tests in `tests/SaborMercado.Web.Tests/Domain/CartCalculationTests.cs` (subtotal `qty × unitPrice`, total Σ, restante `B−T`, quantity > 0, preço ≥ 0, 3 × R$ 8,99 = R$ 26,97) and `tests/SaborMercado.Web.Tests/Features/ShoppingServiceTests.cs` (uma sessão ativa por vez, transições Active→Finished/Abandoned, mutações chamam avaliador e persistem estado — storage fake em memória)

### Implementation for User Story 1

- [X] T018 [US1] Implement `src/SaborMercado.Web/Features/Shopping/ShoppingService.cs`: load/restore active session, start (meta opcional + sugestão de `budgetDefault`), add/update/remove item (source `Manual`), quantity actions, totals, finish/abandon; chama `StatusMessageEvaluator` a cada mutação, persiste sessão+itens+`AlertState` imediatamente, evento `StateChanged` (depends on T008, T013, T017)
- [X] T019 [US1] Create session start UI `src/SaborMercado.Web/Features/Shopping/SessionStart.razor` ("Quanto pretende gastar?", mercado opcional, bloqueio de 2ª sessão ativa com opção encerrar/abandonar)
- [X] T020 [US1] Create cart UI `src/SaborMercado.Web/Features/Shopping/ShoppingPage.razor` + `CartItemRow.razor` + `CartItemForm.razor` (lista, total/restante, ações `+1`/`+5`/`Digitar quantidade`, editar/remover com confirmação, encerrar sessão com resumo `SESSION_FINISHED`, `StatusBanner` integrado), rota raiz `/` (depends on T018, T016)

**Checkpoint**: compra completa 100% offline funcional (MVP)

---

## Phase 4: User Story 2 - Barra de orçamento e alertas determinísticos (Priority: P2)

**Goal**: barra com faixas de cor exatas do catálogo + exibição dos alertas de orçamento/projeção emitidos pelo avaliador

**Independent Test**: cenário 2 do quickstart.md (meta R$ 100; cruzar 50/75/90/100%+ conferindo cor e mensagem; rearme após remoções)

### Tests for User Story 2

- [X] T021 [P] [US2] Write bUnit tests in `tests/SaborMercado.Web.Tests/Components/BudgetBarTests.cs` (cor/percentual por faixa, sem meta = sem barra) and `tests/SaborMercado.Web.Tests/Components/StatusBannerTests.cs` (renderiza texto do recurso pelo código, nunca hardcoded)

### Implementation for User Story 2

- [X] T022 [US2] Create `src/SaborMercado.Web/Shared/BudgetBar.razor` (+`.razor.css`): barra de progresso com classes `budget-ok|warn|high|over` via `BudgetRanges.FromPercent`, exibe total, restante e % — mobile-first (depends on T007, T021)
- [X] T023 [US2] Integrate `BudgetBar` + alert flow into `src/SaborMercado.Web/Features/Shopping/ShoppingPage.razor` (barra visível só com meta; banner mostra a mensagem da última mutação; cores também no texto de restante) (depends on T020, T022)

**Checkpoint**: F3+F4 completos; US1 e US2 testáveis de ponta a ponta

---

## Phase 5: User Story 3 - Catálogo pessoal com histórico de preços (Priority: P3)

**Goal**: CRUD de produtos, histórico de preços e "adicionar ao carrinho" a partir do catálogo com último preço e source `Catalog`

**Independent Test**: cenário 4 do quickstart.md offline (cadastrar → 2 preços → editar → adicionar ao carrinho → excluir; item do carrinho intacto)

### Tests for User Story 3

- [X] T024 [P] [US3] Write `tests/SaborMercado.Web.Tests/Features/CatalogServiceTests.cs` (CRUD, nome obrigatório, histórico ordenado desc, último preço conhecido, exclusão cascateia priceRecords, snapshot no carrinho sobrevive à exclusão — storage fake)

### Implementation for User Story 3

- [X] T025 [US3] Implement `src/SaborMercado.Web/Features/Catalog/CatalogService.cs` (CRUD produtos + price records, last-known-price, persistência imediata, `StateChanged`) (depends on T013, T024)
- [X] T026 [US3] Create catalog UI `src/SaborMercado.Web/Features/Catalog/CatalogPage.razor` (rota `/catalogo`, lista com busca por nome) + `ProductForm.razor` (todos os campos do data-model, unidade g/kg/ml/l/un) (depends on T025, T016)
- [X] T027 [US3] Create product detail `src/SaborMercado.Web/Features/Catalog/ProductDetail.razor` (histórico de preços desc, registrar novo preço, editar/excluir com confirmação) and "Adicionar à compra" action wiring `CatalogService` → `ShoppingService.AddFromCatalog` (snapshot + último preço + source `Catalog`; botão visível só com sessão ativa) (depends on T025, T018)

**Checkpoint**: todas as user stories funcionais e independentes

---

## Phase 6: Polish & Cross-Cutting Concerns

- [X] T028 [P] Update root `README.md` index/instructions (solution layout, build/test/run commands) per docs-standards
- [X] T029 Full verification: `dotnet build SaborMercado.sln` zero warnings, `dotnet test SaborMercado.sln` green, `dotnet run --project src/SaborMercado.Web` sobe sem erro (encerrar após smoke test), walk through quickstart.md scenarios
- [X] T030 Review conformidade final: nenhum texto de status hardcoded, nenhuma mensagem fora do catálogo, dinheiro sempre `decimal`/pt-BR, JS apenas em `Interop`/`wwwroot/js`, localStorage só preferências

---

## Dependencies & Execution Order

- **Phase 1 → Phase 2 → (Phases 3, 4, 5) → Phase 6**
- **US1 (P1)**: depende só da Phase 2 — é o MVP
- **US2 (P2)**: depende da Phase 2; T023 integra na página criada em T020 (US1)
- **US3 (P3)**: depende da Phase 2; T027 integra com `ShoppingService` (US1) para "adicionar à compra", mas CRUD (T024–T026) é independente
- Dentro de cada story: testes → service → UI

### Parallel Opportunities

- Phase 2: T005, T006, T007 em paralelo; T011 em paralelo com domínio; T010, T014, T015 em paralelo após tipos básicos
- Após Phase 2: US1, US2 (componente BudgetBar) e US3 (CRUD) podem andar em paralelo; integrações (T023, T027) ao final
- Testes [P] de stories diferentes em paralelo (arquivos distintos)

## Implementation Strategy

MVP = Phases 1+2+3 (US1). Entrega incremental: US1 → validar → US2 → validar →
US3 → polish. Critério de pronto global: build sem warnings, testes verdes,
quickstart.md validado.
