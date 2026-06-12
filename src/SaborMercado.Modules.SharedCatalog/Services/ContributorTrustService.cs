using Microsoft.EntityFrameworkCore;
using SaborMercado.Modules.SharedCatalog.Data;
using SaborMercado.Modules.SharedCatalog.Domain;
using SaborMercado.Shared.Community;

namespace SaborMercado.Modules.SharedCatalog.Services;

public sealed class ContributorTrustService(SharedCatalogDbContext db, TimeProvider clock)
{
    public async Task<ContributorTrust> GetOrCreateAsync(Guid pseudonymId, CancellationToken cancellationToken)
    {
        var trust = await db.ContributorTrusts.FindAsync([pseudonymId], cancellationToken);
        if (trust is not null)
        {
            await RefreshRestrictionAsync(trust, cancellationToken);
            return trust;
        }

        trust = new ContributorTrust
        {
            PseudonymId = pseudonymId,
            TrustScore = 50,
            UpdatedAt = clock.GetUtcNow(),
        };
        db.ContributorTrusts.Add(trust);
        await db.SaveChangesAsync(cancellationToken);
        return trust;
    }

    public async Task IncrementAcceptedContributionAsync(
        Guid pseudonymId,
        Guid contributorUserId,
        CancellationToken cancellationToken)
    {
        var trust = await GetOrCreateAsync(pseudonymId, cancellationToken);
        trust.ContributorUserId ??= contributorUserId;
        trust.AcceptedContributions++;
        Recalculate(trust);
        trust.UpdatedAt = clock.GetUtcNow();
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task ApplyVoteDeltaAsync(
        Guid pseudonymId,
        int previousVote,
        int newVote,
        CancellationToken cancellationToken)
    {
        if (previousVote == newVote)
        {
            return;
        }

        var trust = await GetOrCreateAsync(pseudonymId, cancellationToken);

        if (previousVote > 0)
        {
            trust.TotalUpvotesReceived = Math.Max(0, trust.TotalUpvotesReceived - 1);
        }
        else if (previousVote < 0)
        {
            trust.TotalDownvotesReceived = Math.Max(0, trust.TotalDownvotesReceived - 1);
        }

        if (newVote > 0)
        {
            trust.TotalUpvotesReceived++;
        }
        else if (newVote < 0)
        {
            trust.TotalDownvotesReceived++;
        }

        Recalculate(trust);
        trust.UpdatedAt = clock.GetUtcNow();
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task RegisterReportAsync(Guid targetPseudonymId, CancellationToken cancellationToken)
    {
        var trust = await GetOrCreateAsync(targetPseudonymId, cancellationToken);
        trust.ReportCount++;

        var windowStart = clock.GetUtcNow().AddDays(-7);
        var recentReports = await db.ContributorReports
            .AsNoTracking()
            .Where(r => r.TargetPseudonymId == targetPseudonymId)
            .ToListAsync(cancellationToken);

        var distinctReporters = recentReports
            .Where(r => r.CreatedAt >= windowStart)
            .Select(r => r.ReporterUserId)
            .Distinct()
            .Count();

        if (distinctReporters >= TrustScoreCalculator.RestrictionReportThreshold)
        {
            trust.IsRestricted = true;
            trust.RestrictedUntil = clock.GetUtcNow().AddDays(7);
        }

        Recalculate(trust);
        trust.UpdatedAt = clock.GetUtcNow();
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task EnsureCanContributeAsync(Guid pseudonymId, CancellationToken cancellationToken)
    {
        var trust = await GetOrCreateAsync(pseudonymId, cancellationToken);
        if (trust.IsRestricted)
        {
            throw new CommunityException(
                CommunityErrorCodes.ContributorRestricted,
                "Sua conta de contribuidor está temporariamente restrita por denúncias da comunidade.");
        }
    }

    public ContributorTrustDto ToDto(ContributorTrust trust) => new(
        trust.PseudonymId,
        trust.TrustScore,
        TrustScoreCalculator.Label(trust.TrustScore),
        trust.AcceptedContributions,
        trust.IsRestricted);

    private async Task RefreshRestrictionAsync(ContributorTrust trust, CancellationToken cancellationToken)
    {
        if (!trust.IsRestricted || trust.RestrictedUntil is null)
        {
            return;
        }

        if (trust.RestrictedUntil <= clock.GetUtcNow())
        {
            trust.IsRestricted = false;
            trust.RestrictedUntil = null;
            Recalculate(trust);
            trust.UpdatedAt = clock.GetUtcNow();
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static void Recalculate(ContributorTrust trust)
    {
        trust.TrustScore = TrustScoreCalculator.Calculate(
            trust.TotalUpvotesReceived,
            trust.TotalDownvotesReceived,
            trust.AcceptedContributions,
            trust.ReportCount,
            trust.IsRestricted);
    }
}

public sealed class CommunityException(string code, string detail) : Exception(detail)
{
    public string Code { get; } = code;
}
