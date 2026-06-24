using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SaborMercado.Modules.Recognition.Domain;
using SaborMercado.Shared.Gemini;
using SaborMercado.Shared.Recognition;

namespace SaborMercado.Modules.Recognition.Services;

public sealed class GeminiVisionClient(
    HttpClient httpClient,
    IOptions<RecognitionOptions> options,
    ILogger<GeminiVisionClient> logger) : IGeminiVisionClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private static readonly string Prompt = LoadPrompt();

    public async Task<RecognitionResultDto> RecognizeAsync(
        ReadOnlyMemory<byte> imageBytes,
        string contentType,
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var apiKey = settings.GeminiApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new OcrUnavailableException("GEMINI_API_KEY not configured");
        }

        var models = GeminiModelChain.Build(settings.GeminiModel, settings.GeminiModelFallbacks);
        var body = new GeminiRequest(
            [
                new GeminiContent(
                [
                    new GeminiPart(Text: Prompt),
                    new GeminiPart(
                        InlineData: new GeminiInlineData(contentType, Convert.ToBase64String(imageBytes.Span))),
                ]),
            ],
            new GeminiGenerationConfig(ResponseMimeType: "application/json", ResponseSchema: BuildSchema()));

        GeminiGenerateContentResult result;
        try
        {
            result = await GeminiGenerateContentExecutor.PostJsonAsync(
                httpClient,
                apiKey,
                models,
                body,
                JsonOptions,
                cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Gemini API failed after trying {ModelCount} models", models.Count);
            throw new OcrUnavailableException(ex.Message);
        }

        var gemini = JsonSerializer.Deserialize<GeminiResponse>(result.Payload, JsonOptions)
            ?? throw new OcrUnavailableException("Empty Gemini response");

        var text = gemini.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new OcrUnavailableException($"Gemini ({result.ModelUsed}) returned no structured text");
        }

        var raw = JsonSerializer.Deserialize<RecognitionResultDto>(text, JsonOptions)
            ?? throw new OcrUnavailableException("Invalid structured JSON from Gemini");

        return RecognitionNormalizer.Normalize(raw);
    }

    private static string LoadPrompt()
    {
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("SaborMercado.Modules.Recognition.Prompts.shelf-label.txt");
        return stream is null
            ? "Extract Brazilian supermarket shelf label data as JSON."
            : new StreamReader(stream).ReadToEnd();
    }

    private static object BuildSchema() => new
    {
        type = "object",
        properties = new
        {
            productName = new { type = "string", nullable = true },
            brand = new { type = "string", nullable = true },
            quantityValue = new { type = "number", nullable = true },
            quantityUnit = new { type = "string", nullable = true },
            price = new { type = "number", nullable = true },
            ean = new { type = "string", nullable = true },
            confidence = new { type = "number" },
            rawText = new { type = "string", nullable = true },
        },
        required = new[] { "confidence" },
    };

    private sealed record GeminiRequest(
        GeminiContent[] Contents,
        GeminiGenerationConfig GenerationConfig);

    private sealed record GeminiContent(GeminiPart[] Parts);

    private sealed record GeminiPart(string? Text = null, GeminiInlineData? InlineData = null);

    private sealed record GeminiInlineData(string MimeType, string Data);

    private sealed record GeminiGenerationConfig(string ResponseMimeType, object ResponseSchema);

    private sealed record GeminiResponse(GeminiCandidate[]? Candidates);

    private sealed record GeminiCandidate(GeminiContent? Content);
}
