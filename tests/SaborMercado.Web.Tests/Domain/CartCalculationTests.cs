using SaborMercado.Web.Domain.Shopping;
using SaborMercado.Web.Domain.Status;

namespace SaborMercado.Web.Tests.Domain;

/// <summary>Cálculos do carrinho (F2): subtotal, total e percentual.</summary>
public class CartCalculationTests
{
    private static CartItem Item(decimal unitPrice, int quantity) => new()
    {
        ProductSnapshot = new ProductSnapshot("Óleo de Soja Liza", null, 900m, SaborMercado.Web.Domain.Catalog.QuantityUnit.Ml),
        UnitPrice = unitPrice,
        Quantity = quantity,
    };

    [Fact]
    public void Subtotal_IsQuantityTimesUnitPrice()
    {
        // Exemplo canônico da visão: 3 × R$ 8,99 = R$ 26,97.
        Assert.Equal(26.97m, Item(8.99m, 3).Subtotal);
    }

    [Fact]
    public void Subtotal_WithQuantityOne_EqualsUnitPrice()
    {
        Assert.Equal(8.99m, Item(8.99m, 1).Subtotal);
    }

    [Fact]
    public void Total_IsSumOfSubtotals()
    {
        var items = new[] { Item(8.99m, 3), Item(12.50m, 2), Item(0m, 5) };

        Assert.Equal(51.97m, items.Sum(i => i.Subtotal));
    }

    [Fact]
    public void PercentUsed_ComputedFromBudget()
    {
        var snapshot = new CartSnapshot(62m, 4, 100m, DateTimeOffset.UtcNow);

        Assert.Equal(62m, snapshot.PercentUsed);
    }

    [Fact]
    public void PercentUsed_WithoutBudget_IsZero()
    {
        var snapshot = new CartSnapshot(62m, 4, null, DateTimeOffset.UtcNow);

        Assert.Equal(0m, snapshot.PercentUsed);
    }
}
