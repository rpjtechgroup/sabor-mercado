namespace SaborMercado.Web.Domain.Catalog;

public static class QuantityUnitExtensions
{
    
    public static string Label(this QuantityUnit unit) => unit switch
    {
        QuantityUnit.G => "g",
        QuantityUnit.Kg => "kg",
        QuantityUnit.Ml => "ml",
        QuantityUnit.L => "l",
        _ => "un",
    };
}
