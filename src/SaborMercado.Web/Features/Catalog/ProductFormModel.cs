using SaborMercado.Web.Domain.Catalog;

namespace SaborMercado.Web.Features.Catalog;

public sealed class ProductFormModel
{
    public string Name { get; set; } = string.Empty;

    public string? Brand { get; set; }

    public decimal? QuantityValue { get; set; }

    public QuantityUnit? QuantityUnit { get; set; }

    public string? Ean { get; set; }

    public string? Category { get; set; }

    public string? Notes { get; set; }

    public Guid StoreId { get; set; }

    public static ProductFormModel FromProduct(Product product) => new()
    {
        Name = product.Name,
        Brand = product.Brand,
        QuantityValue = product.QuantityValue,
        QuantityUnit = product.QuantityUnit,
        Ean = product.Ean,
        Category = product.Category,
        Notes = product.Notes,
        StoreId = product.StoreId,
    };

    public void ApplyTo(Product product)
    {
        product.Name = Name.Trim();
        product.Brand = Normalize(Brand);
        product.QuantityValue = QuantityValue;
        product.QuantityUnit = QuantityUnit;
        product.Ean = Normalize(Ean);
        product.Category = Normalize(Category);
        product.Notes = Normalize(Notes);
        product.StoreId = StoreId;
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
