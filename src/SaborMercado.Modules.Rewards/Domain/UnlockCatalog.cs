namespace SaborMercado.Modules.Rewards.Domain;

public static class UnlockCatalog
{
    public static readonly IReadOnlyDictionary<string, UnlockDefinition> Features =
        new Dictionary<string, UnlockDefinition>(StringComparer.Ordinal)
        {
            ["collaborative-price-history"] = new(10, TimeSpan.FromDays(30), true),
            ["market-comparison"] = new(20, TimeSpan.FromDays(30), true),
            ["export-csv"] = new(15, null, true),
            ["smart-lists"] = new(30, TimeSpan.FromDays(30), true),
            ["advanced-stats"] = new(25, TimeSpan.FromDays(30), true),
        };
}

public sealed record UnlockDefinition(int Cost, TimeSpan? Duration, bool IsAvailable);
