using SaborMercado.Web.Features.Catalog;
using SaborMercado.Web.Features.Shopping;

namespace SaborMercado.Web.Features.Voice;

public static class VoiceParsedFieldsApplicator
{
    public static void ApplyToCartItemForm(VoiceParsedFields parsed, CartItemFormModel model)
    {
        if (!string.IsNullOrWhiteSpace(parsed.Name))
        {
            model.Name = parsed.Name;
        }

        if (!string.IsNullOrWhiteSpace(parsed.Brand))
        {
            model.Brand = parsed.Brand;
        }

        if (parsed.QuantityValue is > 0m)
        {
            model.QuantityValue = parsed.QuantityValue;
        }

        if (parsed.QuantityUnit is not null)
        {
            model.QuantityUnit = parsed.QuantityUnit;
        }

        if (parsed.UnitPrice is >= 0m)
        {
            model.UnitPrice = parsed.UnitPrice;
        }

        if (parsed.Quantity is > 0)
        {
            model.Quantity = parsed.Quantity.Value;
        }
    }

    public static void ApplyToProductForm(VoiceParsedFields parsed, ProductFormModel model)
    {
        if (!string.IsNullOrWhiteSpace(parsed.Name))
        {
            model.Name = parsed.Name;
        }

        if (!string.IsNullOrWhiteSpace(parsed.Brand))
        {
            model.Brand = parsed.Brand;
        }

        if (parsed.QuantityValue is > 0m)
        {
            model.QuantityValue = parsed.QuantityValue;
        }

        if (parsed.QuantityUnit is not null)
        {
            model.QuantityUnit = parsed.QuantityUnit;
        }
    }
}
