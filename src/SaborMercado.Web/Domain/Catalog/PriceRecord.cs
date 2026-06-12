using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Domain.Catalog;

public sealed class PriceRecord
{
    public Guid Id { get; set; } = Ids.NewId();

    public Guid ProductId { get; set; }

    public decimal Price { get; set; }

    public Guid? StoreId { get; set; }

    
    public string? MarketName { get; set; }

    public DateTimeOffset ObservedAt { get; set; }

    public PriceSource Source { get; set; } = PriceSource.Manual;

    public int SchemaVersion { get; set; } = StorageSchema.CurrentSchemaVersion;
}
