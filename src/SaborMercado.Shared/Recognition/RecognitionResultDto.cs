namespace SaborMercado.Shared.Recognition;

public sealed record RecognitionResultDto(
    string? ProductName,
    string? Brand,
    decimal? QuantityValue,
    string? QuantityUnit,
    decimal? Price,
    string? Ean,
    decimal Confidence,
    string? RawText);
