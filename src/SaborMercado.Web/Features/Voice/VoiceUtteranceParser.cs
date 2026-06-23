using System.Text.RegularExpressions;
using SaborMercado.Web.Domain.Catalog;
using SaborMercado.Web.Shared;

namespace SaborMercado.Web.Features.Voice;

public sealed class VoiceParsedFields
{
    public string Name { get; set; } = string.Empty;

    public string? Brand { get; set; }

    public decimal? UnitPrice { get; set; }

    public int? Quantity { get; set; }

    public decimal? QuantityValue { get; set; }

    public QuantityUnit? QuantityUnit { get; set; }

    public string? Ean { get; set; }

    public string? Category { get; set; }

    public string? Notes { get; set; }
}

public static partial class VoiceUtteranceParser
{
    private static readonly Dictionary<string, int> CardinalWords = new(StringComparer.OrdinalIgnoreCase)
    {
        ["zero"] = 0,
        ["um"] = 1,
        ["uma"] = 1,
        ["dois"] = 2,
        ["duas"] = 2,
        ["tres"] = 3,
        ["três"] = 3,
        ["quatro"] = 4,
        ["cinco"] = 5,
        ["seis"] = 6,
        ["sete"] = 7,
        ["oito"] = 8,
        ["nove"] = 9,
        ["dez"] = 10,
        ["onze"] = 11,
        ["doze"] = 12,
        ["treze"] = 13,
        ["quatorze"] = 14,
        ["catorze"] = 14,
        ["quinze"] = 15,
        ["dezesseis"] = 16,
        ["dezessete"] = 17,
        ["dezoito"] = 18,
        ["dezenove"] = 19,
        ["vinte"] = 20,
        ["trinta"] = 30,
        ["quarenta"] = 40,
        ["cinquenta"] = 50,
        ["sessenta"] = 60,
        ["setenta"] = 70,
        ["oitenta"] = 80,
        ["noventa"] = 90,
    };

    public static VoiceParsedFields Parse(string? utterance)
    {
        var result = new VoiceParsedFields();
        if (string.IsNullOrWhiteSpace(utterance))
        {
            return result;
        }

        var text = Normalize(utterance);
        if (string.IsNullOrWhiteSpace(text))
        {
            return result;
        }

        text = ExtractPrice(ref result, text);
        text = ExtractPackQuantity(ref result, text);
        text = ExtractMeasure(ref result, text);

        result.Name = CleanupName(text);
        return result;
    }

    private static string Normalize(string utterance) =>
        utterance.Trim().ToLowerInvariant().Replace("r$", string.Empty, StringComparison.Ordinal);

    private static string ExtractPrice(ref VoiceParsedFields result, string text)
    {
        var priceMatch = PriceSuffixRegex().Match(text);
        if (priceMatch.Success)
        {
            if (TryParsePriceToken(priceMatch.Groups["price"].Value, out var price))
            {
                result.UnitPrice = price;
            }

            text = text[..priceMatch.Index].Trim();
            return text;
        }

        var currencyMatch = CurrencyRegex().Match(text);
        if (currencyMatch.Success && MoneyFormat.TryParse(currencyMatch.Groups["amount"].Value, out var parsed))
        {
            result.UnitPrice = parsed;
            text = text.Remove(currencyMatch.Index, currencyMatch.Length).Trim();
            return text;
        }

        var trailingMatch = TrailingSpokenPriceRegex().Match(text);
        if (trailingMatch.Success && TryParsePriceToken(trailingMatch.Groups["price"].Value, out var spoken))
        {
            result.UnitPrice = spoken;
            text = text[..trailingMatch.Index].Trim();
        }

        return text;
    }

    private static string ExtractPackQuantity(ref VoiceParsedFields result, string text)
    {
        var match = PackQuantityRegex().Match(text);
        if (!match.Success)
        {
            return text;
        }

        if (TryParseNumberToken(match.Groups["qty"].Value, out var qty) && qty > 0)
        {
            result.Quantity = qty;
        }

        return text.Remove(match.Index, match.Length).Trim();
    }

    private static string ExtractMeasure(ref VoiceParsedFields result, string text)
    {
        var match = MeasureRegex().Match(text);
        if (!match.Success)
        {
            return text;
        }

        if (TryParseNumberToken(match.Groups["qty"].Value, out var qty) && qty > 0)
        {
            result.QuantityValue = qty;
        }

        result.QuantityUnit = match.Groups["unit"].Value switch
        {
            "quilo" or "quilos" or "kg" => QuantityUnit.Kg,
            "grama" or "gramas" or "g" => QuantityUnit.G,
            "litro" or "litros" or "l" => QuantityUnit.L,
            "ml" or "mililitro" or "mililitros" => QuantityUnit.Ml,
            _ => QuantityUnit.Un,
        };

        return text.Remove(match.Index, match.Length).Trim();
    }

    private static bool TryParsePriceToken(string token, out decimal price)
    {
        price = 0m;
        token = token.Trim();
        if (MoneyFormat.TryParse(token, out price))
        {
            return true;
        }

        var parts = token.Split(" e ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 2 &&
            TryParseNumberToken(parts[0], out var reais) &&
            TryParseCentavosToken(parts[1], out var centavos))
        {
            price = reais + centavos / 100m;
            return true;
        }

        if (TryParseNumberToken(token, out var only))
        {
            price = only;
            return true;
        }

        return false;
    }

    private static bool TryParseCentavosToken(string token, out int centavos)
    {
        centavos = 0;
        if (TryParseNumberToken(token, out centavos))
        {
            return centavos is >= 0 and < 100;
        }

        return false;
    }

    private static bool TryParseNumberToken(string token, out int value)
    {
        value = 0;
        token = token.Trim();
        if (int.TryParse(token, out value))
        {
            return true;
        }

        if (token.Equals("meio", StringComparison.OrdinalIgnoreCase) ||
            token.Equals("meia", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return CardinalWords.TryGetValue(token, out value);
    }

    private static string CleanupName(string text)
    {
        text = Regex.Replace(text, @"\s+", " ").Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        return char.ToUpper(text[0], MoneyFormat.Culture) + text[1..];
    }

    [GeneratedRegex(@"(?<price>[\d]+(?:[,\.]\d{1,2})?|\w+(?:\s+e\s+\w+)?)\s*(?:reais?|real)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex PriceSuffixRegex();

    [GeneratedRegex(@"\s+(?<price>\d+(?:[,\.]\d{1,2})?|\w+(?:\s+e\s+\w+)?)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex TrailingSpokenPriceRegex();

    [GeneratedRegex(@"(?:r\$?\s*)(?<amount>[\d]+(?:[,\.]\d{1,2})?)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex CurrencyRegex();

    [GeneratedRegex(@"(?<qty>\d+|\w+)\s*(?:unidades?|un\.?)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex PackQuantityRegex();

    [GeneratedRegex(@"(?:(?<qty>\d+|\w+|meio|meia)\s+)?(?<unit>quilo?s?|kg|gramas?|g|litros?|l|ml|mililitros?)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex MeasureRegex();
}
