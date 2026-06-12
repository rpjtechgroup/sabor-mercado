using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SaborMercado.Modules.Rewards.Services;
using SaborMercado.Shared.Rewards;

namespace SaborMercado.Modules.Rewards.Endpoints;

public static class RewardsEndpoints
{
    public static IEndpointRouteBuilder MapRewardsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1").RequireAuthorization();

        group.MapGet("/credits", GetCreditsAsync);
        group.MapPost("/unlocks", UnlockAsync);

        return app;
    }

    private static async Task<IResult> GetCreditsAsync(
        ClaimsPrincipal user,
        RewardsService rewards,
        CancellationToken cancellationToken)
    {
        var response = await rewards.GetCreditsAsync(user, cancellationToken);
        return Results.Ok(response);
    }

    private static async Task<IResult> UnlockAsync(
        ClaimsPrincipal user,
        UnlockRequest request,
        RewardsService rewards,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await rewards.UnlockAsync(user, request, cancellationToken);
            return Results.Ok(response);
        }
        catch (RewardsException ex)
        {
            var status = ex.Code == RewardsErrorCodes.InsufficientCredits
                ? StatusCodes.Status422UnprocessableEntity
                : StatusCodes.Status400BadRequest;

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
    }
}
