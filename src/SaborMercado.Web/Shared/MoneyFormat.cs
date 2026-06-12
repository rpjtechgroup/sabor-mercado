using System.Globalization;

namespace SaborMercado.Web.Shared;

/// <summary>
/// Formatação e parsing de dinheiro em cultura pt-BR (ex.: "R$ 8,99").
/// Dinheiro é sempre decimal (docs/standards/frontend-standards.md).
/// </summary>
public static class MoneyFormat
{
    public static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("pt-BR");

    public static string Format(decimal value) => value.ToString("C2", Culture);

    /// <summary>Valor sem símbolo de moeda (ex.: "8,99"), para inputs.</summary>
    public static string FormatPlain(decimal value) => value.ToString("N2", Culture);

    public static bool TryParse(string? text, out decimal value)
    {
        value = 0m;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var cleaned = text.Replace("R$", string.Empty, StringComparison.OrdinalIgnoreCase).Trim();
        return decimal.TryParse(cleaned, NumberStyles.Number, Culture, out value);
    }
}
