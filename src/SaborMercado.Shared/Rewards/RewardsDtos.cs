namespace SaborMercado.Shared.Rewards;

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
