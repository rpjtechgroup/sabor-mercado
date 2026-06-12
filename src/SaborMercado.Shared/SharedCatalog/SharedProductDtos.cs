namespace SaborMercado.Shared.SharedCatalog;

public sealed record SharedProductSummaryDto(
    Guid ProductId,
    string NormalizedName,
    string? Brand,
    string? Ean,
    decimal? LastPrice,
    string? LastMarketName,
    DateOnly? LastObservedOn);

public sealed record SharedProductSearchResponse(IReadOnlyList<SharedProductSummaryDto> Items);

public sealed record MarketPriceDto(
    string MarketName,
    string? City,
    decimal Price,
    DateOnly ObservedOn);

public sealed record MarketPriceComparisonResponse(
    Guid ProductId,
    string ProductName,
    IReadOnlyList<MarketPriceDto> Markets);
