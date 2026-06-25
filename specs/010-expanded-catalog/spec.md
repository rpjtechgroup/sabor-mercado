# Feature Specification: Catálogo Inicial Expandido

**Feature Branch**: `010-expanded-catalog`  
**Created**: 2026-06-24  
**Status**: Implemented

**Input**: Expandir o catálogo sugerido de primeira utilização de 67 para 300+ produtos,
com taxonomia hierárquica por departamento e cobertura ampla de compras domésticas
brasileiras. Marcas são opcionais; o foco é o produto genérico.

## User Scenarios

### US1 — Primeira visita com catálogo rico (P1)

Usuário abre o app pela primeira vez e recebe automaticamente 300+ produtos
organizados em departamentos, categorias e subcategorias, prontos para busca,
carrinho e compra mensal — sem precisar cadastrar itens básicos manualmente.

### US2 — Navegação por hierarquia (P1)

Usuário encontra produtos pelo caminho `Departamento > Categoria > Subcategoria`
(ex.: `Mercearia > Grãos e Cereais > Arroz`) na busca e filtros do catálogo.

### US3 — Importação idempotente preservada (P1)

Usuário que já importou o catálogo antigo não recebe duplicatas ao atualizar o app;
`StarterKey` continua garantindo idempotência.

## Functional Requirements

- **FR-001**: Arquivo canônico [`data/starter-catalog.pt-BR.json`](../../data/starter-catalog.pt-BR.json)
  com `version: 2`, 6 redes BR e **≥ 300 produtos** sem preços.
- **FR-002**: Categoria hierárquica via convenção `Departamento > Categoria > Subcategoria`
  no campo `category` (string livre, sem migração de schema).
- **FR-003**: 8 departamentos: Mercearia, Frios e Laticínios, Bebidas, Hortifruti,
  Açougue, Padaria e Confeitaria, Higiene e Beleza, Limpeza.
- **FR-004**: Cada subcategoria com ≥ 3 produtos representativos.
- **FR-005**: Unidades permitidas: `g`, `kg`, `ml`, `l`, `un` (conforme `QuantityUnit`).
- **FR-006**: `key` única em kebab-case; `defaultStoreKey` referenciando loja existente.
- **FR-007**: Taxonomia documentada em [`data/product-taxonomy.pt-BR.json`](../../data/product-taxonomy.pt-BR.json).
- **FR-008**: Script de validação em [`scripts/catalog-validator.mjs`](../../scripts/catalog-validator.mjs).
- **FR-009**: Documentação de referência em [`docs/catalog/catalog-reference.pt-BR.md`](../../docs/catalog/catalog-reference.pt-BR.md).
- **FR-010**: Compatibilidade com `GET /api/v1/starter-catalog` e fallback offline
  (sem alteração de contrato API).

## Acceptance Criteria

- [x] `starter-catalog.pt-BR.json` contém ≥ 300 produtos.
- [x] Nenhuma `key` duplicada.
- [x] Todas as `defaultStoreKey` existem no array `stores`.
- [x] Todas as `quantityUnit` são válidas (`g`, `kg`, `ml`, `l`, `un`).
- [x] 8 departamentos cobertos com hierarquia consistente.
- [x] `catalog-validator.mjs` passa sem erros.
- [x] Import via `StarterCatalogBootstrapService` funciona em catálogo vazio.

## Constraints

- [Constitution I](../../.specify/memory/constitution.md): catálogo offline-first; JSON estático.
- [Constitution VII](../../.specify/memory/constitution.md): spec-driven; UI em PT-BR.
- Sem preços no catálogo inicial (preços são do usuário/OCR).
- Marcas opcionais; nomes genéricos preferidos.

## Out of Scope

- Alteração do modelo `Product` ou schema IndexedDB.
- UI de árvore de categorias (filtro por string continua suficiente).
- Produtos sazonais (ceia, páscoa) — candidatos para v2.
