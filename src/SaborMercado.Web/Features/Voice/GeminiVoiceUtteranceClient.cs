using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace SaborMercado.Web.Features.Voice;

public sealed class GeminiVoiceUtteranceClient(HttpClient httpClient, IConfiguration configuration)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
    };

    public async Task<string> ExtractProductFieldsAsync(
        string apiKey,
        string transcript,
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
                    new GeminiPart(Text: VoiceUtterancePrompt.Build(transcript.Trim())),
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

        return text;
    }

    private static object BuildSchema() => new
    {
        type = "object",
        properties = new
        {
            name = new { type = "string", nullable = true },
            brand = new { type = "string", nullable = true },
            unitPrice = new { type = "number", nullable = true },
            quantity = new { type = "integer", nullable = true },
            quantityValue = new { type = "number", nullable = true },
            quantityUnit = new { type = "string", nullable = true },
        },
    };

    private sealed record GeminiRequest(
        GeminiContent[] Contents,
        GeminiGenerationConfig GenerationConfig);

    private sealed record GeminiContent(GeminiPart[] Parts);

    private sealed record GeminiPart(string? Text = null);

    private sealed record GeminiGenerationConfig(string ResponseMimeType, object ResponseSchema);

    private sealed record GeminiResponse(GeminiCandidate[]? Candidates);

    private sealed record GeminiCandidate(GeminiContent? Content);
}

internal static class VoiceUtterancePrompt
{
    public static string Build(string transcript) =>
        $"""
        Você extrai campos de produto de mercado a partir de fala em português do Brasil.
        Retorne somente JSON válido conforme o schema.
        Preços falados em reais (ex.: "nove e noventa" = 9.90).
        Unidades: g, kg, ml, l, un.
        Texto do usuário:
        {transcript}
        """;
}
