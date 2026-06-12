using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Domain.Sharing;

public sealed class PendingShare
{
    public Guid Id { get; set; } = Ids.NewId();

    public required string ProductName { get; set; }

    public string? Brand { get; set; }

    public decimal? QuantityValue { get; set; }

    public string? QuantityUnit { get; set; }

    public string? Ean { get; set; }

    public decimal Price { get; set; }

    public required string MarketName { get; set; }

    public string? MarketCity { get; set; }

    public string? MarketState { get; set; }

    public DateOnly ObservedOn { get; set; }

    public DateTimeOffset QueuedAt { get; set; }

    public int SchemaVersion { get; set; } = StorageSchema.CurrentSchemaVersion;
}
