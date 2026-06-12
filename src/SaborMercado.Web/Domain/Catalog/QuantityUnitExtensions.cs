namespace SaborMercado.Web.Domain.Catalog;

public static class QuantityUnitExtensions
{
    /// <summary>Rótulo curto exibido na UI (g | kg | ml | l | un).</summary>
    public static string Label(this QuantityUnit unit) => unit switch
    {
        QuantityUnit.G => "g",
        QuantityUnit.Kg => "kg",
        QuantityUnit.Ml => "ml",
        QuantityUnit.L => "l",
        _ => "un",
    };
}
