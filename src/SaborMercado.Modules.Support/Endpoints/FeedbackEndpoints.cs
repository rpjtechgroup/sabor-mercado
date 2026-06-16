using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using SaborMercado.Modules.Support.Contracts;
using SaborMercado.Modules.Support.Services;

namespace SaborMercado.Modules.Support.Endpoints;

public static class FeedbackEndpoints
{
    public static IEndpointRouteBuilder MapFeedbackEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/feedback");
        group.MapPost("/", SubmitAsync).RequireRateLimiting("feedback");
        return app;
    }

    private static async Task<IResult> SubmitAsync(
        FeedbackRequest request,
        FeedbackEmailService feedback,
        CancellationToken cancellationToken)
    {
        var validation = Validate(request);
        if (validation is not null)
        {
            return Problem(SupportErrorCodes.InvalidFeedback, validation, StatusCodes.Status400BadRequest);
        }

        try
        {
            await feedback.SendAsync(request, cancellationToken);
            return Results.Accepted();
        }
        catch (FeedbackException ex)
        {
            return Problem(ex.Code, ex.Message, StatusCodes.Status503ServiceUnavailable);
        }
    }

    private static string? Validate(FeedbackRequest request)
    {
        if (!FeedbackCategories.IsValid(request.Category))
        {
            return "Categoria inválida.";
        }

        if (string.IsNullOrWhiteSpace(request.Subject) || request.Subject.Trim().Length > 200)
        {
            return "Assunto obrigatório (máx. 200 caracteres).";
        }

        if (string.IsNullOrWhiteSpace(request.Message) || request.Message.Trim().Length < 10)
        {
            return "Mensagem obrigatória (mín. 10 caracteres).";
        }

        if (request.Message.Length > 8000)
        {
            return "Mensagem muito longa.";
        }

        if (!string.IsNullOrWhiteSpace(request.ContactEmail) &&
            (!request.ContactEmail.Contains('@') || request.ContactEmail.Length > 256))
        {
            return "E-mail de contato inválido.";
        }

        if (!string.IsNullOrWhiteSpace(request.ContactName) && request.ContactName.Length > 120)
        {
            return "Nome muito longo.";
        }

        return null;
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
