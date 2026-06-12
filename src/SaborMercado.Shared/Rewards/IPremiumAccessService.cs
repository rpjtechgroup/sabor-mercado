namespace SaborMercado.Shared.Rewards;

/// <summary>Contrato entre SharedCatalog e Rewards para checagem de desbloqueio premium.</summary>
public interface IPremiumAccessService
{
    Task<bool> HasActiveUnlockAsync(Guid userId, string featureCode, CancellationToken cancellationToken = default);
}
