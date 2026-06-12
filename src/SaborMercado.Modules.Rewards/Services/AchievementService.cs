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

    private async Task UnlockIfMissingAsync(
        Guid userId,
        string code,
        bool condition,
        CancellationToken cancellationToken)
    {
        if (!condition)
        {
            return;
        }

        var exists = await db.UserAchievements.AnyAsync(
            a => a.UserId == userId && a.AchievementCode == code,
            cancellationToken);

        if (exists)
        {
            return;
        }

        db.UserAchievements.Add(new UserAchievement
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AchievementCode = code,
            UnlockedAt = clock.GetUtcNow(),
        });

        await db.SaveChangesAsync(cancellationToken);
    }
}
