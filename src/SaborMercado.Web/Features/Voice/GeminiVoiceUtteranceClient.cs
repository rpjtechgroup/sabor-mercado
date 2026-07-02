using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using SaborMercado.Shared.Gemini;

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
        VoiceExtractionTarget target = VoiceExtractionTarget.CartItem,
        CancellationToken cancellationToken = default)
    {
        var models = ResolveModels();
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

        var result = await GeminiGenerateContentExecutor.PostJsonAsync(
            httpClient,
            apiKey,
            models,
            body,
            JsonOptions,
            cancellationToken);

        var gemini = JsonSerializer.Deserialize<GeminiResponse>(result.Payload, JsonOptions)
            ?? throw new InvalidOperationException("Resposta vazia do Gemini.");

        var candidate = gemini.Candidates?.FirstOrDefault();
        var text = candidate?.Content?.Parts?.FirstOrDefault()?.Text;
        if (string.IsNullOrWhiteSpace(text))
        {
            var reason = candidate?.FinishReason ?? "desconhecido";
            throw new InvalidOperationException(
                $"Gemini ({result.ModelUsed}) não retornou JSON estruturado (motivo: {reason}).");
        }

        return text;
    }

    public async Task ValidateApiKeyAsync(string apiKey, CancellationToken cancellationToken = default)
    {
        var models = ResolveModels();
        var body = new GeminiRequest(
            [new GeminiContent([new GeminiPart(Text: "Responda apenas OK.")])],
            null);

        _ = await GeminiGenerateContentExecutor.PostJsonAsync(
            httpClient,
            apiKey,
            models,
            body,
            JsonOptions,
            cancellationToken);
    }

    private IReadOnlyList<string> ResolveModels() =>
        GeminiModelChain.Build(configuration["GeminiModel"], configuration["GeminiModelFallbacks"]);

    private sealed record GeminiRequest(
        GeminiContent[] Contents,
        GeminiGenerationConfig? GenerationConfig);

    private sealed record GeminiContent(GeminiPart[] Parts);

    private sealed record GeminiPart(string? Text = null);

    private sealed record GeminiGenerationConfig(string ResponseMimeType, object ResponseSchema);

    private sealed record GeminiResponse(GeminiCandidate[]? Candidates);

    private sealed record GeminiCandidate(GeminiContent? Content, string? FinishReason);
}
