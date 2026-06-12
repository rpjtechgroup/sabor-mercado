namespace SaborMercado.Web.Domain.Shopping;

public sealed record BudgetAlertState
{
    public HashSet<string> EmittedCodes { get; init; } = [];

    
    public decimal LastPercentUsed { get; init; }

    public DateTimeOffset? LastPaceEmissionAt { get; init; }

    public int? ItemCountAtLastPaceEmission { get; init; }

    public static BudgetAlertState Initial => new();
}
