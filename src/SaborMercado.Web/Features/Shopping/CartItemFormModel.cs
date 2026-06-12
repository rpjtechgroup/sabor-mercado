using SaborMercado.Web.Domain.Catalog;
using SaborMercado.Web.Domain.Shopping;

namespace SaborMercado.Web.Features.Shopping;

public sealed class CartItemFormModel
{
    public string Name { get; set; } = string.Empty;

    public string? Brand { get; set; }

    public decimal? QuantityValue { get; set; }

    public QuantityUnit? QuantityUnit { get; set; }

    public decimal? UnitPrice { get; set; }

    public int Quantity { get; set; } = 1;

    public ProductSnapshot ToSnapshot() =>
        new(Name.Trim(),
            string.IsNullOrWhiteSpace(Brand) ? null : Brand.Trim(),
            QuantityValue,
            QuantityUnit);

    public static CartItemFormModel FromItem(CartItem item) => new()
    {
        Name = item.ProductSnapshot.Name,
        Brand = item.ProductSnapshot.Brand,
        QuantityValue = item.ProductSnapshot.QuantityValue,
        QuantityUnit = item.ProductSnapshot.QuantityUnit,
        UnitPrice = item.UnitPrice,
        Quantity = item.Quantity,
    };
}
