using Microsoft.EntityFrameworkCore;
using SaborMercado.Modules.Rewards.Data;
using SaborMercado.Shared.Community;
using SaborMercado.Shared.Rewards;

namespace SaborMercado.Modules.Rewards.Services;

public sealed class AchievementService(RewardsDbContext db, TimeProvider clock) : IAchievementService
{
    public async Task EvaluateAfterContributionAsync(
        Guid userId,
        int acceptedContributions,
        int trustScore,
        int totalUpvotesReceived,
        CancellationToken cancellationToken = default)
    {
        await UnlockIfMissingAsync(userId, AchievementCodes.FirstContribution, acceptedContributions >= 1, cancellationToken);
        await UnlockIfMissingAsync(userId, AchievementCodes.Contributor10, acceptedContributions >= 10, cancellationToken);
        await UnlockIfMissingAsync(userId, AchievementCodes.Contributor50, acceptedContributions >= 50, cancellationToken);
        await UnlockIfMissingAsync(userId, AchievementCodes.TrustedVoice, trustScore >= 70, cancellationToken);
        await UnlockIfMissingAsync(userId, AchievementCodes.CommunityHelper, totalUpvotesReceived >= 25, cancellationToken);
    }

    public async Task EvaluateAfterVoteAsync(
        Guid? contributorUserId,
        int trustScore,
        int totalUpvotesReceived,
        int observationNetScore,
        CancellationToken cancellationToken = default)
    {
        if (contributorUserId is null)
        {
            return;
        }

        await UnlockIfMissingAsync(
            contributorUserId.Value,
            AchievementCodes.QualityObservation,
            observationNetScore >= 5,
            cancellationToken);
        await UnlockIfMissingAsync(contributorUserId.Value, AchievementCodes.TrustedVoice, trustScore >= 70, cancellationToken);
        await UnlockIfMissingAsync(
            contributorUserId.Value,
            AchievementCodes.CommunityHelper,
            totalUpvotesReceived >= 25,
            cancellationToken);
    }

    public async Task<AchievementListResponse> EvaluateAfterMetricsSyncAsync(
        Guid userId,
        UserMetricsValues metrics,
        CancellationToken cancellationToken = default)
    {
        var newlyUnlocked = new List<AchievementDto>();

        foreach (var (code, (metricKey, threshold)) in AchievementCodes.MetricThresholds)
        {
            var value = AchievementCodes.GetMetricValue(metricKey, metrics);
            if (value < threshold)
            {
                continue;
            }

            var unlocked = await UnlockIfMissingAsync(userId, code, condition: true, cancellationToken);
            if (unlocked is not null)
            {
                newlyUnlocked.Add(unlocked);
            }
        }

        return new AchievementListResponse(newlyUnlocked);
    }

    public async Task<AchievementListResponse> ListForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var items = (await db.UserAchievements
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .ToListAsync(cancellationToken))
            .OrderByDescending(a => a.UnlockedAt)
            .ToList();

        var dtos = items
            .Where(a => AchievementCodes.Catalog.ContainsKey(a.AchievementCode))
            .Select(a =>
            {
                var (title, description) = AchievementCodes.Catalog[a.AchievementCode];
                return new AchievementDto(a.AchievementCode, title, description, a.UnlockedAt);
            })
            .ToList();

        return new AchievementListResponse(dtos);
    }

    public async Task<AchievementCatalogResponse> GetCatalogForUserAsync(
        Guid userId,
        UserMetricsValues? localMetrics,
        CancellationToken cancellationToken)
    {
        var unlocked = await db.UserAchievements
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .ToDictionaryAsync(a => a.AchievementCode, a => a.UnlockedAt, cancellationToken);

        var items = AchievementCodes.Catalog
            .Select(entry =>
            {
                var code = entry.Key;
                var (title, description) = entry.Value;
                var isUnlocked = unlocked.ContainsKey(code);
                int? current = null;
                int? target = null;

                if (AchievementCodes.MetricThresholds.TryGetValue(code, out var threshold))
                {
                    target = threshold.Threshold;
                    if (localMetrics is not null)
                    {
                        current = AchievementCodes.GetMetricValue(threshold.MetricKey, localMetrics);
                    }
                }

                return new AchievementProgressDto(
                    code,
                    title,
                    description,
                    isUnlocked,
                    isUnlocked ? unlocked[code] : null,
                    current,
                    target);
            })
            .OrderByDescending(i => i.IsUnlocked)
            .ThenBy(i => i.Title, StringComparer.Ordinal)
            .ToList();

        return new AchievementCatalogResponse(items, unlocked.Count, AchievementCodes.Catalog.Count);
    }

    private async Task<AchievementDto?> UnlockIfMissingAsync(
        Guid userId,
        string code,
        bool condition,
        CancellationToken cancellationToken)
    {
        if (!condition)
        {
            return null;
        }

        var exists = await db.UserAchievements.AnyAsync(
            a => a.UserId == userId && a.AchievementCode == code,
            cancellationToken);

        if (exists)
        {
            return null;
        }

        var unlockedAt = clock.GetUtcNow();
        db.UserAchievements.Add(new UserAchievement
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AchievementCode = code,
            UnlockedAt = unlockedAt,
        });

        await db.SaveChangesAsync(cancellationToken);

        if (!AchievementCodes.Catalog.TryGetValue(code, out var meta))
        {
            return new AchievementDto(code, code, string.Empty, unlockedAt);
        }

        return new AchievementDto(code, meta.Title, meta.Description, unlockedAt);
    }
}
