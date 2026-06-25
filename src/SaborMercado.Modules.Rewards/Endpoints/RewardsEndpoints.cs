using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SaborMercado.Modules.Rewards.Services;
using SaborMercado.Shared.Community;
using SaborMercado.Shared.Rewards;

namespace SaborMercado.Modules.Rewards.Endpoints;

public static class RewardsEndpoints
{
    public static IEndpointRouteBuilder MapRewardsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1").RequireAuthorization();

        group.MapGet("/achievements", ListAchievementsAsync);
        group.MapGet("/achievements/catalog", GetAchievementCatalogAsync);
        group.MapPost("/metrics/sync", SyncMetricsAsync);
        group.MapGet("/rankings/{rankingType}", GetRankingAsync);
        group.MapGet("/rankings/{rankingType}/me", GetMyRankAsync);

        return app;
    }

    private static async Task<IResult> ListAchievementsAsync(
        ClaimsPrincipal user,
        AchievementService achievements,
        CancellationToken cancellationToken)
    {
        var userId = GetAccountId(user);
        var response = await achievements.ListForUserAsync(userId, cancellationToken);
        return Results.Ok(response);
    }

    private static async Task<IResult> GetAchievementCatalogAsync(
        ClaimsPrincipal user,
        AchievementService achievements,
        CancellationToken cancellationToken)
    {
        var userId = GetAccountId(user);
        var response = await achievements.GetCatalogForUserAsync(userId, localMetrics: null, cancellationToken);
        return Results.Ok(response);
    }

    private static async Task<IResult> SyncMetricsAsync(
        ClaimsPrincipal user,
        SyncMetricsRequest request,
        MetricsSyncService metricsSync,
        AchievementService achievements,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetAccountId(user);
            var response = await metricsSync.SyncAsync(userId, request, cancellationToken);
            return Results.Ok(response);
        }
        catch (RewardsException ex)
        {
            return RewardsProblem(ex);
        }
    }

    private static async Task<IResult> GetRankingAsync(
        string rankingType,
        ClaimsPrincipal user,
        RankingService rankings,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetAccountId(user);
            var response = await rankings.GetRankingAsync(rankingType, userId, cancellationToken);
            return Results.Ok(response);
        }
        catch (RewardsException ex)
        {
            return RewardsProblem(ex);
        }
    }

    private static async Task<IResult> GetMyRankAsync(
        string rankingType,
        ClaimsPrincipal user,
        RankingService rankings,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetAccountId(user);
            var response = await rankings.GetRankingAsync(rankingType, userId, cancellationToken);
            return Results.Ok(new
            {
                response.RankingType,
                response.CurrentUserRank,
                response.CurrentUserScore,
                response.CalculatedAt,
            });
        }
        catch (RewardsException ex)
        {
            return RewardsProblem(ex);
        }
    }

    private static IResult RewardsProblem(RewardsException ex) =>
        Results.Json(
            new
            {
                type = $"https://sabormercado.app/errors/{ex.Code.ToLowerInvariant()}",
                title = ex.Message,
                status = StatusCodes.Status400BadRequest,
                code = ex.Code,
                detail = ex.Message,
            },
            statusCode: StatusCodes.Status400BadRequest);

    private static Guid GetAccountId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id)
            ? id
            : throw new UnauthorizedAccessException();
    }
}
