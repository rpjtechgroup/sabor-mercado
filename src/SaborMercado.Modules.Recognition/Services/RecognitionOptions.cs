using SaborMercado.Shared.Gemini;

namespace SaborMercado.Modules.Recognition.Services;

public sealed class RecognitionOptions
{
    public const string SectionName = "Recognition";

    public string GeminiModel { get; set; } = GeminiModelChain.DefaultPrimary;

    public string? GeminiModelFallbacks { get; set; }

    public string? GeminiApiKey { get; set; }

    public int MaxUploadBytes { get; set; } = 4 * 1024 * 1024;

    public int DailyGlobalQuota { get; set; } = 1200;

    public int PerClientDailyQuota { get; set; } = 150;
}
