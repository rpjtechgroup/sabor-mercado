# Research — 001 Núcleo Offline de Compras

> Phase 0 do plano. Todas as incertezas técnicas resolvidas; nenhuma
> NEEDS CLARIFICATION pendente.

## R1 — Versão do .NET

- **Decision**: .NET 8.0 (SDK 8.0.414 instalado).
- **Rationale**: Constitution VI exige ".NET LTS atual"; dos SDKs instalados
  (8.0.414, 9.0.313), o 8.0 é o LTS. O 9.0 é STS.
- **Alternatives considered**: .NET 9 (STS — viola a constituição); .NET 10
  (LTS mais novo, porém não instalado na máquina de desenvolvimento).

## R2 — Acesso ao IndexedDB a partir do Blazor WASM

- **Decision**: Módulo ES próprio (`wwwroot/js/indexedDb.js`, ~100 linhas)
  com API promise-based (`put`, `get`, `getAll`, `getByIndex`, `delete`,
  `clear`), carregado via `IJSRuntime.InvokeAsync<IJSObjectReference>("import", ...)`
  e envolto pelo wrapper tipado `Interop/IndexedDbInterop.cs`.
- **Rationale**: frontend-standards §6 exige JS somente em `Interop/` com
  wrapper tipado; um módulo fino evita dependência de pacote de terceiros
  (menos payload no PWA, controle total do schema/migrações exigidas pelo
  data-standards).
- **Alternatives considered**: pacotes NuGet (TG.Blazor.IndexedDB,
  Magic.IndexedDb) — abandonados/pesados/sem controle de `schemaVersion`;
  localStorage para entidades — proibido pelo data-standards.

## R3 — Schema do IndexedDB e migrações

- **Decision**: Banco `sabor-mercado`, versão 1, com stores
  `shoppingSessions` (key `id`, índice `status`), `cartItems` (key `id`,
  índice `sessionId`), `products` (key `id`), `priceRecords` (key `id`,
  índice `productId`). Upgrades de schema ficam centralizados na função
  `upgrade(db, oldVersion)` do módulo JS (criação de stores/índices) e
  migrações de dados são funções C# puras versionadas em
  `Storage/Migrations/` rodando na inicialização, dirigidas pelo campo
  `schemaVersion` de cada objeto (data-standards).
