using SaborMercado.Shared.Community;

namespace SaborMercado.Shared.Rewards;

public sealed record UserMetricsSnapshotDto(
    int TotalProductsRegistered,
    int TotalStoresRegistered,
    int TotalPurchasesCompleted,
    int TotalPurchasesWithBudgetOk,
    int TotalOcrItemsAdded,
    int TotalProductsWithPriceHistory,
    int CurrentLoginStreakDays,
    int LongestLoginStreakDays,
    DateTimeOffset? LastLoginAt,
    DateTimeOffset CapturedAt);

public sealed record SyncMetricsRequest(UserMetricsSnapshotDto Metrics);

public sealed record SyncMetricsResponse(bool Accepted, AchievementListResponse? NewAchievements);

public sealed record AchievementProgressDto(
    string Code,
    string Title,
    string Description,
    bool IsUnlocked,
    DateTimeOffset? UnlockedAt,
    int? CurrentProgress,
    int? TargetProgress);

public sealed record AchievementCatalogResponse(
    IReadOnlyList<AchievementProgressDto> Items,
    int UnlockedCount,
    int TotalCount);
