using SaborMercado.Web.Domain.Gamification;
using SaborMercado.Web.Interop;

namespace SaborMercado.Web.Storage;

public sealed class IndexedDbMetricsStore(IndexedDbInterop indexedDb) : IMetricsStore
{
    public async Task<LocalGamificationMetrics> GetOrCreateMetricsAsync()
    {
        var existing = await indexedDb.GetAsync<LocalGamificationMetrics>(
            StorageSchema.GamificationMetricsStore,
            LocalGamificationMetrics.SingletonId);

        if (existing is not null)
        {
            return existing;
        }

        var metrics = new LocalGamificationMetrics();
        await SaveMetricsAsync(metrics);
        return metrics;
    }

    public Task SaveMetricsAsync(LocalGamificationMetrics metrics) =>
        indexedDb.PutAsync(StorageSchema.GamificationMetricsStore, metrics).AsTask();

    public async Task IncrementProductCountAsync()
    {
        var metrics = await GetOrCreateMetricsAsync();
        metrics.TotalProductsRegistered++;
        await SaveMetricsAsync(metrics);
    }

    public async Task IncrementStoreCountAsync()
    {
        var metrics = await GetOrCreateMetricsAsync();
        metrics.TotalStoresRegistered++;
        await SaveMetricsAsync(metrics);
    }

    public async Task IncrementPurchaseCountAsync(bool budgetRespected)
    {
        var metrics = await GetOrCreateMetricsAsync();
        metrics.TotalPurchasesCompleted++;
        if (budgetRespected)
        {
            metrics.TotalPurchasesWithBudgetOk++;
        }

        await SaveMetricsAsync(metrics);
    }

    public async Task IncrementOcrCountAsync()
    {
        var metrics = await GetOrCreateMetricsAsync();
        metrics.TotalOcrItemsAdded++;
        await SaveMetricsAsync(metrics);
    }

    public async Task SetProductsWithPriceHistoryCountAsync(int count)
    {
        var metrics = await GetOrCreateMetricsAsync();
        metrics.TotalProductsWithPriceHistory = Math.Max(metrics.TotalProductsWithPriceHistory, count);
        await SaveMetricsAsync(metrics);
    }

    public async Task UpdateLoginStreakAsync(DateTimeOffset loginAt)
    {
        var metrics = await GetOrCreateMetricsAsync();
        var loginDay = DateOnly.FromDateTime(loginAt.UtcDateTime);

        if (metrics.LastLoginAt is { } lastLogin)
        {
            var lastDay = DateOnly.FromDateTime(lastLogin.UtcDateTime);
            if (lastDay == loginDay)
            {
                metrics.LastLoginAt = loginAt;
                await SaveMetricsAsync(metrics);
                return;
            }

            metrics.CurrentLoginStreakDays = lastDay.AddDays(1) == loginDay
                ? metrics.CurrentLoginStreakDays + 1
                : 1;
        }
        else
        {
            metrics.CurrentLoginStreakDays = 1;
        }

        metrics.LastLoginAt = loginAt;
        metrics.LongestLoginStreakDays = Math.Max(
            metrics.LongestLoginStreakDays,
            metrics.CurrentLoginStreakDays);
        await SaveMetricsAsync(metrics);
    }
}
