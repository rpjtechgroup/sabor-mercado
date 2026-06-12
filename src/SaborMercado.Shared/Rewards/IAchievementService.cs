namespace SaborMercado.Shared.Rewards;

/// <summary>Contrato entre SharedCatalog e Rewards para conquistas.</summary>
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
}
