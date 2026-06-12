using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Infrastructure;

public sealed class SaborMercadoApiClient(HttpClient httpClient, IPreferencesStore preferences)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    public bool IsConfigured => httpClient.BaseAddress is not null;

    public async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string path,
        object? body = null,
        bool requireAuth = false,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(method, path);

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            request.Headers.TryAddWithoutValidation("Idempotency-Key", idempotencyKey);
        }

        if (requireAuth)
        {
            var token = await preferences.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new InvalidOperationException("Login necessário para esta ação.");
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }

        return await httpClient.SendAsync(request, cancellationToken);
    }

    public async Task<T?> ReadJsonAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.Content.Headers.ContentLength == 0)
        {
            return default;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, cancellationToken);
    }

    public async Task<string?> ReadErrorDetailAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var problem = await ReadJsonAsync<ApiProblemDetails>(response, cancellationToken);
            return problem?.Detail ?? problem?.Title;
        }
        catch
        {
            return response.ReasonPhrase;
        }
    }

    private sealed class ApiProblemDetails
    {
        public string? Title { get; set; }

        public string? Detail { get; set; }
    }
}
