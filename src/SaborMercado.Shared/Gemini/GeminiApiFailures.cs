namespace SaborMercado.Shared.Gemini;

public static class GeminiApiFailures
{
    public static bool ShouldTryNextModel(int statusCode, string? responseBody)
    {
        if (IsApiKeyError(responseBody))
        {
            return false;
        }

        return statusCode switch
        {
            404 or 429 or 503 => true,
            400 => IsModelOrQuotaError(responseBody),
            _ => false,
        };
    }

    public static bool IsApiKeyError(string? responseBody) =>
        Contains(responseBody, "API_KEY_INVALID") ||
        Contains(responseBody, "API key not valid");

    private static bool IsModelOrQuotaError(string? responseBody) =>
        Contains(responseBody, "RESOURCE_EXHAUSTED") ||
        Contains(responseBody, "RATE_LIMIT") ||
        Contains(responseBody, "quota") ||
        Contains(responseBody, "NOT_FOUND") && Contains(responseBody, "model");

    private static bool Contains(string? haystack, string needle) =>
        !string.IsNullOrWhiteSpace(haystack) &&
        haystack.Contains(needle, StringComparison.OrdinalIgnoreCase);
}
