using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SaborMercado.Modules.SharedCatalog.Services;
using SaborMercado.Shared.Rewards;

namespace SaborMercado.Modules.SharedCatalog.Endpoints;

public static class SharedProductEndpoints
{
    public static IEndpointRouteBuilder MapSharedProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/shared-products").RequireAuthorization();
        group.MapGet("/", SearchAsync);
        group.MapGet("/{productId:guid}/markets", CompareMarketsAsync);
        return app;
    }

    private static async Task<IResult> CompareMarketsAsync(
        Guid productId,
        ClaimsPrincipal user,
        MarketComparisonService comparison,
        IPremiumAccessService premium,
        CancellationToken cancellationToken)
    {
        var accountId = GetAccountId(user);
        var hasUnlock = await premium.HasActiveUnlockAsync(
            accountId,
            PremiumFeatureCodes.MarketComparison,
            cancellationToken);

        if (!hasUnlock)
        {
            return Results.Json(
                new
                {
                    type = "https://sabormercado.app/errors/premium-required",
                    title = "Comparação entre mercados requer desbloqueio premium.",
                    status = 403,
                    code = "PREMIUM_REQUIRED",
                    detail = "Desbloqueie a comparação entre mercados em Minha conta.",
                },
                statusCode: StatusCodes.Status403Forbidden);
        }

        var result = await comparison.GetMarketPricesAsync(productId, cancellationToken);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }

    private static async Task<IResult> SearchAsync(
        ClaimsPrincipal user,
        string? query,
        SharedProductSearchService search,
        IPremiumAccessService premium,
        CancellationToken cancellationToken)
    {
        var accountId = GetAccountId(user);
        var hasUnlock = await premium.HasActiveUnlockAsync(
            accountId,
            PremiumFeatureCodes.CollaborativePriceHistory,
            cancellationToken);

        if (!hasUnlock)
        {
            return Results.Json(
                new
                {
                    type = "https://sabormercado.app/errors/premium-required",
                    title = "Histórico colaborativo requer desbloqueio premium.",
                    status = 403,
                    code = "PREMIUM_REQUIRED",
                    detail = "Desbloqueie o histórico colaborativo de preços em Minha conta.",
                },
                statusCode: StatusCodes.Status403Forbidden);
        }

        if (string.IsNullOrWhiteSpace(query))
        {
            return Results.BadRequest(new { detail = "Parâmetro query é obrigatório." });
        }

        var result = await search.SearchAsync(query, limit: 20, cancellationToken);
        return Results.Ok(result);
    }

    private static Guid GetAccountId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id)
            ? id
            : throw new UnauthorizedAccessException();
    }
}
