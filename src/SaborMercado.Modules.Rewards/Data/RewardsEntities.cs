namespace SaborMercado.Modules.Rewards.Data;

public sealed class UserAchievement
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string AchievementCode { get; set; } = string.Empty;

    public DateTimeOffset UnlockedAt { get; set; }
}

public sealed class UserGamificationMetrics
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public int TotalProductsRegistered { get; set; }

    public int TotalStoresRegistered { get; set; }

    public int TotalPurchasesCompleted { get; set; }

    public int TotalPurchasesWithBudgetOk { get; set; }

    public int TotalOcrItemsAdded { get; set; }

    public int TotalProductsWithPriceHistory { get; set; }

    public int CurrentLoginStreakDays { get; set; }

    public int LongestLoginStreakDays { get; set; }

    public DateTimeOffset? LastLoginAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class RankingSnapshot
{
    public Guid Id { get; set; }

    public string RankingType { get; set; } = string.Empty;

    public Guid UserId { get; set; }

    public string PseudonymDisplay { get; set; } = string.Empty;

    public int RankPosition { get; set; }

    public int Score { get; set; }

    public DateTimeOffset CalculatedAt { get; set; }
}
