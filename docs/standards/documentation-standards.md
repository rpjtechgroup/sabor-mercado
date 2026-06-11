# Padrões de Documentação e SDD

> Como escrever e manter specs e docs para que a estrutura seja construída de
> maneira uniforme (Constitution VII).

## Hierarquia de fontes de verdade

1. `.specify/memory/constitution.md` — princípios inegociáveis.
2. `docs/` — documentação canônica de negócio, domínio, arquitetura e padrões.
3. `specs/NNN-nome-da-feature/` — especificações por feature (geradas pelo
   spec-kit).
4. Código — implementa specs; nunca contradiz docs.

Conflito entre níveis = bug de documentação; corrigir o nível inferior ou
emendar o superior via processo de governança.

## Fluxo SDD (spec-kit)

Toda feature segue, nesta ordem:

| Passo | Skill                 | Artefato                              |
|-------|-----------------------|----------------------------------------|
| 1     | `/speckit-specify`    | `specs/NNN-feature/spec.md`            |
| 2     | `/speckit-clarify`    | (opcional) perguntas de desambiguação  |
| 3     | `/speckit-plan`       | `specs/NNN-feature/plan.md`            |
| 4     | `/speckit-tasks`      | `specs/NNN-feature/tasks.md`           |
| 5     | `/speckit-analyze`    | (opcional) verificação de consistência |
| 6     | `/speckit-implement`  | código + testes                        |

Regras:
- Specs **referenciam** docs canônicos (link relativo) em vez de copiar
  regras. Ex.: regras de alerta apontam para `docs/domain/status-messages.md`.
- Spec descreve **o quê** (comportamento, regras, critérios de aceite);
  plan descreve **como** (projetos, classes, migrations).
- Critérios de aceite em formato verificável (Given/When/Then ou tabela).
- Specs em PT-BR; identificadores de código em inglês.

## Convenções de escrita

- Markdown; títulos `#`/`##`/`###`; tabelas para regras enumeráveis;
  Mermaid para diagramas (nunca imagens binárias de diagrama).
- Valores monetários em exemplos: formato brasileiro (`R$ 8,99`).
- Todo doc novo entra no índice do `README.md` da raiz.
- ADRs: ver `docs/architecture/adr/README.md`.

## Definition of Done (documentação)

Um PR de feature só está completo quando:
1. Spec/plan/tasks existem e foram seguidos.
2. Docs canônicos impactados foram atualizados no mesmo PR (ex.: novo
   endpoint → tabela em `api-standards.md`; nova mensagem → catálogo em
   `status-messages.md`).
3. ADR criado se houve decisão arquitetural nova.
