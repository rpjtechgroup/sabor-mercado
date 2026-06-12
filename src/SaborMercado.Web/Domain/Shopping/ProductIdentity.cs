using System.Globalization;
using SaborMercado.Web.Domain.Catalog;
using SaborMercado.Web.Shared;

namespace SaborMercado.Web.Domain.Shopping;

public static class ProductIdentity
{
    public static string FromSnapshot(ProductSnapshot snapshot)
    {
        var name = Normalize(snapshot.Name);
        var brand = Normalize(snapshot.Brand) ?? string.Empty;
        var qty = snapshot.QuantityValue?.ToString("0.####", CultureInfo.InvariantCulture) ?? string.Empty;
        var unit = snapshot.QuantityUnit?.Label() ?? string.Empty;
        return $"{name}|{brand}|{qty}|{unit}";
    }

    public static string FromProduct(Product product)
    {
        var snapshot = ProductSnapshot.FromProduct(product);
        return FromSnapshot(snapshot);
    }

    public static string GetLineKey(Guid? productId, ProductSnapshot snapshot) =>
        productId?.ToString("N") ?? FromSnapshot(snapshot);

    public static bool Matches(Product product, ProductSnapshot snapshot) =>
        string.Equals(FromProduct(product), FromSnapshot(snapshot), StringComparison.Ordinal);

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpper(MoneyFormat.Culture);
}
