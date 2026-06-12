namespace SaborMercado.Shared.Recognition;

/// <summary>Resultado estruturado da leitura de etiqueta (contrato API ↔ PWA).</summary>
public sealed record RecognitionResultDto(
    string? ProductName,
    string? Brand,
    decimal? QuantityValue,
    string? QuantityUnit,
    decimal? Price,
    string? Ean,
    decimal Confidence,
    string? RawText);
