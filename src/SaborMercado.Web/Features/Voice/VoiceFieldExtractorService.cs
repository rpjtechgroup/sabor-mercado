using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Features.Voice;

public sealed class VoiceFieldExtractorService(
    IPreferencesStore preferences,
    GeminiVoiceUtteranceClient gemini)
{
    public async Task<VoiceFieldExtractionResult> ExtractAsync(
        string transcript,
        VoiceExtractionTarget target = VoiceExtractionTarget.ProductCatalog,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(transcript))
        {
            return Fallback(transcript);
        }

        var apiKey = await preferences.GetGeminiApiKeyAsync();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return Fallback(transcript);
        }

        try
        {
            var modelOutput = await gemini.ExtractProductFieldsAsync(
                apiKey,
                transcript.Trim(),
                target,
                cancellationToken);
            return VoiceModelOutputParser.Parse(transcript, modelOutput);
        }
        catch
        {
            return Fallback(transcript);
        }
    }

    private static VoiceFieldExtractionResult Fallback(string transcript) =>
        new(VoiceUtteranceParser.Parse(transcript), VoiceExtractionSource.DeterministicFallback);
}
