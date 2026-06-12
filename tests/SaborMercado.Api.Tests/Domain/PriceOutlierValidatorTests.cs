using SaborMercado.Modules.SharedCatalog.Domain;

namespace SaborMercado.Api.Tests.Domain;

public class PriceOutlierValidatorTests
{
    [Fact]
    public void IsPlausible_WithFewSamples_AlwaysTrue()
    {
        var history = new[] { 4.99m, 5.10m, 5.00m, 4.95m };
        Assert.True(PriceOutlierValidator.IsPlausible(99m, history));
    }

    [Fact]
    public void IsPlausible_RejectsExtremeOutlier()
    {
        var history = new[] { 5.00m, 5.10m, 4.95m, 5.05m, 4.99m, 5.02m };
        Assert.False(PriceOutlierValidator.IsPlausible(50m, history));
        Assert.True(PriceOutlierValidator.IsPlausible(5.15m, history));
    }
}
