using Bunit;
using SaborMercado.Web.Shared;

namespace SaborMercado.Web.Tests.Components;

public class BudgetBarTests : BunitContext
{
    private IRenderedComponent<BudgetBar> RenderBar(decimal total, decimal? budget) =>
        Render<BudgetBar>(parameters => parameters
            .Add(p => p.Total, total)
            .Add(p => p.Budget, budget));

    [Theory]
    [InlineData("30", "budget-ok")]
    [InlineData("59.99", "budget-ok")]
    [InlineData("60", "budget-warn")]
    [InlineData("84.99", "budget-warn")]
    [InlineData("85", "budget-high")]
    [InlineData("99.99", "budget-high")]
    [InlineData("100", "budget-over")]
    [InlineData("130", "budget-over")]
    public void Fill_UsesCatalogRangeCssCode(string totalText, string expectedCss)
    {
        var total = decimal.Parse(totalText, System.Globalization.CultureInfo.InvariantCulture);

        var cut = RenderBar(total, 100m);

        var fill = cut.Find(".budget-fill");
        Assert.Contains(expectedCss, fill.ClassList);
    }

    [Fact]
    public void WithoutBudget_RendersNothing()
    {
        var cut = RenderBar(50m, null);

        Assert.Empty(cut.Markup.Trim());
    }

    [Fact]
    public void Legend_ShowsSpentAndRemainingInPtBr()
    {
        var cut = RenderBar(62m, 100m);

        Assert.Contains("62,00", cut.Markup);
        Assert.Contains("38,00", cut.Markup);
        Assert.Contains("62% da meta", cut.Markup);
    }

    [Fact]
    public void OverBudget_ShowsNegativeRemaining()
    {
        var cut = RenderBar(127m, 100m);

        Assert.Contains(MoneyFormat.Format(-27m), cut.Markup);
    }
}
