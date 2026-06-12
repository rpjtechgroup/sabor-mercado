using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SaborMercado.Modules.SharedCatalog.Services;
using SaborMercado.Shared.Community;
using SaborMercado.Shared.SharedCatalog;

namespace SaborMercado.Modules.SharedCatalog.Endpoints;

public static class ContributionEndpoints
{
    public static IEndpointRouteBuilder MapContributionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/price-observations").RequireAuthorization();

        group.MapPost("/", SubmitAsync);

        return app;
    }

    private static async Task<IResult> SubmitAsync(
        ClaimsPrincipal user,
        SubmitPriceObservationRequest request,
        ContributionService service,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        try
        {
            var idempotencyKey = httpContext.Request.Headers.TryGetValue("Idempotency-Key", out var values)
                ? values.ToString()
                : null;

            var result = await service.SubmitAsync(user, request, idempotencyKey, cancellationToken);
            return Results.Accepted($"/api/v1/price-observations/{result.ObservationId}", result);
        }
        catch (CommunityException ex)
        {
            return Results.Json(
                new
                {
                    type = $"https://sabormercado.app/errors/{ex.Code.ToLowerInvariant()}",
                    title = ex.Message,
                    status = StatusCodes.Status403Forbidden,
                    code = ex.Code,
                    detail = ex.Message,
                },
                statusCode: StatusCodes.Status403Forbidden);
        }
        catch (ContributionException ex)
        {
            var statusCode = ex.Code == ContributionErrorCodes.IdempotencyConflict
                ? StatusCodes.Status409Conflict
                : StatusCodes.Status422UnprocessableEntity;

            return Results.Json(
                new
                {
                    type = $"https://sabormercado.app/errors/{ex.Code.ToLowerInvariant()}",
                    title = ex.Message,
                    status = statusCode,
                    code = ex.Code,
                    detail = ex.Message,
                },
                statusCode: statusCode);
        }
    }
}
