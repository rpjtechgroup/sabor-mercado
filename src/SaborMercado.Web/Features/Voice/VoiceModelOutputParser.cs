using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using SaborMercado.Web.Domain.Catalog;

namespace SaborMercado.Web.Features.Voice;

public enum VoiceExtractionSource
{
    Gemini,
    DeterministicFallback,
}

public sealed record VoiceFieldExtractionResult(
    VoiceParsedFields Fields,
    VoiceExtractionSource Source,
    string? ErrorMessage = null);

public static partial class VoiceModelOutputParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static VoiceFieldExtractionResult Parse(string transcript, string? modelOutput)
    {
        var rules = VoiceUtteranceParser.Parse(transcript);
        if (string.IsNullOrWhiteSpace(modelOutput))
        {
            return new(rules, VoiceExtractionSource.DeterministicFallback, "O Gemini não retornou conteúdo estruturado.");
        }

        var fromModel = TryParseModelJson(modelOutput);
        if (fromModel is null)
        {
            return new(
                rules,
                VoiceExtractionSource.DeterministicFallback,
                "Não foi possível interpretar o JSON retornado pelo Gemini.");
        }

        return new(Merge(fromModel, rules), VoiceExtractionSource.Gemini);
    }

    private static VoiceParsedFields? TryParseModelJson(string modelOutput)
    {
        var json = ExtractJsonPayload(modelOutput);
        if (json is null)
        {
            return null;
        }

        try
        {
            var dto = JsonSerializer.Deserialize<VoiceModelDto>(json, JsonOptions);
            if (dto is null)
            {
                return null;
            }

            var name = NormalizeOptional(dto.Name) ?? NormalizeOptional(dto.ProductName);
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            return new VoiceParsedFields
            {
                Name = name,
                Brand = NormalizeOptional(dto.Brand),
                UnitPrice = dto.UnitPrice is >= 0 ? (decimal)dto.UnitPrice.Value : null,
                Quantity = dto.Quantity is > 0 ? dto.Quantity : null,
                QuantityValue = dto.QuantityValue is >= 0 ? (decimal)dto.QuantityValue.Value : null,
                QuantityUnit = ParseUnit(dto.QuantityUnit),
                Ean = NormalizeEan(dto.Ean),
                Category = NormalizeOptional(dto.Category),
                Notes = NormalizeOptional(dto.Notes),
            };
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static VoiceParsedFields Merge(VoiceParsedFields model, VoiceParsedFields rules) =>
        new()
        {
            Name = string.IsNullOrWhiteSpace(model.Name) ? rules.Name : model.Name.Trim(),
            Brand = string.IsNullOrWhiteSpace(model.Brand)
                ? NormalizeOptional(rules.Brand)
                : model.Brand.Trim(),
            UnitPrice = model.UnitPrice ?? rules.UnitPrice,
            Quantity = model.Quantity ?? rules.Quantity,
            QuantityValue = model.QuantityValue ?? rules.QuantityValue,
            QuantityUnit = model.QuantityUnit ?? rules.QuantityUnit,
            Ean = NormalizeEan(model.Ean) ?? NormalizeEan(rules.Ean),
            Category = NormalizeOptional(model.Category) ?? NormalizeOptional(rules.Category),
            Notes = NormalizeOptional(model.Notes) ?? NormalizeOptional(rules.Notes),
        };

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeEan(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var digits = new string(value.Where(char.IsDigit).ToArray());
        return digits.Length is >= 8 and <= 14 ? digits : null;
    }

    private static QuantityUnit? ParseUnit(string? unit) =>
        unit?.Trim().ToLowerInvariant() switch
        {
            "g" => QuantityUnit.G,
            "kg" => QuantityUnit.Kg,
            "ml" => QuantityUnit.Ml,
            "l" => QuantityUnit.L,
            "un" => QuantityUnit.Un,
            _ => null,
        };

    private static string? ExtractJsonPayload(string text)
    {
        var trimmed = text.Trim();
        if (trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            var firstLineEnd = trimmed.IndexOf('\n');
            if (firstLineEnd >= 0)
            {
                trimmed = trimmed[(firstLineEnd + 1)..];
            }

            var fence = trimmed.LastIndexOf("```", StringComparison.Ordinal);
            if (fence >= 0)
            {
                trimmed = trimmed[..fence];
            }

            trimmed = trimmed.Trim();
        }

        if (trimmed.StartsWith("{", StringComparison.Ordinal))
        {
            return trimmed;
        }

        var match = JsonObjectRegex().Match(trimmed);
        return match.Success ? match.Value : null;
    }

    [GeneratedRegex(@"\{[\s\S]*\}", RegexOptions.CultureInvariant)]
    private static partial Regex JsonObjectRegex();

    private sealed class VoiceModelDto
    {
        public string? Name { get; set; }

        public string? ProductName { get; set; }

        public string? Brand { get; set; }

        public double? UnitPrice { get; set; }

        public int? Quantity { get; set; }

        public double? QuantityValue { get; set; }

        public string? QuantityUnit { get; set; }

        public string? Ean { get; set; }

        public string? Category { get; set; }

        public string? Notes { get; set; }
    }
}
