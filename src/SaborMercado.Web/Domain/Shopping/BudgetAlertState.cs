namespace SaborMercado.Web.Domain.Shopping;

/// <summary>
/// Estado de emissão de alertas da sessão (regras de emissão de
/// docs/domain/status-messages.md): emissão única por código com rearme,
/// detecção de cruzamento de limiar e cooldown das projeções PACE_*.
/// </summary>
public sealed record BudgetAlertState
{
    public HashSet<string> EmittedCodes { get; init; } = [];

    /// <summary>Percentual utilizado (0–100+) após a última mutação.</summary>
    public decimal LastPercentUsed { get; init; }

    public DateTimeOffset? LastPaceEmissionAt { get; init; }

    public int? ItemCountAtLastPaceEmission { get; init; }

    public static BudgetAlertState Initial => new();
}
