using System.Globalization;
using System.Text.RegularExpressions;

namespace SaborMercado.Modules.SharedCatalog.Domain;

public static partial class ProductNormalizer
{
    public static string NormalizeName(string value)
    {
        var collapsed = Whitespace().Replace(value.Trim(), " ");
        return collapsed.ToUpperInvariant();
    }

    public static string? NormalizeEan(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var digits = new string(value.Where(char.IsDigit).ToArray());
        return digits.Length is 8 or 13 ? digits : null;
    }

    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    private static partial Regex Whitespace();
}
