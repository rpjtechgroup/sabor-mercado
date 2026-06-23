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
        VoiceExtractionTarget target = VoiceExtractionTarget.ProductCatalog,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Gemini API key is required.");
        }

        var model = configuration["GeminiModel"] ?? "gemini-2.0-flash";
        var requestUri = BuildRequestUri(model, apiKey);

        var body = new GeminiRequest(
            [
                new GeminiContent(
                [
                    new GeminiPart(Text: VoiceUtterancePrompt.Build(target, transcript.Trim())),
                ]),
            ],
            new GeminiGenerationConfig(
                ResponseMimeType: "application/json",
                ResponseSchema: VoiceUtterancePrompt.BuildSchema(target)));

        using var response = await httpClient.PostAsJsonAsync(requestUri, body, JsonOptions, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Gemini HTTP {(int)response.StatusCode}: {payload}");
        }

        var gemini = JsonSerializer.Deserialize<GeminiResponse>(payload, JsonOptions)
            ?? throw new InvalidOperationException("Resposta vazia do Gemini.");

        var candidate = gemini.Candidates?.FirstOrDefault();
        var text = candidate?.Content?.Parts?.FirstOrDefault()?.Text;
        if (string.IsNullOrWhiteSpace(text))
        {
            var reason = candidate?.FinishReason ?? "desconhecido";
            throw new InvalidOperationException($"Gemini não retornou JSON estruturado (motivo: {reason}).");
        }

        return text;
    }

    public async Task ValidateApiKeyAsync(string apiKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Informe a chave da API Gemini.");
        }

        var model = configuration["GeminiModel"] ?? "gemini-2.0-flash";
        var requestUri = BuildRequestUri(model, apiKey);
        var body = new GeminiRequest(
            [new GeminiContent([new GeminiPart(Text: "Responda apenas OK.")])],
            null);

        using var response = await httpClient.PostAsJsonAsync(requestUri, body, JsonOptions, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Gemini HTTP {(int)response.StatusCode}: {payload}");
        }
    }

    private static string BuildRequestUri(string model, string apiKey) =>
        $"v1beta/models/{model}:generateContent?key={Uri.EscapeDataString(apiKey.Trim())}";

    private sealed record GeminiRequest(
        GeminiContent[] Contents,
        GeminiGenerationConfig? GenerationConfig);

    private sealed record GeminiContent(GeminiPart[] Parts);

    private sealed record GeminiPart(string? Text = null);

    private sealed record GeminiGenerationConfig(string ResponseMimeType, object ResponseSchema);

    private sealed record GeminiResponse(GeminiCandidate[]? Candidates);

    private sealed record GeminiCandidate(GeminiContent? Content, string? FinishReason);
}
