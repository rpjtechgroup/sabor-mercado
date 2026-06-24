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
        var rules = VoiceUtteranceParser.Parse(transcript);
        if (string.IsNullOrWhiteSpace(transcript))
        {
            return new(rules, VoiceExtractionSource.DeterministicFallback);
        }

        var apiKey = await preferences.GetGeminiApiKeyAsync();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return new(
                rules,
                VoiceExtractionSource.DeterministicFallback,
                "Chave Gemini não encontrada. Salve a chave em Configurações e tente novamente.");
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
        catch (HttpRequestException ex)
        {
            return new(rules, VoiceExtractionSource.DeterministicFallback, DescribeHttpError(ex));
        }
        catch (TaskCanceledException)
        {
            return new(
                rules,
                VoiceExtractionSource.DeterministicFallback,
                "Tempo esgotado ao contactar o Gemini. Verifique a conexão e tente novamente.");
        }
        catch (InvalidOperationException ex)
        {
            return new(rules, VoiceExtractionSource.DeterministicFallback, ex.Message);
        }
    }

    private static string DescribeHttpError(HttpRequestException ex)
    {
        var message = ex.Message;
        if (message.Contains("API_KEY_INVALID", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("API key not valid", StringComparison.OrdinalIgnoreCase))
        {
            return "Chave Gemini inválida. Gere uma nova chave no Google AI Studio e salve em Configurações.";
        }

        if (message.Contains("FAILED_PRECONDITION", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("User location is not supported", StringComparison.OrdinalIgnoreCase))
        {
            return "O Gemini não está disponível para esta região ou conta.";
        }

        if (message.Contains("RESOURCE_EXHAUSTED", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("quota", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("RATE_LIMIT", StringComparison.OrdinalIgnoreCase))
        {
            return "Cota do modelo Gemini esgotada. O app tenta modelos alternativos automaticamente; tente de novo em alguns minutos.";
        }

        return string.IsNullOrWhiteSpace(message)
            ? "Falha ao contactar o Gemini."
            : message;
    }
}
