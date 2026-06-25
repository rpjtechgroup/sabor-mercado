using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SaborMercado.Modules.SharedCatalog.Services;

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
        MarketComparisonService comparison,
        CancellationToken cancellationToken)
    {
        var result = await comparison.GetMarketPricesAsync(productId, cancellationToken);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }

    private static async Task<IResult> SearchAsync(
        string? query,
        SharedProductSearchService search,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Results.BadRequest(new { detail = "Parâmetro query é obrigatório." });
        }

        var result = await search.SearchAsync(query, limit: 20, cancellationToken);
        return Results.Ok(result);
    }
}
