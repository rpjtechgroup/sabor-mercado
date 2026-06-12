namespace SaborMercado.Web.Contracts.SharedCatalog;

public sealed record SubmitPriceObservationRequest(
    string ProductName,
    string? Brand,
    decimal? QuantityValue,
    string? QuantityUnit,
    string? Ean,
    decimal Price,
    string MarketName,
    string? MarketCity,
    string? MarketState,
    DateOnly ObservedOn);

public sealed record PriceObservationResponse(
    Guid ObservationId,
    string Status,
    int CreditsGranted,
    bool IsNewProduct);
