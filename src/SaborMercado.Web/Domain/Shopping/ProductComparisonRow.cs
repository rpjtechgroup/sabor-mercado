namespace SaborMercado.Web.Domain.Shopping;

public sealed record ProductComparisonRow(
    string ProductName,
    string BestMarketName,
    decimal BestPrice,
    decimal WorstPrice,
    DateOnly BestPriceDate,
    string ProductKey);
