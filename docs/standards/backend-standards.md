# Padrões de Backend (C#/.NET)

> Obrigatório para todo código em `src/SaborMercado.Api` e
> `src/SaborMercado.Modules.*`. Reforçado pela rule
> `.cursor/rules/dotnet-standards.mdc`.

## Stack

- .NET LTS atual, ASP.NET Core **Minimal APIs** (sem Controllers).
- `Nullable` e `ImplicitUsings` habilitados; `TreatWarningsAsErrors=true`.
- `Directory.Build.props` centraliza versões e propriedades; pacotes via
  Central Package Management (`Directory.Packages.props`).

## Estrutura de módulo

Cada módulo (`SaborMercado.Modules.Xxx`) contém:

```
Endpoints/        # Mapeamento Minimal API (1 arquivo por recurso)
Domain/           # Entidades e regras (sem dependência de infra)
Data/             # DbContext, configurações EF, migrations do módulo
Services/         # Casos de uso (1 classe por operação relevante)
Contracts/        # DTOs públicos de entrada/saída
XxxModule.cs      # AddXxxModule(IServiceCollection) + MapXxxEndpoints(...)
```

Regras de fronteira (ADR-0003):
- Módulo não referencia outro módulo; integração via `SaborMercado.Shared`.
- Entidades de domínio nunca saem do módulo — endpoints retornam DTOs de
  `Contracts/`.

## Convenções de código

- Nomes em inglês; comentários e mensagens de UI em PT-BR via recursos.
- `decimal` para qualquer valor monetário; nunca `double`/`float`.
- `DateTimeOffset` (UTC) para timestamps; datas civis com `DateOnly`.
- Injeção por construtor; sem service locator; sem estado estático mutável.
- Resultados de operação com tipo `Result<T>`/erros de domínio explícitos;
  exceções só para condições excepcionais.
- Validação de entrada nos endpoints com FluentValidation (ou validação
  manual simples); falha → `400` com `ProblemDetails`.

## Memória (Constitution V / mvp-infrastructure.md)

- Proibido carregar coleções inteiras sem paginação (`Take` máximo 100).
- Upload de imagem por streaming; limite 4 MB no endpoint.
- `IMemoryCache` sempre com `Size` definido por entrada e `SizeLimit` global.
- `AsNoTracking()` por padrão em consultas de leitura.

## Logging e observabilidade

- `ILogger<T>` com message templates estruturados (nunca interpolação).
- Sem log de dados pessoais ou imagens; `RecognitionLog` guarda apenas
  metadados.
- Health checks: `/healthz` (liveness) e `/readyz` (Postgres).

## Testes

- xUnit. Unidade para domínio/serviços; integração com `WebApplicationFactory`
  + Testcontainers (PostgreSQL) para endpoints.
- Toda regra de negócio de specs (`specs/`) tem teste com nome rastreável à
  spec (ex.: `BudgetAlert_Crosses75Percent_EmitsBudgetWarn75`).
- Cobertura mínima dos módulos de domínio: 80%.
