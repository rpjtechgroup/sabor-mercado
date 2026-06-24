namespace SaborMercado.Shared.Gemini;

/// <summary>
/// Ordem de modelos para tier gratuito (fonte: taxa_uso_gemini_20260624.csv).
/// RPM / RPD por modelo: 2.5 Flash 5/20, 2.5 Flash Lite 10/20, 3.1 Flash Lite 15/500, 3 Flash 5/20, 3.5 Flash 5/20.
/// </summary>
public static class GeminiModelChain
{
    public const string DefaultPrimary = "gemini-2.5-flash";

    public static readonly string[] DefaultFallbackModels =
    [
        "gemini-2.5-flash",
        "gemini-2.5-flash-lite",
        "gemini-3.1-flash-lite",
        "gemini-3-flash",
        "gemini-3.5-flash",
    ];

    public static IReadOnlyList<string> Build(string? primaryModel, string? commaSeparatedFallbacks)
    {
        var models = new List<string>();

        if (!string.IsNullOrWhiteSpace(primaryModel))
        {
            AddUnique(models, primaryModel);
        }

        if (!string.IsNullOrWhiteSpace(commaSeparatedFallbacks))
        {
            foreach (var model in commaSeparatedFallbacks.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                AddUnique(models, model);
            }
        }

        foreach (var model in DefaultFallbackModels)
        {
            AddUnique(models, model);
        }

        return models;
    }

    private static void AddUnique(List<string> models, string model)
    {
        var normalized = model.Trim();
        if (normalized.Length == 0)
        {
            return;
        }

        if (models.Contains(normalized, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        models.Add(normalized);
    }
}
