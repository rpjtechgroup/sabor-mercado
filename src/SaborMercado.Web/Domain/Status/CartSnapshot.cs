namespace SaborMercado.Web.Domain.Status;

public sealed record CartSnapshot(
    decimal Total,
    int DistinctItemCount,
    decimal? BudgetAmount,
    DateTimeOffset SessionStartedAt,
    int? PlannedListSize = null,
    TimeSpan? AverageSessionDuration = null)
{
    public static readonly TimeSpan DefaultAverageSessionDuration = TimeSpan.FromMinutes(40);

    public TimeSpan EffectiveAverageSessionDuration =>
        AverageSessionDuration is { } avg && avg > TimeSpan.Zero
            ? avg
            : DefaultAverageSessionDuration;

    
    public decimal PercentUsed =>
        BudgetAmount is { } budget && budget > 0m ? Total / budget * 100m : 0m;
}
