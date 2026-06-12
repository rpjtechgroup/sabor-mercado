using SaborMercado.Web.Domain.Catalog;

namespace SaborMercado.Web.Domain.Shopping;

public sealed record ProductSnapshot(
    string Name,
    string? Brand,
    decimal? QuantityValue,
    QuantityUnit? QuantityUnit)
{
    public static ProductSnapshot FromProduct(Product product) =>
        new(product.Name, product.Brand, product.QuantityValue, product.QuantityUnit);
}
