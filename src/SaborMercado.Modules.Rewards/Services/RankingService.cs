using Microsoft.EntityFrameworkCore;
using SaborMercado.Modules.Rewards.Data;
using SaborMercado.Shared.Rewards;

namespace SaborMercado.Modules.Rewards.Services;

public sealed class RankingService(RewardsDbContext db, TimeProvider clock)
{
    private const int TopLimit = 100;

    public async Task<RankingListResponse> GetRankingAsync(
        string rankingType,
        Guid? currentUserId,
        CancellationToken cancellationToken)
    {
        if (!RankingTypes.IsValid(rankingType))
        {
            throw new RewardsException(RewardsErrorCodes.UnknownRankingType, "Tipo de ranking desconhecido.");
        }

        var snapshots = await db.RankingSnapshots
            .AsNoTracking()
            .Where(r => r.RankingType == rankingType)
            .ToListAsync(cancellationToken);

        var latestCalculatedAt = snapshots
            .OrderByDescending(r => r.CalculatedAt)
            .Select(r => (DateTimeOffset?)r.CalculatedAt)
            .FirstOrDefault();

        if (latestCalculatedAt is null)
        {
            await RecalculateAllAsync(cancellationToken);
            snapshots = await db.RankingSnapshots
                .AsNoTracking()
                .Where(r => r.RankingType == rankingType)
                .ToListAsync(cancellationToken);
            latestCalculatedAt = snapshots
                .OrderByDescending(r => r.CalculatedAt)
                .Select(r => (DateTimeOffset?)r.CalculatedAt)
                .FirstOrDefault();
        }

        if (latestCalculatedAt is null)
        {
            return new RankingListResponse(
                rankingType,
                RankingTypes.Titles[rankingType],
                [],
                null,
                null,
                clock.GetUtcNow());
        }

        var entries = snapshots
            .Where(r => r.CalculatedAt == latestCalculatedAt)
            .OrderBy(r => r.RankPosition)
            .Take(TopLimit)
            .Select(r => new RankingEntryDto(
                r.RankPosition,
                r.PseudonymDisplay,
                r.Score,
                currentUserId.HasValue && r.UserId == currentUserId.Value))
            .ToList();

        int? currentUserRank = null;
        int? currentUserScore = null;
        if (currentUserId.HasValue)
        {
            var current = snapshots.FirstOrDefault(
                r => r.CalculatedAt == latestCalculatedAt &&
                     r.UserId == currentUserId.Value);

            if (current is not null)
            {
                currentUserRank = current.RankPosition;
                currentUserScore = current.Score;
            }
        }

        return new RankingListResponse(
            rankingType,
            RankingTypes.Titles[rankingType],
            entries,
            currentUserRank,
            currentUserScore,
            latestCalculatedAt.Value);
    }

    public async Task RefreshForUserAsync(
        Guid userId,
        UserGamificationMetrics metrics,
        CancellationToken cancellationToken)
    {
        foreach (var rankingType in RankingTypes.Titles.Keys)
        {
            await RecalculateRankingTypeAsync(rankingType, cancellationToken);
        }
    }

    public async Task RecalculateAllAsync(CancellationToken cancellationToken)
    {
        foreach (var rankingType in RankingTypes.Titles.Keys)
        {
            await RecalculateRankingTypeAsync(rankingType, cancellationToken);
        }
    }

    private async Task RecalculateRankingTypeAsync(string rankingType, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var scores = await BuildScoresAsync(rankingType, cancellationToken);

        var oldSnapshots = await db.RankingSnapshots
            .Where(r => r.RankingType == rankingType)
            .ToListAsync(cancellationToken);

        if (oldSnapshots.Count > 0)
        {
            db.RankingSnapshots.RemoveRange(oldSnapshots);
        }

        var rank = 1;
        foreach (var (userId, score) in scores.OrderByDescending(s => s.Score).ThenBy(s => s.UserId))
        {
            if (score <= 0)
            {
                continue;
            }

            db.RankingSnapshots.Add(new RankingSnapshot
            {
                Id = Guid.NewGuid(),
                RankingType = rankingType,
                UserId = userId,
                PseudonymDisplay = BuildPseudonymDisplay(userId),
                RankPosition = rank++,
                Score = score,
                CalculatedAt = now,
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<List<(Guid UserId, int Score)>> BuildScoresAsync(
        string rankingType,
        CancellationToken cancellationToken)
    {
        if (rankingType == RankingTypes.Achievements)
        {
            return await db.UserAchievements
                .AsNoTracking()
                .GroupBy(a => a.UserId)
                .Select(g => new ValueTuple<Guid, int>(g.Key, g.Count()))
                .ToListAsync(cancellationToken);
        }

        var metrics = await db.UserGamificationMetrics.AsNoTracking().ToListAsync(cancellationToken);
        return metrics
            .Select(m => (m.UserId, GetScore(rankingType, m)))
            .ToList();
    }

    private static int GetScore(string rankingType, UserGamificationMetrics metrics) =>
        rankingType switch
        {
            RankingTypes.Products => metrics.TotalProductsRegistered,
            RankingTypes.Stores => metrics.TotalStoresRegistered,
            RankingTypes.Purchases => metrics.TotalPurchasesCompleted,
            RankingTypes.LoginStreak => metrics.LongestLoginStreakDays,
            _ => 0,
        };

    private static string BuildPseudonymDisplay(Guid userId)
    {
        var suffix = userId.ToString("N")[..4].ToUpperInvariant();
        return $"Usuario#{suffix}";
    }
}
