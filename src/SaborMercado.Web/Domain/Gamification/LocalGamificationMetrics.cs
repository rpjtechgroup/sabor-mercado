using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Domain.Gamification;

public sealed class LocalGamificationMetrics
{
    public static readonly Guid SingletonId = Guid.Parse("00000000-0000-4000-8000-000000000010");

    public Guid Id { get; set; } = SingletonId;

    public int TotalProductsRegistered { get; set; }

    public int TotalStoresRegistered { get; set; }

    public int TotalPurchasesCompleted { get; set; }

    public int TotalPurchasesWithBudgetOk { get; set; }

    public int TotalOcrItemsAdded { get; set; }

    public int TotalProductsWithPriceHistory { get; set; }

    public int CurrentLoginStreakDays { get; set; }

    public int LongestLoginStreakDays { get; set; }

    public DateTimeOffset? LastLoginAt { get; set; }

    public DateTimeOffset LastSyncedAt { get; set; }

    public int SchemaVersion { get; set; } = StorageSchema.CurrentSchemaVersion;
}
