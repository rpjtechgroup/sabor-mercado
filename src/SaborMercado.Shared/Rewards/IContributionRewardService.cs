namespace SaborMercado.Shared.Rewards;

/// <summary>Contrato entre SharedCatalog e Rewards (via composição na Api).</summary>
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
