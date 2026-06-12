using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using SaborMercado.Web.Contracts.Recognition;

namespace SaborMercado.Web.Features.Recognition;

public sealed class GeminiShelfLabelClient(HttpClient httpClient, IConfiguration configuration)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
    };

    public async Task<RecognitionResultDto> RecognizeAsync(
        string apiKey,
        byte[] jpegBytes,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Gemini API key is required.");
        }

        var model = configuration["GeminiModel"] ?? "gemini-2.0-flash";
        var requestUri = $"v1beta/models/{model}:generateContent?key={Uri.EscapeDataString(apiKey.Trim())}";

        var body = new GeminiRequest(
            [
                new GeminiContent(
                [
                    new GeminiPart(Text: ShelfLabelPrompt.Text),
                    new GeminiPart(
                        InlineData: new GeminiInlineData("image/jpeg", Convert.ToBase64String(jpegBytes))),
                ]),
            ],
            new GeminiGenerationConfig(ResponseMimeType: "application/json", ResponseSchema: BuildSchema()));

        using var response = await httpClient.PostAsJsonAsync(requestUri, body, JsonOptions, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Gemini HTTP {(int)response.StatusCode}: {payload}");
        }

        var gemini = JsonSerializer.Deserialize<GeminiResponse>(payload, JsonOptions)
            ?? throw new InvalidOperationException("Empty Gemini response");

        var text = gemini.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("Gemini returned no structured text");
        }

        var raw = JsonSerializer.Deserialize<RecognitionResultDto>(text, JsonOptions)
            ?? throw new InvalidOperationException("Invalid structured JSON from Gemini");

        return RecognitionNormalizer.Normalize(raw);
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
