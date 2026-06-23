using SaborMercado.Web.Interop;

namespace SaborMercado.Web.Features.Voice;

public sealed class VoiceFieldExtractorService(IVoiceFieldExtractorInterop interop)
{
    public async Task<VoiceFieldExtractionResult> ExtractAsync(string transcript)
    {
        if (string.IsNullOrWhiteSpace(transcript))
        {
            return new(VoiceUtteranceParser.Parse(transcript), VoiceExtractionSource.DeterministicFallback);
        }

        try
        {
            if (!await interop.IsModelSupportedAsync())
            {
                return Fallback(transcript);
            }

            var modelOutput = await interop.ExtractProductFieldsAsync(transcript.Trim());
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
