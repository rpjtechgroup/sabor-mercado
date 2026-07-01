using SaborMercado.Web.Domain.Shopping;

namespace SaborMercado.Web.Tests;

public sealed class ComparatorColumnOrderTests
{
    [Fact]
    public void TryParse_Invalid_ReturnsFalseAndDefault()
    {
        var ok = ComparatorColumnOrder.TryParse("Product,NotAColumn", out var order);
        Assert.False(ok);
        Assert.Equal(ComparatorColumnOrder.DefaultOrder, order);
    }

    [Fact]
    public void TryParse_Empty_ReturnsFalse()
    {
        var ok = ComparatorColumnOrder.TryParse(string.Empty, out var order);
        Assert.False(ok);
        Assert.Equal(ComparatorColumnOrder.DefaultOrder, order);
    }

    [Fact]
    public void Normalize_AddsMissingColumns()
    {
        var saved = new[] { ComparatorColumnId.When, ComparatorColumnId.Product };
        var normalized = ComparatorColumnOrder.Normalize(saved);
        Assert.Equal(
            [
                ComparatorColumnId.When,
                ComparatorColumnId.Product,
                ComparatorColumnId.Store,
                ComparatorColumnId.BestPrice,
                ComparatorColumnId.WorstPrice
            ],
            normalized);
    }

    [Fact]
    public void Storage_RoundTrip_PreservesOrder()
    {
        var order = new[]
        {
            ComparatorColumnId.WorstPrice,
            ComparatorColumnId.Product,
            ComparatorColumnId.When,
            ComparatorColumnId.BestPrice,
            ComparatorColumnId.Store
        };

        var raw = ComparatorColumnOrder.ToStorageString(order);
        Assert.True(ComparatorColumnOrder.TryParse(raw, out var parsed));
        Assert.Equal(order, parsed);
    }
}
