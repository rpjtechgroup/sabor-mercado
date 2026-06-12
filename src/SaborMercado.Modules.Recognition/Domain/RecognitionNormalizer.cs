using System.Globalization;
using System.Text.RegularExpressions;
using SaborMercado.Shared.Recognition;

namespace SaborMercado.Modules.Recognition.Domain;

public static partial class RecognitionNormalizer
{
    private static readonly TextInfo TextInfo = CultureInfo.GetCultureInfo("pt-BR").TextInfo;

    public static RecognitionResultDto Normalize(RecognitionResultDto raw)
    {
        var name = NormalizeName(raw.ProductName);
        var brand = NormalizeName(raw.Brand);
        var unit = NormalizeUnit(raw.QuantityUnit);
        var price = raw.Price is { } p ? NormalizePrice(p, raw.RawText) : TryParsePriceFromRaw(raw.RawText);
        var ean = EanValidator.Normalize(raw.Ean);
        var confidence = Math.Clamp(raw.Confidence, 0m, 1m);

        return raw with
        {
            ProductName = name,
            Brand = brand,
            QuantityUnit = unit,
            Price = price,
            Ean = ean,
            Confidence = confidence,
        };
    }

    public static string? NormalizeName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var collapsed = Whitespace().Replace(value.Trim(), " ");
        return TextInfo.ToTitleCase(collapsed.ToLowerInvariant());
    }

    public static string? NormalizeUnit(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var token = value.Trim().TrimEnd('.').ToLowerInvariant();
        return token switch
        {
            "g" or "gr" or "grama" or "gramas" => "g",
            "kg" or "quilo" or "quilos" or "kilograma" or "kilogramas" => "kg",
            "ml" or "mililitro" or "mililitros" => "ml",
            "l" or "lt" or "litro" or "litros" => "l",
            "un" or "und" or "unid" or "unidade" or "unidades" => "un",
            _ => null,
        };
    }

    public static decimal? NormalizePrice(decimal value, string? rawText)
    {
        if (value < 0m)
        {
            return TryParsePriceFromRaw(rawText);
        }

        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    public static decimal? TryParsePriceFromRaw(string? rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
        {
            return null;
        }

        var match = PricePattern().Match(rawText);
        if (!match.Success)
        {
            return null;
        }

        var digits = match.Groups["amount"].Value.Replace('.', ' ').Replace(',', '.').Replace(" ", "");
        return decimal.TryParse(digits, NumberStyles.Number, CultureInfo.InvariantCulture, out var price)
            ? Math.Round(price, 2, MidpointRounding.AwayFromZero)
            : null;
    }

    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    private static partial Regex Whitespace();

    [GeneratedRegex(@"R\$\s*(?<amount>\d{1,3}(?:[.\s]\d{3})*(?:,\d{2})|\d+(?:,\d{2})?)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex PricePattern();
}
