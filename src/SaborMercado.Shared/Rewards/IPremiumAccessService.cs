namespace SaborMercado.Shared.Rewards;

public interface IPremiumAccessService
{
    Task<bool> HasActiveUnlockAsync(Guid userId, string featureCode, CancellationToken cancellationToken = default);
}
