# Implementation Plan: Núcleo Offline de Compras (Shopping + Catalog)

**Branch**: `001-offline-shopping-core` | **Date**: 2026-06-11 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/001-offline-shopping-core/spec.md`

## Summary

Primeira feature do produto: núcleo 100% cliente dos contextos **Shopping** e
**Catalog** ([domain-model](../../docs/domain/domain-model.md)) — sessão de
compra com meta de orçamento, carrinho virtual manual com ações rápidas de
quantidade, barra de orçamento com faixas de cor e avaliador determinístico de
mensagens de status ([catálogo](../../docs/domain/status-messages.md)), CRUD de
produtos com histórico de preços, e persistência local imediata (IndexedDB +
localStorage). Implementado como Blazor WebAssembly standalone PWA, sem backend.

## Technical Context

**Language/Version**: C# / .NET 8.0 (LTS atual instalado — SDK 8.0.414)

**Primary Dependencies**: Blazor WebAssembly standalone (template PWA com service worker + manifest); sem bibliotecas de state management (frontend-standards §8)

**Storage**: IndexedDB (stores `shoppingSessions`, `cartItems`, `products`, `priceRecords`, com `schemaVersion`) via interop JS fino e tipado; localStorage apenas para preferências leves ([data-standards](../../docs/standards/data-standards.md))

**Testing**: xUnit (Domain — avaliador de status é alvo nº 1) + bUnit (componentes com lógica: BudgetBar, banner de status)

**Target Platform**: Navegadores móveis (PWA mobile-first) e desktop; funciona offline (service worker cache-first para assets)

**Project Type**: Web app cliente puro (Blazor WASM standalone); nenhum projeto de backend nesta feature

**Performance Goals**: Recálculo total/alertas percebido instantâneo (< 100 ms por mutação); persistência imediata por mutação sem travar UI

**Constraints**: Offline-first (Constitution I); mensagens só do catálogo fechado (Constitution III); `decimal` + cultura `pt-BR` para dinheiro; `TreatWarningsAsErrors=true`; UI PT-BR, identificadores em inglês

**Scale/Scope**: Uso pessoal local (1 usuário/dispositivo); ~4 telas (sessão/carrinho, catálogo, detalhe de produto, formulários); 13 FRs

## Constitution Check

*GATE: avaliado antes da Phase 0 e reavaliado após Phase 1.*

| Princípio | Avaliação |
|-----------|-----------|
| I. Offline-First, Grátis por Padrão | ✅ Feature roda 100% no cliente; nenhuma chamada HTTP; dados em IndexedDB/localStorage |
| II. Degradação Graciosa da IA | ✅ N/A direto (OCR fora do escopo); o fluxo manual desta feature É o fallback exigido; `source: Ocr` previsto no modelo |
| III. Mensagens Determinísticas | ✅ Avaliador puro em `Domain/`, implementa exatamente o catálogo; textos em `.resx`; nenhum LLM |
| IV. Share-to-Unlock | ✅ N/A nesta feature; nenhum dado sai do dispositivo |
| V. Caber no MVP, Projetar p/ 10k+ | ✅ Sem backend (zero memória de servidor); fronteiras Shopping/Catalog respeitadas em pastas separadas |
| VI. C#/.NET de Ponta a Ponta | ✅ Blazor WASM PWA, .NET 8 LTS; JS apenas em interop fino para IndexedDB (sem alternativa nativa em WASM — não é mudança de stack) |
| VII. Spec-Driven Development | ✅ Este plano deriva de `specs/001-offline-shopping-core/spec.md` |

**Resultado**: PASS (pré e pós-design). Sem violações; Complexity Tracking vazio.

## Project Structure

### Documentation (this feature)

```text
specs/001-offline-shopping-core/
├── plan.md              # Este arquivo
├── research.md          # Phase 0
├── data-model.md        # Phase 1
├── quickstart.md        # Phase 1
├── contracts/
│   └── status-evaluator.md   # Contrato do avaliador de mensagens
├── checklists/
│   └── requirements.md
└── tasks.md             # Phase 2 (/speckit-tasks)
```

### Source Code (repository root)

Subconjunto da estrutura de [`docs/architecture/overview.md`](../../docs/architecture/overview.md)
(projetos de backend/módulos NÃO entram nesta feature):

```text
SaborMercado.sln
Directory.Build.props            # TargetFramework, Nullable, TreatWarningsAsErrors
Directory.Packages.props         # Central Package Management
src/
  SaborMercado.Web/              # Blazor WASM standalone PWA
    Domain/                      # Modelos locais + avaliador puro de status
      Shopping/                  #   ShoppingSession, CartItem, BudgetAlertState
      Catalog/                   #   Product, PriceRecord
      Status/                    #   StatusMessageEvaluator, StatusCode, BudgetRange
    Features/
      Shopping/                  # Páginas/componentes F2–F4 + ShoppingService
      Catalog/                   # Páginas/componentes F5 + CatalogService
    Shared/                      # BudgetBar, MoneyInput, StatusBanner, layout
    Storage/                     # IndexedDbStore<T>, PreferencesStore, schema/migrações
    Interop/                     # IndexedDbInterop (wrapper C# tipado)
    Resources/                   # StatusMessages.resx + UI strings (pt-BR)
    wwwroot/
      js/indexedDb.js            # Módulo JS isolado (único JS do app)
      manifest.webmanifest, service-worker.js (template PWA)
