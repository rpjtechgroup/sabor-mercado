using SaborMercado.Web.Domain.Status;

namespace SaborMercado.Web.Tests.Domain;

public class BudgetRangesTests
{
    [Theory]
    [InlineData("0", BudgetRange.Ok)]
    [InlineData("30", BudgetRange.Ok)]
    [InlineData("59.99", BudgetRange.Ok)]
    [InlineData("60", BudgetRange.Warn)]
    [InlineData("75", BudgetRange.Warn)]
    [InlineData("84.99", BudgetRange.Warn)]
    [InlineData("85", BudgetRange.High)]
    [InlineData("99.99", BudgetRange.High)]
    [InlineData("100", BudgetRange.Over)]
    [InlineData("150", BudgetRange.Over)]
    public void FromPercent_ReturnsCatalogRange(string percentText, BudgetRange expected)
    {
        var percent = decimal.Parse(percentText, System.Globalization.CultureInfo.InvariantCulture);

        Assert.Equal(expected, BudgetRanges.FromPercent(percent));
    }

    [Theory]
    [InlineData(BudgetRange.Ok, "budget-ok")]
    [InlineData(BudgetRange.Warn, "budget-warn")]
    [InlineData(BudgetRange.High, "budget-high")]
    [InlineData(BudgetRange.Over, "budget-over")]
    public void ToCssCode_MatchesCatalogCodes(BudgetRange range, string expected)
    {
        Assert.Equal(expected, range.ToCssCode());
    }
}
