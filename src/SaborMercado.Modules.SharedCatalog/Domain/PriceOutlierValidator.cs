namespace SaborMercado.Modules.SharedCatalog.Domain;

public static class PriceOutlierValidator
{
    private const int MinSamples = 5;
    private const decimal ZThreshold = 3m;

    public static bool IsPlausible(decimal price, IReadOnlyList<decimal> historicalPrices)
    {
        if (historicalPrices.Count < MinSamples)
        {
            return true;
        }

        var mean = historicalPrices.Average();
        var variance = historicalPrices
            .Select(p => (p - mean) * (p - mean))
            .Average();

        var stdDev = (decimal)Math.Sqrt((double)variance);
        if (stdDev == 0m)
        {
            return price == mean;
        }

        var zScore = Math.Abs((price - mean) / stdDev);
        return zScore <= ZThreshold;
    }
}
