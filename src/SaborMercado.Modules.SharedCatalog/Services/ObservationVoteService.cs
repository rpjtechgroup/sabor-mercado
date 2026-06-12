using Microsoft.EntityFrameworkCore;
using SaborMercado.Modules.SharedCatalog.Data;
using SaborMercado.Modules.SharedCatalog.Domain;
using SaborMercado.Shared.Community;
using SaborMercado.Shared.Rewards;

namespace SaborMercado.Modules.SharedCatalog.Services;

public sealed class ObservationVoteService(
    SharedCatalogDbContext db,
    ContributorTrustService trust,
    IAchievementService achievements,
    TimeProvider clock)
{
    public async Task<VoteObservationResponse> VoteAsync(
        Guid observationId,
        Guid voterUserId,
        Guid voterPseudonymId,
        int value,
        CancellationToken cancellationToken)
    {
        if (value is not (1 or -1))
        {
            throw new CommunityException(CommunityErrorCodes.InvalidVoteValue, "Voto deve ser +1 ou −1.");
        }

        var observation = await db.PriceObservations
            .FirstOrDefaultAsync(o => o.Id == observationId, cancellationToken)
            ?? throw new CommunityException(CommunityErrorCodes.ObservationNotFound, "Observação não encontrada.");

        if (observation.Status != ObservationStatus.Accepted)
        {
            throw new CommunityException(CommunityErrorCodes.ObservationNotVotable, "Observação não pode ser votada.");
        }

        if (observation.ContributorPseudonymId == voterPseudonymId)
        {
            throw new CommunityException(CommunityErrorCodes.SelfVoteNotAllowed, "Você não pode votar na própria observação.");
        }

        var existing = await db.ObservationVotes
            .FirstOrDefaultAsync(v => v.ObservationId == observationId && v.VoterUserId == voterUserId, cancellationToken);

        var previousVote = existing?.Value ?? 0;
        if (existing is null)
        {
            db.ObservationVotes.Add(new ObservationVote
            {
                Id = Guid.NewGuid(),
                ObservationId = observationId,
                VoterUserId = voterUserId,
                Value = value,
                CreatedAt = clock.GetUtcNow(),
                UpdatedAt = clock.GetUtcNow(),
            });
        }
        else
        {
            existing.Value = value;
            existing.UpdatedAt = clock.GetUtcNow();
        }

        ApplyObservationCounts(observation, previousVote, value);
        await trust.ApplyVoteDeltaAsync(observation.ContributorPseudonymId, previousVote, value, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        var netScore = observation.UpvoteCount - observation.DownvoteCount;
        var contributorTrust = await trust.GetOrCreateAsync(observation.ContributorPseudonymId, cancellationToken);
        await achievements.EvaluateAfterVoteAsync(
            contributorTrust.ContributorUserId,
            contributorTrust.TrustScore,
            contributorTrust.TotalUpvotesReceived,
            netScore,
            cancellationToken);

        return new VoteObservationResponse(
            observation.Id,
            netScore,
            observation.UpvoteCount,
            observation.DownvoteCount,
            value);
    }

    public async Task<VoteObservationResponse> RemoveVoteAsync(
        Guid observationId,
        Guid voterUserId,
        CancellationToken cancellationToken)
    {
        var observation = await db.PriceObservations
            .FirstOrDefaultAsync(o => o.Id == observationId, cancellationToken)
            ?? throw new CommunityException(CommunityErrorCodes.ObservationNotFound, "Observação não encontrada.");

        var existing = await db.ObservationVotes
            .FirstOrDefaultAsync(v => v.ObservationId == observationId && v.VoterUserId == voterUserId, cancellationToken);

        if (existing is null)
        {
            return new VoteObservationResponse(
                observation.Id,
                observation.UpvoteCount - observation.DownvoteCount,
                observation.UpvoteCount,
                observation.DownvoteCount,
                null);
        }

        var previousVote = existing.Value;
        db.ObservationVotes.Remove(existing);
        ApplyObservationCounts(observation, previousVote, 0);
        await trust.ApplyVoteDeltaAsync(observation.ContributorPseudonymId, previousVote, 0, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return new VoteObservationResponse(
            observation.Id,
            observation.UpvoteCount - observation.DownvoteCount,
            observation.UpvoteCount,
            observation.DownvoteCount,
            null);
    }

    private static void ApplyObservationCounts(PriceObservation observation, int previousVote, int newVote)
    {
        if (previousVote > 0)
        {
            observation.UpvoteCount = Math.Max(0, observation.UpvoteCount - 1);
        }
        else if (previousVote < 0)
        {
            observation.DownvoteCount = Math.Max(0, observation.DownvoteCount - 1);
        }

        if (newVote > 0)
        {
            observation.UpvoteCount++;
        }
        else if (newVote < 0)
        {
            observation.DownvoteCount++;
        }

        var netScore = observation.UpvoteCount - observation.DownvoteCount;
        observation.IsHidden = TrustScoreCalculator.ShouldHideObservation(netScore);
    }
}
