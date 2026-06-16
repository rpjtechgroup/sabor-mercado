using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SaborMercado.Shared.StarterCatalog;

namespace SaborMercado.Modules.SharedCatalog.Endpoints;

public static class StarterCatalogEndpoints
{
    public static IEndpointRouteBuilder MapStarterCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/starter-catalog", GetStarterCatalogAsync);
        return app;
    }

    private static IResult GetStarterCatalogAsync(HttpContext context, StarterCatalogProvider provider)
    {
        var etag = provider.GetETag();
        if (context.Request.Headers.IfNoneMatch.Contains(etag))
        {
            return Results.StatusCode(StatusCodes.Status304NotModified);
        }

        context.Response.Headers.ETag = etag;
        return Results.Ok(provider.GetCatalog());
    }
}
