using SaborMercado.Shared.Community;
using SaborMercado.Shared.Rewards;
using SaborMercado.Web.Domain.Gamification;
using SaborMercado.Web.Infrastructure;
using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Features.Gamification;

public sealed class ClientGamificationSyncService(
    SaborMercadoApiClient api,
    GamificationMetricsService metrics,
    IMetricsStore metricsStore,
    AchievementNotificationService notifications,
    TimeProvider clock)
{
    private readonly TimeSpan _syncInterval = TimeSpan.FromMinutes(5);
    private DateTimeOffset _lastSyncAttempt;
    private bool _syncInProgress;

    public async Task TrySyncAsync(bool force = false)
    {
        if (_syncInProgress)
        {
            return;
        }

        var now = clock.GetUtcNow();
        if (!force && now - _lastSyncAttempt < _syncInterval)
        {
            return;
        }

        _lastSyncAttempt = now;
        _syncInProgress = true;

        try
        {
            await metrics.RecomputeDerivedCountsAsync();
            var current = await metricsStore.GetOrCreateMetricsAsync();
            var snapshot = new UserMetricsSnapshotDto(
                current.TotalProductsRegistered,
                current.TotalStoresRegistered,
                current.TotalPurchasesCompleted,
                current.TotalPurchasesWithBudgetOk,
                current.TotalOcrItemsAdded,
                current.TotalProductsWithPriceHistory,
                current.CurrentLoginStreakDays,
                current.LongestLoginStreakDays,
                current.LastLoginAt,
                now);

            var response = await api.SendAsync(
                HttpMethod.Post,
                "/api/v1/metrics/sync",
                new SyncMetricsRequest(snapshot),
                requireAuth: true);

            if (!response.IsSuccessStatusCode)
            {
                return;
            }

            var result = await api.ReadJsonAsync<SyncMetricsResponse>(response, CancellationToken.None);
            if (result is null)
            {
                return;
            }

            current.LastSyncedAt = now;
            await metricsStore.SaveMetricsAsync(current);

            if (result.NewAchievements?.Items.Count > 0)
            {
                notifications.NotifyNewAchievements(result.NewAchievements.Items);
            }
        }
        finally
        {
            _syncInProgress = false;
        }
    }
}
