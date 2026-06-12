namespace SaborMercado.Shared.Rewards;

public interface IContributionRewardService
{
    Task<int> GrantForAcceptedObservationAsync(
        Guid userId,
        ContributionRewardContext context,
        CancellationToken cancellationToken = default);
}

public sealed record ContributionRewardContext(
    Guid ObservationId,
    bool IsNewProduct,
    bool HasValidEan);
