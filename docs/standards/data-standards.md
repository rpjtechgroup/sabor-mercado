# Padrões de Dados e Persistência

## Servidor (PostgreSQL + EF Core)

### Organização
- Um schema por módulo: `identity`, `rewards`, `shared_catalog`,
  `recognition` (ADR-0001/0003). Proibido FK entre schemas — integração por
  IDs + eventos.
- Um `DbContext` por módulo, registrado em `AddXxxModule(...)` com
  `AddDbContextPool` (pool ≤ 32 na Fase 1).

### Convenções de schema
- Tabelas e colunas em `snake_case` (`UseSnakeCaseNamingConvention`).
- PK: `id UUID` gerado pela aplicação (`Guid.CreateVersion7()` — ordenável,
  amigável a índices).
- Auditoria mínima: `created_at timestamptz` em tudo; `updated_at` onde há
  mutação.
- Dinheiro: `numeric(10,2)`. Datas civis: `date`. Timestamps: `timestamptz`.
- Atributos flexíveis de produto: coluna `attributes jsonb` com índice GIN
  quando consultada.
- Ledger de créditos é **append-only**: sem UPDATE/DELETE em
  `credit_ledger_entries`; saldo é agregação.

### Migrations
- EF Core Migrations versionadas no repositório, uma pasta por módulo.
- Nome: `YYYYMMDD_DescricaoCurta`. Nunca editar migration aplicada.
- Deploy via `dotnet ef migrations bundle` antes do restart da API
  (`mvp-infrastructure.md`).

## Cliente (IndexedDB / localStorage)

- **IndexedDB** (via abstração em `Storage/`): stores `shoppingSessions`,
  `cartItems`, `products`, `priceRecords`, `pendingShares` (fila de
  contribuições aguardando rede).
- **localStorage**: somente preferências (`budgetDefault`, `geminiApiKey`,
  tema, flags de onboarding). Proibido dado de domínio em localStorage (limite
  de 5MB e serialização síncrona). A chave Gemini é credencial do usuário —
  nunca sincronizada nem enviada ao backend.
- Todo objeto persistido carrega `schemaVersion`; migrações de schema do
  cliente são funções puras versionadas em `Storage/Migrations/` e rodam na
  inicialização.
- Exportação/backup: o usuário pode exportar tudo em JSON a qualquer momento
  (dado é dele — Constitution IV).

## Anonimização (share-to-unlock)

- A projeção compartilhada contém somente os campos da tabela em
  `docs/business/share-to-unlock.md`.
- `contributorPseudonymId` não é reversível para e-mail fora do módulo
  Identity.
- Validação no servidor rejeita payloads com campos extras
  (`UnmappedMemberHandling.Disallow`).