tests/
  SaborMercado.Web.Tests/        # xUnit + bUnit
    Domain/                      # Avaliador de status, cálculos do carrinho
    Components/                  # bUnit: BudgetBar, StatusBanner
```

**Structure Decision**: pastas por feature conforme
[frontend-standards](../../docs/standards/frontend-standards.md); `Features/Recognition`
e `Features/Rewards` não são criadas (features futuras). Testes ficam em
`tests/SaborMercado.Web.Tests` (único projeto de teste necessário; `UnitTests`/
`IntegrationTests` do overview são para o backend futuro).

## Decisões de design (resumo — detalhes em research.md)

1. **Avaliador de status**: função pura `Evaluate(BudgetAlertState before, CartSnapshot cart, CartMutation mutation, TimeProvider clock) → (StatusMessage?, BudgetAlertState after)` — sem efeitos colaterais, retorna também o novo estado para persistência. Implementa exatamente os códigos/gatilhos/prioridades/cooldown/rearme do catálogo.
2. **IndexedDB**: módulo JS `wwwroot/js/indexedDb.js` exportando `openDb/put/get/getAll/getByIndex/delete/clear`; wrapper C# `IndexedDbInterop` (IJSRuntime → `IJSObjectReference` de módulo ES); serialização JSON camelCase; `schemaVersion` em todo objeto; store `pendingShares` do data-standards fica para a feature de share-to-unlock (criada no schema desde já para evitar bump de versão? — NÃO: versão do DB é incremental, criar quando necessário; registrado em research.md).
3. **Dinheiro**: `decimal` em todos os modelos; formatação centralizada (`MoneyFormat.Format(decimal)` com `CultureInfo("pt-BR")`); input com vírgula via componente `MoneyInput`.
4. **i18n**: `Resources/StatusMessages.resx` com chave = código do catálogo; `StatusMessageLocalizer` resolve texto + placeholders. UI strings também em `.resx`.
5. **Estado**: serviços por feature (`ShoppingService`, `CatalogService`) com evento `StateChanged`; componentes assinam e re-renderizam.
6. **Sessão única ativa**: `ShoppingService` garante no máximo 1 sessão `Active`; ao iniciar nova com outra ativa, UI exige encerrar/abandonar.

## Complexity Tracking

Sem violações da Constitution — tabela não aplicável.
