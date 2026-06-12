using Microsoft.EntityFrameworkCore;
using SaborMercado.Modules.SharedCatalog.Data;
using SaborMercado.Shared.Community;

namespace SaborMercado.Modules.SharedCatalog.Services;

public sealed class SharedObservationQueryService(
    SharedCatalogDbContext db,
    ContributorTrustService trust)
{
    public async Task<SharedObservationListResponse?> ListByProductAsync(
        Guid productId,
        Guid viewerUserId,
        Guid viewerPseudonymId,
        int limit,
        CancellationToken cancellationToken)
    {
        var product = await db.SharedProducts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product is null)
        {
            return null;
        }

        var observations = (await db.PriceObservations
            .AsNoTracking()
            .Include(o => o.Market)
            .Where(o => o.SharedProductId == productId
                && o.Status == ObservationStatus.Accepted
                && !o.IsHidden)
            .ToListAsync(cancellationToken))
            .OrderByDescending(o => o.ObservedOn)
            .ThenByDescending(o => o.CreatedAt)
            .Take(limit)
            .ToList();

        if (observations.Count == 0)
        {
            return new SharedObservationListResponse(product.Id, product.NormalizedName, []);
        }

        var observationIds = observations.Select(o => o.Id).ToList();
        var votes = await db.ObservationVotes
            .AsNoTracking()
            .Where(v => observationIds.Contains(v.ObservationId) && v.VoterUserId == viewerUserId)
            .ToDictionaryAsync(v => v.ObservationId, v => v.Value, cancellationToken);

        var pseudonymIds = observations.Select(o => o.ContributorPseudonymId).Distinct().ToList();
        var trusts = await db.ContributorTrusts
            .AsNoTracking()
            .Where(t => pseudonymIds.Contains(t.PseudonymId))
            .ToDictionaryAsync(t => t.PseudonymId, cancellationToken);

        var items = new List<SharedObservationDto>();
        foreach (var observation in observations)
        {
            if (!trusts.TryGetValue(observation.ContributorPseudonymId, out var contributorTrust))
            {
                contributorTrust = new ContributorTrust { PseudonymId = observation.ContributorPseudonymId };
            }

            int? currentUserVote = votes.TryGetValue(observation.Id, out var currentVote) ? currentVote : null;
            var netScore = observation.UpvoteCount - observation.DownvoteCount;
            var isOwn = observation.ContributorPseudonymId == viewerPseudonymId;

            items.Add(new SharedObservationDto(
                observation.Id,
                observation.Price,
                observation.ObservedOn,
                observation.Market.Name,
                observation.Market.City,
                observation.Market.State,
                netScore,
                observation.UpvoteCount,
                observation.DownvoteCount,
                currentUserVote,
                trust.ToDto(contributorTrust),
                CanVote: !isOwn,
                CanReport: !isOwn));
        }

        return new SharedObservationListResponse(product.Id, product.NormalizedName, items);
    }
}
