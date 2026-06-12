using SaborMercado.Web.Contracts.Recognition;
using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Features.Recognition;

public sealed class RecognitionService(
    IPreferencesStore preferences,
    GeminiShelfLabelClient gemini)
{
    public async Task<RecognitionApiResult> RecognizeAsync(
        byte[] jpegBytes,
        CancellationToken cancellationToken = default)
    {
        var apiKey = await preferences.GetGeminiApiKeyAsync();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return RecognitionApiResult.MissingApiKey();
        }

        try
        {
            var result = await gemini.RecognizeAsync(apiKey, jpegBytes, cancellationToken);
            return RecognitionApiResult.Success(result);
        }
        catch (HttpRequestException)
        {
            return RecognitionApiResult.Unavailable();
        }
        catch (TaskCanceledException)
        {
            return RecognitionApiResult.Unavailable();
        }
        catch (InvalidOperationException)
        {
            return RecognitionApiResult.Unavailable();
        }
    }
}
