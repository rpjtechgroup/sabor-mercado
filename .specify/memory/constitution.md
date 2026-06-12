# Sabor Mercado — Constitution

Princípios inegociáveis que governam toda especificação, plano e implementação
deste projeto. Qualquer artefato (spec, plan, task, código, doc) que conflite
com esta Constitution está errado e deve ser corrigido.

## Core Principles

### I. Offline-First, Grátis por Padrão
O PWA funciona 100% sem backend e sem custo para o usuário. Todos os dados do
usuário (produtos, carrinhos, orçamentos, histórico) vivem primeiro no
dispositivo (localStorage/IndexedDB). O backend é opcional e agrega valor
(OCR, sincronização, catálogo colaborativo), nunca é pré-requisito para o
fluxo básico de compra. Nenhuma feature do fluxo principal pode quebrar quando
não há conectividade ou quando o servidor está fora do ar.

### II. Degradação Graciosa da IA (OCR)
A leitura por foto usa os modelos gratuitos do Google (Gemini). A IA é um
acelerador, não uma dependência: toda operação assistida por IA DEVE ter um
caminho manual equivalente. Se a chamada falhar (quota, rede, erro), o app
abre o formulário manual pré-preenchido com o que foi possível extrair, sem
perder o contexto do usuário. A chave da API Gemini é **opcional**, fornecida
pelo próprio usuário, armazenada somente em localStorage no dispositivo e usada
para chamar o Google diretamente do PWA — nunca é enviada ao nosso backend.
Sem chave, o modo foto cai no cadastro manual.

### III. Mensagens de Status Determinísticas
Alertas de orçamento, projeções de gasto e avisos ("seu orçamento foi
ultrapassado em R$ X") são gerados por regras pré-estabelecidas no sistema
(catálogo em `docs/domain/status-messages.md`), nunca por LLM em tempo real.
Cálculo no cliente, instantâneo, auditável e gratuito.

### IV. Compartilhe-para-Desbloquear (Share-to-Unlock)
O modelo de negócio segue a estrutura "passeidireto": dados pessoais são do
usuário e ficam locais; ao compartilhar dados anonimizados de produtos/preços
com o catálogo colaborativo, o usuário ganha créditos que desbloqueiam
funcionalidades premium. Nenhum dado é compartilhado sem ação explícita do
usuário (opt-in por envio). Dados compartilhados são sempre anonimizados.

### V. Caber no MVP, Projetar para 10k+
A implementação inicial DEVE rodar na VM OCI de 1GB RAM + 1GB swap (monólito
modular, orçamento de memória documentado). Porém, todo módulo respeita as
fronteiras de contexto definidas em `docs/domain/domain-model.md` para que a
extração futura para serviços distribuídos (plano em
`docs/architecture/scale-migration-plan.md`) não exija reescrita. Proibido
acoplamento entre módulos fora dos contratos definidos.

### VI. C#/.NET de Ponta a Ponta
Backend em ASP.NET Core (.NET LTS atual) e frontend em Blazor WebAssembly PWA.
Persistência via EF Core. Decisões de stack só mudam por ADR aprovado em
`docs/architecture/adr/`. Padrões de código em `.cursor/rules/` e
`docs/standards/` são obrigatórios.

### VII. Spec-Driven Development
Nenhuma feature é implementada sem passar pelo fluxo spec-kit:
`/speckit-specify` → `/speckit-plan` → `/speckit-tasks` → `/speckit-implement`.
Specs vivem em `specs/`, documentação de referência em `docs/`. Toda decisão
arquitetural relevante gera um ADR. Specs descrevem comportamento e regras de
negócio em PT-BR; identificadores de código em inglês.

## Restrições Técnicas

- **MVP (Fase 1)**: 1 VM OCI (1GB RAM + 1GB swap) com Caddy + API ASP.NET Core
  + PostgreSQL (JSONB para atributos flexíveis). Cache via `IMemoryCache`.
  Sem Redis, sem MongoDB nesta fase. Orçamento de memória por processo em
  `docs/architecture/mvp-infrastructure.md` é limite rígido.
- **Escala (Fase 2+)**: PostgreSQL gerenciado + Redis (cache/rate-limit) +
  MongoDB (catálogo colaborativo), conforme
  `docs/architecture/scale-migration-plan.md`.
- **Privacidade**: dados pessoais nunca saem do dispositivo sem opt-in;
  payloads compartilhados não contêm identificadores pessoais.
- **Idioma**: UI e documentação de negócio em PT-BR; código, commits e
  identificadores em inglês.

## Workflow de Desenvolvimento

- Specs e planos referenciam os documentos canônicos de `docs/` em vez de
  redefinir regras (fonte única de verdade).
- Toda task de implementação inclui: testes (xUnit), verificação de lint e
  conformidade com o orçamento de memória quando tocar o backend.
- Mudanças de schema sempre via EF Core Migrations versionadas.
- PRs devem declarar qual spec/ADR cobrem; mudanças sem spec são rejeitadas,
  exceto correções triviais.

## Governance

Esta Constitution prevalece sobre qualquer outra prática. Emendas exigem:
(1) justificativa documentada, (2) atualização dos docs impactados,
(3) incremento de versão abaixo. Revisões de código e de specs DEVEM verificar
conformidade com os princípios I–VII.

**Version**: 1.0.0 | **Ratified**: 2026-06-11 | **Last Amended**: 2026-06-11
