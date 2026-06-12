using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using SaborMercado.Modules.Identity.Data;
using SaborMercado.Modules.Identity.Services;
using SaborMercado.Shared.Auth;

namespace SaborMercado.Modules.Identity.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth");

        group.MapPost("/register", RegisterAsync);
        group.MapPost("/login", LoginAsync);
        group.MapPost("/refresh", RefreshAsync);
        group.MapGet("/me", MeAsync).RequireAuthorization();
        group.MapDelete("/me", DeleteAccountAsync).RequireAuthorization();

        return app;
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        AuthService auth,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await auth.RegisterAsync(request, cancellationToken);
            return Results.Ok(response);
        }
        catch (AuthException ex)
        {
            return Problem(ex.Code, ex.Message, StatusCodes.Status400BadRequest);
        }
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        AuthService auth,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await auth.LoginAsync(request, cancellationToken);
            return Results.Ok(response);
        }
        catch (AuthException ex)
        {
            return Problem(ex.Code, ex.Message, StatusCodes.Status401Unauthorized);
        }
    }

    private static async Task<IResult> RefreshAsync(
        RefreshRequest request,
        AuthService auth,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await auth.RefreshAsync(request, cancellationToken);
            return Results.Ok(response);
        }
        catch (AuthException ex)
        {
            return Problem(ex.Code, ex.Message, StatusCodes.Status401Unauthorized);
        }
    }

    private static async Task<IResult> DeleteAccountAsync(
        ClaimsPrincipal user,
        AuthService auth,
        CancellationToken cancellationToken)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            return Results.Unauthorized();
        }

        try
        {
            await auth.DeleteAccountAsync(userId, cancellationToken);
            return Results.NoContent();
        }
        catch (AuthException ex)
        {
            return Problem(ex.Code, ex.Message, StatusCodes.Status400BadRequest);
        }
    }

    private static async Task<IResult> MeAsync(
        ClaimsPrincipal user,
        IdentityDbContext db,
        CancellationToken cancellationToken)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            return Results.Unauthorized();
        }

        var account = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (account is null)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new { userId = sub, email = account.Email, pseudonymId = account.PseudonymId });
    }

    private static IResult Problem(string code, string detail, int status) =>
        Results.Json(
            new
            {
                type = $"https://sabormercado.app/errors/{code.ToLowerInvariant()}",
                title = detail,
                status,
                code,
                detail,
            },
            statusCode: status);
}
