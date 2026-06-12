using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SaborMercado.Modules.Recognition.Services;
using SaborMercado.Shared.Recognition;

namespace SaborMercado.Modules.Recognition.Endpoints;

public static class RecognitionEndpoints
{
    public static IEndpointRouteBuilder MapRecognitionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/recognitions")
            .RequireRateLimiting("recognition");

        group.MapPost("/", RecognizeAsync)
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<RecognitionResultDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable);

        return app;
    }

    private static async Task<IResult> RecognizeAsync(
        HttpContext httpContext,
        [FromForm] IFormFile? image,
        ShelfLabelRecognitionService service,
        CancellationToken cancellationToken)
    {
        if (image is null || image.Length == 0)
        {
            return Problem(
                RecognitionErrorCodes.InvalidImage,
                "Imagem obrigatória",
                StatusCodes.Status400BadRequest,
                "Envie um arquivo de imagem no campo 'image'.");
        }

        var clientKey = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        try
        {
            await using var stream = image.OpenReadStream();
            var result = await service.RecognizeAsync(stream, image.ContentType, clientKey, cancellationToken);
            return Results.Ok(result);
        }
        catch (InvalidDataException ex) when (ex.Message.Contains("maximum size", StringComparison.OrdinalIgnoreCase))
        {
            return Problem(
                RecognitionErrorCodes.PayloadTooLarge,
                "Imagem muito grande",
                StatusCodes.Status400BadRequest,
                "A imagem deve ter no máximo 4 MB.");
        }
        catch (InvalidDataException ex)
        {
            return Problem(
                RecognitionErrorCodes.InvalidImage,
                "Imagem inválida",
                StatusCodes.Status400BadRequest,
                ex.Message);
        }
        catch (OcrUnavailableException ex)
        {
            return Problem(
                RecognitionErrorCodes.OcrUnavailable,
                "Serviço de leitura indisponível",
                StatusCodes.Status503ServiceUnavailable,
                ex.Reason);
        }
    }

    private static IResult Problem(string code, string title, int status, string detail) =>
        Results.Json(
            new
            {
                type = $"https://sabormercado.app/errors/{code.ToLowerInvariant()}",
                title,
                status,
                code,
                detail,
            },
            statusCode: status);
}
