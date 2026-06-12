using SaborMercado.Web.Contracts.Recognition;
using SaborMercado.Web.Domain.Catalog;
using SaborMercado.Web.Features.Shopping;

namespace SaborMercado.Web.Features.Recognition;

public static class RecognitionResultMapper
{
    public static CartItemFormModel ToFormModel(RecognitionResultDto result)
    {
        QuantityUnit? unit = result.QuantityUnit switch
        {
            "g" => QuantityUnit.G,
            "kg" => QuantityUnit.Kg,
            "ml" => QuantityUnit.Ml,
            "l" => QuantityUnit.L,
            "un" => QuantityUnit.Un,
            _ => null,
        };

        return new CartItemFormModel
        {
            Name = result.ProductName ?? string.Empty,
            Brand = result.Brand,
            QuantityValue = result.QuantityValue,
            QuantityUnit = unit,
            UnitPrice = result.Price,
            Quantity = 1,
        };
    }
}
