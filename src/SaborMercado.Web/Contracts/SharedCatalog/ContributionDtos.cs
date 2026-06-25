namespace SaborMercado.Web.Contracts.SharedCatalog;

public sealed record PriceObservationResponse(
    Guid ObservationId,
    string Status,
    bool IsNewProduct);
