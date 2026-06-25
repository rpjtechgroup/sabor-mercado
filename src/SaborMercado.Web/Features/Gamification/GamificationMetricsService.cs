using SaborMercado.Shared.Community;
using SaborMercado.Web.Domain.Gamification;
using SaborMercado.Web.Domain.Shopping;
using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Features.Gamification;

public sealed class GamificationMetricsService(
    IMetricsStore metricsStore,
    ICatalogStore catalogStore,
    IStoreStore storeStore,
    IShoppingStore shoppingStore,
    TimeProvider clock)
{
    public event Action? StateChanged;

    public LocalGamificationMetrics? Current { get; private set; }

    public async Task InitializeAsync()
    {
        Current = await metricsStore.GetOrCreateMetricsAsync();
        await RecomputeDerivedCountsAsync();
        Notify();
    }

    public async Task IncrementProductCountAsync()
    {
        await metricsStore.IncrementProductCountAsync();
        Current = await metricsStore.GetOrCreateMetricsAsync();
        Notify();
    }

    public async Task IncrementStoreCountAsync()
    {
        await metricsStore.IncrementStoreCountAsync();
        Current = await metricsStore.GetOrCreateMetricsAsync();
        Notify();
    }

    public async Task IncrementPurchaseCountAsync(bool budgetRespected)
    {
        await metricsStore.IncrementPurchaseCountAsync(budgetRespected);
        Current = await metricsStore.GetOrCreateMetricsAsync();
        Notify();
    }

    public async Task IncrementOcrCountAsync()
    {
        await metricsStore.IncrementOcrCountAsync();
        Current = await metricsStore.GetOrCreateMetricsAsync();
        Notify();
    }

    public async Task UpdateLoginStreakAsync()
    {
        await metricsStore.UpdateLoginStreakAsync(clock.GetUtcNow());
        Current = await metricsStore.GetOrCreateMetricsAsync();
        Notify();
    }

    public async Task RecomputeDerivedCountsAsync()
    {
        var metrics = await metricsStore.GetOrCreateMetricsAsync();
        var products = await catalogStore.GetAllProductsAsync();
        var stores = await storeStore.GetAllStoresAsync();
        var sessions = await shoppingStore.GetAllSessionsAsync();

        metrics.TotalProductsRegistered = Math.Max(metrics.TotalProductsRegistered, products.Count);
        metrics.TotalStoresRegistered = Math.Max(metrics.TotalStoresRegistered, stores.Count);

        var finished = sessions
            .Where(s => s.Status == SessionStatus.Finished && s.FinishedAt is not null)
            .ToList();
        metrics.TotalPurchasesCompleted = Math.Max(metrics.TotalPurchasesCompleted, finished.Count);

        var budgetOk = 0;
        foreach (var session in finished)
        {
            if (session.BudgetAmount is not { } budget || budget <= 0m)
            {
                continue;
            }

            var items = await shoppingStore.GetItemsAsync(session.Id);
            var total = items.Sum(i => i.Subtotal);
            if (total <= budget)
            {
                budgetOk++;
            }
        }

        metrics.TotalPurchasesWithBudgetOk = Math.Max(metrics.TotalPurchasesWithBudgetOk, budgetOk);

        var productsWithHistory = 0;
        foreach (var product in products)
        {
            var records = await catalogStore.GetPriceRecordsAsync(product.Id);
            if (records.Count > 0)
            {
                productsWithHistory++;
            }
        }

        metrics.TotalProductsWithPriceHistory = Math.Max(
            metrics.TotalProductsWithPriceHistory,
            productsWithHistory);

        await metricsStore.SaveMetricsAsync(metrics);
        Current = metrics;
        Notify();
    }

    public UserMetricsValues ToMetricValues() =>
        Current is null
            ? new UserMetricsValues(0, 0, 0, 0, 0, 0, 0)
            : new UserMetricsValues(
                Current.TotalProductsRegistered,
                Current.TotalStoresRegistered,
                Current.TotalPurchasesCompleted,
                Current.TotalPurchasesWithBudgetOk,
                Current.TotalOcrItemsAdded,
                Current.TotalProductsWithPriceHistory,
                Current.CurrentLoginStreakDays);

    public IReadOnlyList<AchievementProgressView> BuildLocalAchievementProgress()
    {
        var values = ToMetricValues();
        return AchievementCodes.Catalog
            .Select(entry =>
            {
                var code = entry.Key;
                var (title, description) = entry.Value;
                int? current = null;
                int? target = null;

                if (AchievementCodes.MetricThresholds.TryGetValue(code, out var threshold))
                {
                    target = threshold.Threshold;
                    current = AchievementCodes.GetMetricValue(threshold.MetricKey, values);
                }

                return new AchievementProgressView(code, title, description, current, target);
            })
            .ToList();
    }

    private void Notify() => StateChanged?.Invoke();
}

public sealed record AchievementProgressView(
    string Code,
    string Title,
    string Description,
    int? CurrentProgress,
    int? TargetProgress);
