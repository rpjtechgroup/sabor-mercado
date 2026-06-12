using System.Globalization;
using System.Text;
using SaborMercado.Web.Domain.Shopping;
using SaborMercado.Web.Shared;

namespace SaborMercado.Web.Features.Shopping;

public static class CartCsvExporter
{
    public static string Build(IEnumerable<CartItem> items, string? marketName, decimal total)
    {
        var builder = new StringBuilder();
        builder.AppendLine("produto;marca;quantidade;preco_unitario;subtotal;origem");

        foreach (var item in items)
        {
            var line = string.Join(
                ';',
                Escape(item.ProductSnapshot.Name),
                Escape(item.ProductSnapshot.Brand ?? string.Empty),
                item.Quantity.ToString(CultureInfo.InvariantCulture),
                item.UnitPrice.ToString("F2", CultureInfo.InvariantCulture),
                item.Subtotal.ToString("F2", CultureInfo.InvariantCulture),
                item.Source.ToString());
            builder.AppendLine(line);
        }

        builder.AppendLine();
        builder.AppendLine($"mercado;{Escape(marketName ?? string.Empty)}");
        builder.AppendLine($"total;{total.ToString("F2", CultureInfo.InvariantCulture)}");
        builder.AppendLine($"exportado_em;{DateTimeOffset.Now.ToString("O", MoneyFormat.Culture)}");
        return builder.ToString();
    }

    private static string Escape(string value) =>
        value.Contains(';') || value.Contains('"') || value.Contains('\n')
            ? $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\""
            : value;
}
