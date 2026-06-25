using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SaborMercado.Modules.SharedCatalog.Services;
using SaborMercado.Shared.Community;
namespace SaborMercado.Modules.SharedCatalog.Endpoints;

public static class CommunityEndpoints
{
    public static IEndpointRouteBuilder MapCommunityEndpoints(this IEndpointRouteBuilder app)
    {
        var votes = app.MapGroup("/api/v1/price-observations").RequireAuthorization();
        votes.MapPost("/{observationId:guid}/vote", VoteAsync);
        votes.MapDelete("/{observationId:guid}/vote", RemoveVoteAsync);

        var reports = app.MapGroup("/api/v1/contributor-reports").RequireAuthorization();
        reports.MapPost("/", ReportAsync);

        return app;
    }

    public static IEndpointRouteBuilder MapSharedObservationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/shared-products").RequireAuthorization();
        group.MapGet("/{productId:guid}/observations", ListObservationsAsync);
        return app;
    }

    private static async Task<IResult> VoteAsync(
        Guid observationId,
        VoteObservationRequest request,
        ClaimsPrincipal user,
        ObservationVoteService votes,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await votes.VoteAsync(
                observationId,
                GetAccountId(user),
                GetPseudonymId(user),
                request.Value,
                cancellationToken);
            return Results.Ok(response);
        }
        catch (CommunityException ex)
        {
            return CommunityProblem(ex);
        }
    }

    private static async Task<IResult> RemoveVoteAsync(
        Guid observationId,
        ClaimsPrincipal user,
        ObservationVoteService votes,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await votes.RemoveVoteAsync(observationId, GetAccountId(user), cancellationToken);
            return Results.Ok(response);
        }
        catch (CommunityException ex)
        {
            return CommunityProblem(ex);
        }
    }

    private static async Task<IResult> ReportAsync(
        SubmitContributorReportRequest request,
        ClaimsPrincipal user,
        ContributorReportService reports,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await reports.SubmitAsync(
                GetAccountId(user),
                GetPseudonymId(user),
                request,
                cancellationToken);
            return Results.Accepted($"/api/v1/contributor-reports/{response.ReportId}", response);
        }
        catch (CommunityException ex)
        {
            return CommunityProblem(ex);
        }
    }

    private static async Task<IResult> ListObservationsAsync(
        Guid productId,
        ClaimsPrincipal user,
        SharedObservationQueryService query,
        CancellationToken cancellationToken)
    {
        var accountId = GetAccountId(user);
        var result = await query.ListByProductAsync(
            productId,
            accountId,
            GetPseudonymId(user),
            limit: 20,
            cancellationToken);

        return result is null ? Results.NotFound() : Results.Ok(result);
    }

    private static IResult CommunityProblem(CommunityException ex)
    {
        var status = ex.Code switch
        {
            CommunityErrorCodes.ObservationNotFound => StatusCodes.Status404NotFound,
            CommunityErrorCodes.ReportAlreadySubmitted => StatusCodes.Status409Conflict,
            CommunityErrorCodes.InvalidVoteValue
                or CommunityErrorCodes.InvalidReportReason
                or CommunityErrorCodes.ReportDetailsRequired => StatusCodes.Status400BadRequest,
            CommunityErrorCodes.SelfVoteNotAllowed
                or CommunityErrorCodes.SelfReportNotAllowed
                or CommunityErrorCodes.ContributorRestricted => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status422UnprocessableEntity,
        };

        return Results.Json(
            new
            {
                type = $"https://sabormercado.app/errors/{ex.Code.ToLowerInvariant()}",
                title = ex.Message,
                status,
                code = ex.Code,
                detail = ex.Message,
            },
            statusCode: status);
    }

    private static Guid GetAccountId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id)
            ? id
            : throw new UnauthorizedAccessException();
    }

    private static Guid GetPseudonymId(ClaimsPrincipal user)
    {
        var pseudonym = user.FindFirstValue("pseudonym_id");
        return Guid.TryParse(pseudonym, out var id)
            ? id
            : throw new UnauthorizedAccessException("Pseudônimo ausente no token.");
    }
}
