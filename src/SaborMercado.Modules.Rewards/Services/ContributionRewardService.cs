using SaborMercado.Modules.Rewards.Data;
using SaborMercado.Shared.Rewards;

namespace SaborMercado.Modules.Rewards.Services;

public sealed class ContributionRewardService(RewardsDbContext db, TimeProvider clock) : IContributionRewardService
{
    public async Task<int> GrantForAcceptedObservationAsync(
        Guid userId,
        ContributionRewardContext context,
        CancellationToken cancellationToken = default)
    {
        var credits = context.IsNewProduct ? 5 : 1;
        if (context.HasValidEan)
        {
            credits += 1;
        }

        db.CreditLedgerEntries.Add(new CreditLedgerEntry
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Amount = credits,
            Reason = context.IsNewProduct ? CreditReason.NewProductBonus : CreditReason.ContributionAccepted,
            ReferenceId = context.ObservationId,
            CreatedAt = clock.GetUtcNow(),
        });

        await db.SaveChangesAsync(cancellationToken);
        return credits;
    }
}
