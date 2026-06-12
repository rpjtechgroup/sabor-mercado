using Microsoft.EntityFrameworkCore;
using SaborMercado.Modules.SharedCatalog.Data;
using SaborMercado.Shared.Community;

namespace SaborMercado.Modules.SharedCatalog.Services;

public sealed class ContributorReportService(
    SharedCatalogDbContext db,
    ContributorTrustService trust,
    TimeProvider clock)
{
    public async Task<ContributorReportResponse> SubmitAsync(
        Guid reporterUserId,
        Guid reporterPseudonymId,
        SubmitContributorReportRequest request,
        CancellationToken cancellationToken)
    {
        if (request.TargetPseudonymId == reporterPseudonymId)
        {
            throw new CommunityException(
                CommunityErrorCodes.SelfReportNotAllowed,
                "Você não pode denunciar a si mesmo.");
        }

        if (!ReportReasons.All.Contains(request.Reason))
        {
            throw new CommunityException(CommunityErrorCodes.InvalidReportReason, "Motivo de denúncia inválido.");
        }

        if (request.Reason == ReportReasons.Other && string.IsNullOrWhiteSpace(request.Details))
        {
            throw new CommunityException(
                CommunityErrorCodes.ReportDetailsRequired,
                "Descreva o motivo ao escolher 'Outro'.");
        }

        if (request.ObservationId is { } observationId)
        {
            var exists = await db.PriceObservations.AnyAsync(o => o.Id == observationId, cancellationToken);
            if (!exists)
            {
                throw new CommunityException(CommunityErrorCodes.ObservationNotFound, "Observação não encontrada.");
            }
        }

        var since = clock.GetUtcNow().AddHours(-24);
        var recentReports = await db.ContributorReports
            .AsNoTracking()
            .Where(r => r.ReporterUserId == reporterUserId
                && r.TargetPseudonymId == request.TargetPseudonymId
                && r.Reason == request.Reason)
            .ToListAsync(cancellationToken);

        var duplicate = recentReports.Any(r =>
            r.CreatedAt >= since && r.ObservationId == request.ObservationId);

        if (duplicate)
        {
            throw new CommunityException(
                CommunityErrorCodes.ReportAlreadySubmitted,
                "Você já enviou esta denúncia nas últimas 24 horas.");
        }

        var report = new ContributorReport
        {
            Id = Guid.NewGuid(),
            ReporterUserId = reporterUserId,
            TargetPseudonymId = request.TargetPseudonymId,
            ObservationId = request.ObservationId,
            Reason = request.Reason,
            Details = string.IsNullOrWhiteSpace(request.Details) ? null : request.Details.Trim(),
            CreatedAt = clock.GetUtcNow(),
        };

        db.ContributorReports.Add(report);
        await db.SaveChangesAsync(cancellationToken);
        await trust.RegisterReportAsync(request.TargetPseudonymId, cancellationToken);

        return new ContributorReportResponse(report.Id, "Accepted");
    }
}
