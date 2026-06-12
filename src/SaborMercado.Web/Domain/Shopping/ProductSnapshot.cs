using SaborMercado.Web.Domain.Catalog;

namespace SaborMercado.Web.Domain.Shopping;

/// <summary>
/// Cópia imutável dos dados do produto no momento da adição ao carrinho.
/// Alterações/exclusões posteriores no catálogo não afetam o item.
/// </summary>
public sealed record ProductSnapshot(
    string Name,
    string? Brand,
    decimal? QuantityValue,
    QuantityUnit? QuantityUnit)
{
    public static ProductSnapshot FromProduct(Product product) =>
        new(product.Name, product.Brand, product.QuantityValue, product.QuantityUnit);
}
