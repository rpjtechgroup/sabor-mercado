namespace SaborMercado.Web.Contracts.Rewards;

public static class PremiumFeatureCodes
{
    public const string CollaborativePriceHistory = "collaborative-price-history";
    public const string MarketComparison = "market-comparison";
    public const string ExportCsv = "export-csv";
    public const string SmartLists = "smart-lists";
    public const string AdvancedStats = "advanced-stats";
}

public sealed record CreditsResponse(
    int Balance,
    IReadOnlyList<CreditLedgerEntryDto> RecentEntries,
    IReadOnlyList<FeatureUnlockDto> ActiveUnlocks);

public sealed record CreditLedgerEntryDto(
    int Amount,
    string Reason,
    DateTimeOffset CreatedAt);

public sealed record FeatureUnlockDto(
    string FeatureCode,
    DateTimeOffset UnlockedAt,
    DateTimeOffset? ExpiresAt);

public sealed record UnlockRequest(string FeatureCode);

public sealed record UnlockResponse(
    string FeatureCode,
    int NewBalance,
    DateTimeOffset? ExpiresAt);

public sealed record UnlockOffer(
    string Code,
    string Label,
    int Cost,
    string DurationLabel,
    bool IsAvailable);

public static class UnlockOffers
{
    public static readonly IReadOnlyList<UnlockOffer> All =
    [
        new(PremiumFeatureCodes.CollaborativePriceHistory, "Histórico colaborativo de preços", 10, "30 dias", true),
        new(PremiumFeatureCodes.ExportCsv, "Exportar compras (CSV)", 15, "permanente", true),
        new(PremiumFeatureCodes.MarketComparison, "Comparação entre mercados", 20, "30 dias", true),
        new(PremiumFeatureCodes.SmartLists, "Listas inteligentes", 30, "30 dias", true),
        new(PremiumFeatureCodes.AdvancedStats, "Estatísticas avançadas", 25, "30 dias", true),
    ];
}
