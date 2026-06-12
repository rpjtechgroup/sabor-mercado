# Contrato — Avaliador de Mensagens de Status

> Interface pública mais crítica da feature (Constitution III). Não há API
> HTTP nesta feature; o contrato exposto é a função pura do domínio usada
> pela UI e pelos testes. Fonte canônica das regras:
> [`docs/domain/status-messages.md`](../../../docs/domain/status-messages.md).

## Assinatura

```csharp
namespace SaborMercado.Web.Domain.Status;

public static class StatusMessageEvaluator
{
    public static EvaluationResult Evaluate(EvaluationInput input);
}

public sealed record EvaluationInput(
    BudgetAlertState Before,      // estado anterior (da sessão)
    CartSnapshot Cart,            // T, n, B, início da sessão...
    CartMutation Mutation,        // o que acabou de acontecer
    DateTimeOffset Now);          // relógio injetado (cooldown)

public sealed record EvaluationResult(
    StatusMessage? Message,       // no máx. 1 por mutação; null = silêncio
    BudgetAlertState After);      // novo estado a persistir na sessão
```

```csharp
public sealed record CartSnapshot(
    decimal Total,                       // T
    int DistinctItemCount,               // n
    decimal? BudgetAmount,               // B (null = sem meta)
    DateTimeOffset SessionStartedAt,
    int? PlannedListSize,                // N (sempre null nesta feature)
    TimeSpan AverageSessionDuration);    // default 40 min

public sealed record StatusMessage(
    string Code,                                   // código estável do catálogo
    IReadOnlyDictionary<string, string> Args);     // placeholders já formatados (pt-BR)
```

## Garantias do contrato

1. **Pureza**: sem I/O, sem relógio interno, sem estado estático. Mesmo
   input ⇒ mesmo output.
2. **Catálogo fechado**: `Message.Code` ∈ códigos da tabela do catálogo.
   Nenhum código novo pode ser emitido sem atualizar o catálogo antes.
3. **Máx. 1 mensagem por mutação**, escolhida pela ordem de prioridade do
   catálogo: `BUDGET_EXCEEDED` > `BUDGET_REACHED` > `BUDGET_HIGH_90` >
   `BUDGET_WARN_75` > `BUDGET_HALF` > `PACE_*` > `ITEM_*`.
4. **Emissão única + rearme**: códigos de cruzamento disparam 1× por sessão;
   queda de `P` abaixo do limiar rearma o código (regra nº 1).
5. **Cooldown `PACE_*`**: 5 itens **ou** 5 minutos desde a última emissão
   `PACE_*` (regra nº 2).
6. **Sem meta (`B = null`)**: somente `ITEM_*`, `OCR_UNAVAILABLE`,
   `SESSION_FINISHED` podem ser emitidos (regra nº 4).
7. **Faixa da barra**: `BudgetRanges.FromPercent(decimal p)` →
   `budget-ok` / `budget-warn` / `budget-high` / `budget-over`, com os
   limites exatos do catálogo (60%, 85%, 100%).
8. **Projeção `E`**: regra 1 (lista planejada, `n ≥ 3`) e regra 2 (ritmo
   temporal, `n ≥ 5`, teto `3 × T`); caso contrário `E` indisponível.
9. **Args formatados**: valores monetários chegam prontos para exibição
   (`R$ 8,99`, cultura pt-BR, 2 casas).

## Códigos emissíveis nesta feature

`BUDGET_SET`, `BUDGET_HALF`, `BUDGET_WARN_75`, `BUDGET_HIGH_90`,
`BUDGET_REACHED`, `BUDGET_EXCEEDED`, `PACE_PROJECTION_OVER`,
`PACE_PROJECTION_OK`, `SESSION_FINISHED`.

`ITEM_ADDED_OCR`, `ITEM_ADDED_OCR_REVIEW`, `OCR_UNAVAILABLE` são constantes
declaradas (catálogo completo) mas nenhum fluxo desta feature os emite
(gatilhos pertencem ao fluxo F1 — feature futura).

## Consumo pela UI

- `ShoppingService` chama `Evaluate` a cada mutação, persiste `After` na
  sessão e publica `Message` (se houver) para o `StatusBanner`.
- Texto exibido = `IStringLocalizer`/`StatusMessages.resx[Code]` com
  substituição dos `Args`. Proibido texto hardcoded em componente.
