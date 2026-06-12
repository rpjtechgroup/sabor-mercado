namespace SaborMercado.Web.Contracts.Recognition;

/// <summary>
/// Espelho JSON de <c>SaborMercado.Shared.Recognition.RecognitionResultDto</c>.
/// Mantido no Web para evitar carregar um assembly WASM extra (class library).
/// </summary>
public sealed record RecognitionResultDto(
    string? ProductName,
    string? Brand,
    decimal? QuantityValue,
    string? QuantityUnit,
    decimal? Price,
    string? Ean,
    decimal Confidence,
    string? RawText);
