using SaborMercado.Web.Domain.Gamification;

namespace SaborMercado.Web.Storage;

public interface IMetricsStore
{
    Task<LocalGamificationMetrics> GetOrCreateMetricsAsync();

    Task SaveMetricsAsync(LocalGamificationMetrics metrics);

    Task IncrementProductCountAsync();

    Task IncrementStoreCountAsync();

    Task IncrementPurchaseCountAsync(bool budgetRespected);

    Task IncrementOcrCountAsync();

    Task SetProductsWithPriceHistoryCountAsync(int count);

    Task UpdateLoginStreakAsync(DateTimeOffset loginAt);
}
