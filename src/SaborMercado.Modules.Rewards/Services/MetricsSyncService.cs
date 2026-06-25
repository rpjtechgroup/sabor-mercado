using Microsoft.EntityFrameworkCore;
using SaborMercado.Modules.Rewards.Data;
using SaborMercado.Shared.Community;
using SaborMercado.Shared.Rewards;

namespace SaborMercado.Modules.Rewards.Services;

public sealed class MetricsSyncService(
    RewardsDbContext db,
    IAchievementService achievements,
    RankingService rankings,
    TimeProvider clock)
{
    private const int MaxIncrementPerSync = 25;

    public async Task<SyncMetricsResponse> SyncAsync(
        Guid userId,
        SyncMetricsRequest request,
        CancellationToken cancellationToken)
    {
        ValidateMetrics(request.Metrics);

        var now = clock.GetUtcNow();
        var existing = await db.UserGamificationMetrics
            .FirstOrDefaultAsync(m => m.UserId == userId, cancellationToken);

        if (existing is null)
        {
            existing = new UserGamificationMetrics
            {
                Id = Guid.NewGuid(),
                UserId = userId,
            };
            db.UserGamificationMetrics.Add(existing);
        }
        else
        {
            ValidateIncrement(existing, request.Metrics);
        }

        existing.TotalProductsRegistered = request.Metrics.TotalProductsRegistered;
        existing.TotalStoresRegistered = request.Metrics.TotalStoresRegistered;
        existing.TotalPurchasesCompleted = request.Metrics.TotalPurchasesCompleted;
        existing.TotalPurchasesWithBudgetOk = request.Metrics.TotalPurchasesWithBudgetOk;
        existing.TotalOcrItemsAdded = request.Metrics.TotalOcrItemsAdded;
        existing.TotalProductsWithPriceHistory = request.Metrics.TotalProductsWithPriceHistory;
        existing.CurrentLoginStreakDays = request.Metrics.CurrentLoginStreakDays;
        existing.LongestLoginStreakDays = Math.Max(
            existing.LongestLoginStreakDays,
            request.Metrics.LongestLoginStreakDays);
        existing.LastLoginAt = request.Metrics.LastLoginAt;
        existing.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        var metricValues = new UserMetricsValues(
            existing.TotalProductsRegistered,
            existing.TotalStoresRegistered,
            existing.TotalPurchasesCompleted,
            existing.TotalPurchasesWithBudgetOk,
            existing.TotalOcrItemsAdded,
            existing.TotalProductsWithPriceHistory,
            existing.CurrentLoginStreakDays);

        var newAchievements = await achievements.EvaluateAfterMetricsSyncAsync(
            userId,
            metricValues,
            cancellationToken);

        await rankings.RefreshForUserAsync(userId, existing, cancellationToken);

        return new SyncMetricsResponse(true, newAchievements.Items.Count > 0 ? newAchievements : null);
    }

    public async Task<UserGamificationMetrics?> GetMetricsAsync(Guid userId, CancellationToken cancellationToken) =>
        await db.UserGamificationMetrics
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.UserId == userId, cancellationToken);

    private static void ValidateMetrics(UserMetricsSnapshotDto metrics)
    {
        if (metrics.TotalProductsRegistered < 0 ||
            metrics.TotalStoresRegistered < 0 ||
            metrics.TotalPurchasesCompleted < 0 ||
            metrics.TotalPurchasesWithBudgetOk < 0 ||
            metrics.TotalOcrItemsAdded < 0 ||
            metrics.TotalProductsWithPriceHistory < 0 ||
            metrics.CurrentLoginStreakDays < 0 ||
            metrics.LongestLoginStreakDays < 0)
        {
            throw new RewardsException(RewardsErrorCodes.InvalidMetrics, "Métricas inválidas.");
        }

        if (metrics.TotalPurchasesWithBudgetOk > metrics.TotalPurchasesCompleted)
        {
            throw new RewardsException(
                RewardsErrorCodes.InvalidMetrics,
                "Compras com orçamento respeitado não pode exceder o total de compras.");
        }
    }

    private static void ValidateIncrement(UserGamificationMetrics existing, UserMetricsSnapshotDto incoming)
    {
        ValidateFieldIncrement(
            existing.TotalProductsRegistered,
            incoming.TotalProductsRegistered,
            "produtos");
        ValidateFieldIncrement(
            existing.TotalStoresRegistered,
            incoming.TotalStoresRegistered,
            "comércios");
        ValidateFieldIncrement(
            existing.TotalPurchasesCompleted,
            incoming.TotalPurchasesCompleted,
            "compras");
        ValidateFieldIncrement(
            existing.TotalPurchasesWithBudgetOk,
            incoming.TotalPurchasesWithBudgetOk,
            "compras com orçamento");
        ValidateFieldIncrement(
            existing.TotalOcrItemsAdded,
            incoming.TotalOcrItemsAdded,
            "itens OCR");
        ValidateFieldIncrement(
            existing.TotalProductsWithPriceHistory,
            incoming.TotalProductsWithPriceHistory,
            "históricos de preço");
    }

    private static void ValidateFieldIncrement(int previous, int current, string label)
    {
        if (current < previous)
        {
            return;
        }

        if (current - previous > MaxIncrementPerSync)
        {
            throw new RewardsException(
                RewardsErrorCodes.InvalidMetrics,
                $"Incremento de {label} excede o limite por sincronização.");
        }
    }
}
