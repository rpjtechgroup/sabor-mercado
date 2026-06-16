using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Domain.Catalog;

public sealed class Product
{
    public Guid Id { get; set; } = Ids.NewId();

    public required string Name { get; set; }

    public string? StarterKey { get; set; }

    public string? Brand { get; set; }

    public decimal? QuantityValue { get; set; }

    public QuantityUnit? QuantityUnit { get; set; }

    public string? Ean { get; set; }

    public string? Category { get; set; }

    public string? Notes { get; set; }

    
    public Guid StoreId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public int SchemaVersion { get; set; } = StorageSchema.CurrentSchemaVersion;
}
