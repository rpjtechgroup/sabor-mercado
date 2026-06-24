using System.Net.Http.Json;
using System.Text.Json;

namespace SaborMercado.Shared.Gemini;

public sealed record GeminiGenerateContentResult(string ModelUsed, string Payload);

public static class GeminiGenerateContentExecutor
{
    public static async Task<GeminiGenerateContentResult> PostJsonAsync<TBody>(
        HttpClient httpClient,
        string apiKey,
        IReadOnlyList<string> models,
        TBody body,
        JsonSerializerOptions jsonOptions,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Gemini API key is required.");
        }

        if (models.Count == 0)
        {
            throw new InvalidOperationException("Nenhum modelo Gemini configurado.");
        }

        HttpRequestException? lastError = null;

        foreach (var model in models)
        {
            var requestUri = BuildRequestUri(model, apiKey);
            using var response = await httpClient.PostAsJsonAsync(requestUri, body, jsonOptions, cancellationToken);
            var payload = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return new GeminiGenerateContentResult(model, payload);
            }

            if (!GeminiApiFailures.ShouldTryNextModel((int)response.StatusCode, payload))
            {
                throw new HttpRequestException($"Gemini HTTP {(int)response.StatusCode} ({model}): {payload}");
            }

            lastError = new HttpRequestException($"Gemini HTTP {(int)response.StatusCode} ({model}): {payload}");
        }

        throw lastError ?? new HttpRequestException("Nenhum modelo Gemini disponível na cadeia de fallback.");
    }

    public static string BuildRequestUri(string model, string apiKey) =>
        $"v1beta/models/{model}:generateContent?key={Uri.EscapeDataString(apiKey.Trim())}";
}