- **Rationale**: separa migração estrutural (só JS pode criar store) da
  migração de dados (testável em C#).
- **Alternatives considered**: criar já o store `pendingShares` do
  data-standards — rejeitado: IndexedDB versiona incrementalmente sem custo,
  e o store pertence à feature share-to-unlock (YAGNI; evita store morto).
- **Nota**: `BudgetAlertState` não ganha store próprio — é estado da sessão
  e é persistido embutido no objeto `shoppingSessions` (1:1 com a sessão,
  sempre lido/escrito junto).

## R4 — Forma do avaliador de mensagens de status

- **Decision**: registro imutável de entrada/saída:
  `StatusMessageEvaluator.Evaluate(EvaluationInput) → EvaluationResult`
  com `EvaluationInput = (BudgetAlertState before, CartSnapshot cart,
  CartMutation mutation, DateTimeOffset now)` e
  `EvaluationResult = (StatusMessage? Message, BudgetAlertState After)`.
  `CartSnapshot` carrega `Total`, `DistinctItemCount`, `Budget?`,
  `SessionStartedAt`, `PlannedListSize?` (sempre `null` nesta feature) e
  `AverageSessionDuration` (default 40 min). Percentual `P` calculado com
  `decimal`. O chamador injeta `now` (testabilidade total, sem relógio
  interno).
- **Rationale**: status-messages.md exige avaliador puro
  `(estadoAnterior, carrinho, orçamento) → mensagem?`; devolver o novo
  estado torna emissão única/rearme/cooldown persistíveis e auditáveis.
- **Alternatives considered**: serviço com estado interno — rejeitado
  (impuro, difícil de testar); eventos múltiplos por mutação — proibido
  (máx. 1 mensagem por mutação).

## R5 — Cruzamento de faixa e rearme

- **Decision**: gatilho de cruzamento = `P_before < limiar ≤ P_after`,
  onde `P_before` vem do snapshot anterior guardado em `BudgetAlertState`
  (`LastPercentUsed`). Emissão única por código controlada por set
  `EmittedCodes`; remoção que faz `P` cair abaixo do limiar remove o código
  do set (rearme, regra de emissão nº 1). `BUDGET_EXCEEDED` dispara a cada
  adição de item enquanto `T > B` (texto do catálogo: "a cada novo item
  enquanto estourado") e não entra no set de emissão única; `BUDGET_REACHED`
  cobre o cruzamento de 100%.
- **Rationale**: leitura literal da tabela de gatilhos + regras de emissão.
- **Alternatives considered**: `BUDGET_EXCEEDED` em qualquer mutação
  (edição/remoção) — rejeitado: o gatilho diz "a cada novo item".

## R6 — Cooldown de PACE_*

- **Decision**: `BudgetAlertState` guarda `LastPaceEmissionAt` e
  `ItemCountAtLastPaceEmission`. Nova emissão `PACE_*` só se
  `now − LastPaceEmissionAt ≥ 5 min` **ou** `n − ItemCountAtLastPaceEmission ≥ 5`
  (catálogo: "cooldown de 5 itens ou 5 minutos" — qualquer um dos dois
  satisfeito libera).
- **Rationale**: interpretação mais natural de "X ou Y entre emissões".

## R7 — i18n dos textos de status

- **Decision**: `Resources/StatusMessages.resx` (cultura neutra = pt-BR,
  default do app) com chave = código do catálogo e placeholders nomeados
  (`{B}`, `{R}`, `{T}`, `{E}`, `{n}`, `{product}`, `{excess}`).
  `SESSION_FINISHED` usa três chaves (`SESSION_FINISHED`,
  `SESSION_FINISHED_UNDER`, `SESSION_FINISHED_OVER`) para as variações
  econômicas do texto — mesmo código `SESSION_FINISHED` no contrato, apenas
  o recurso varia (catálogo permite refinar texto sem mudar lógica).
  Formatação monetária aplicada antes da substituição (`R$ 8,99`).
- **Rationale**: frontend-standards §3 (proibido hardcode em componente);
  catálogo define que o código é o contrato e o texto é recurso.
- **Alternatives considered**: JSON i18n custom — `.resx` é o caminho padrão
  Blazor com `IStringLocalizer`, sem código extra.

## R8 — Testes

- **Decision**: xUnit 2.x + bUnit 1.x (compatíveis com .NET 8) num único
  projeto `tests/SaborMercado.Web.Tests`. Nomes rastreáveis à spec
  (ex.: `BudgetAlert_Crosses75Percent_EmitsBudgetWarn75`). Cobertura
  prioritária: todos os códigos do catálogo, cruzamentos, prioridade,
  emissão única, rearme, cooldown, sessão sem meta; cálculos de carrinho
  (subtotal, total, +1/+5/digitar).
- **Rationale**: frontend-standards §Testes; backend-standards (nomes
  rastreáveis) aplicado por analogia.

## R9 — PWA

- **Decision**: template oficial `dotnet new blazorwasm --pwa` (manifest +
  `service-worker.js`/`service-worker.published.js` cache-first padrão).
  Ajustes: nome/cores PT-BR no manifest. Estratégia network-only para
  `/api` (frontend-standards) já é irrelevante aqui (sem chamadas), fica o
  default do template.
- **Rationale**: requisito explícito da feature; menor manutenção.

## R10 — Persistência imediata sem travar UI

- **Decision**: toda mutação atualiza o estado em memória primeiro
  (recálculo síncrono < 100 ms), depois `await` da escrita IndexedDB na
  mesma operação (interop é async por natureza). Falha de escrita exibe
  aviso e mantém o app operando em memória (edge case da spec).
- **Rationale**: data-standards "toda escrita é imediata"; UX instantânea.
