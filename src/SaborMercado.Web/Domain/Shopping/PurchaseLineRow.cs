namespace SaborMercado.Web.Domain.Shopping;

public sealed record PurchaseLineRow(
    Guid SessionId,
    DateOnly PurchaseDate,
    string? MarketName,
    string ProductName,
    string? VolumeLabel,
    decimal UnitPrice,
    decimal? DeltaVsPrevious,
    PriceTrend Trend,
    string ProductKey);

public sealed record MarketPriceCell(string MarketName, decimal UnitPrice, DateOnly LastSeen);

public sealed record MarketMatrixRow(
    string ProductName,
    string? VolumeLabel,
    string ProductKey,
    IReadOnlyList<MarketPriceCell> Cells,
    string? BestMarketName,
    decimal? BestPrice);

public sealed record FinishedSessionSummary(
    Guid SessionId,
    DateOnly PurchaseDate,
    string? MarketName,
    decimal Total,
    int ItemCount);
