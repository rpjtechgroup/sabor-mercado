using SaborMercado.Shared.Community;

namespace SaborMercado.Shared.Rewards;

public interface IAchievementService
{
    Task EvaluateAfterContributionAsync(
        Guid userId,
        int acceptedContributions,
        int trustScore,
        int totalUpvotesReceived,
        CancellationToken cancellationToken = default);

    Task EvaluateAfterVoteAsync(
        Guid? contributorUserId,
        int trustScore,
        int totalUpvotesReceived,
        int observationNetScore,
        CancellationToken cancellationToken = default);

    Task<AchievementListResponse> EvaluateAfterMetricsSyncAsync(
        Guid userId,
        UserMetricsValues metrics,
        CancellationToken cancellationToken = default);

    Task<AchievementListResponse> ListForUserAsync(Guid userId, CancellationToken cancellationToken);

    Task<AchievementCatalogResponse> GetCatalogForUserAsync(
        Guid userId,
        UserMetricsValues? localMetrics,
        CancellationToken cancellationToken);
}
