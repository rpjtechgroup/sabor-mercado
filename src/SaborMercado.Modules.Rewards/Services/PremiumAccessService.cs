using SaborMercado.Shared.Rewards;

namespace SaborMercado.Modules.Rewards.Services;

public sealed class PremiumAccessService(RewardsService rewards) : IPremiumAccessService
{
    public Task<bool> HasActiveUnlockAsync(
        Guid userId,
        string featureCode,
        CancellationToken cancellationToken = default) =>
        rewards.HasActiveUnlockAsync(userId, featureCode, cancellationToken);
}
