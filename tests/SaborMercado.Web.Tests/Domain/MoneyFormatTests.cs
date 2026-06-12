using SaborMercado.Web.Shared;

namespace SaborMercado.Web.Tests.Domain;

public class MoneyFormatTests
{
    [Fact]
    public void Format_UsesPtBrCurrencyWithTwoDecimals()
    {
        var formatted = MoneyFormat.Format(8.99m);

        Assert.Contains("R$", formatted);
        Assert.Contains("8,99", formatted);
    }

    [Fact]
    public void Format_ThousandsSeparatorIsDot()
    {
        var formatted = MoneyFormat.Format(1234.50m);

        Assert.Contains("1.234,50", formatted);
    }

    [Theory]
    [InlineData("8,99", "8.99")]
    [InlineData("0,50", "0.50")]
    [InlineData("1.234,56", "1234.56")]
    [InlineData("R$ 8,99", "8.99")]
    [InlineData(" 300 ", "300")]
    public void TryParse_AcceptsPtBrInput(string input, string expectedText)
    {
        var expected = decimal.Parse(expectedText, System.Globalization.CultureInfo.InvariantCulture);

        Assert.True(MoneyFormat.TryParse(input, out var value));
        Assert.Equal(expected, value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    public void TryParse_RejectsInvalidInput(string? input)
    {
        Assert.False(MoneyFormat.TryParse(input, out _));
    }
}
