using SaborMercado.Web.Domain.Catalog;
using SaborMercado.Web.Domain.Shopping;
using SaborMercado.Web.Features.Shopping;

namespace SaborMercado.Web.Tests.Features;

public class CartCsvExporterTests
{
    [Fact]
    public void Build_IncludesItemsAndTotal()
    {
        var items = new[]
        {
            new CartItem
            {
                ProductSnapshot = new ProductSnapshot("Arroz", "Tio João", 1m, QuantityUnit.Kg),
                UnitPrice = 5.49m,
                Quantity = 2,
                Source = CartItemSource.Manual,
            },
        };

        var csv = CartCsvExporter.Build(items, "Mercado Central", 10.98m);

        Assert.Contains("produto;marca;quantidade;preco_unitario;subtotal;origem", csv);
        Assert.Contains("Arroz", csv);
        Assert.Contains("total;10.98", csv);
        Assert.Contains("mercado;Mercado Central", csv);
    }
}
