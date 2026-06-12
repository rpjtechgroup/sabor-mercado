using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Domain.Shopping;

public sealed class ShoppingPattern
{
    public Guid Id { get; set; } = Ids.NewId();

    public string Name { get; set; } = "Compra mensal";

    public List<PatternItem> Items { get; set; } = [];

    public DateTimeOffset UpdatedAt { get; set; }

    public int SchemaVersion { get; set; } = StorageSchema.CurrentSchemaVersion;
}

public sealed class PatternItem
{
    public Guid ProductId { get; set; }

    public int DefaultQuantity { get; set; } = 1;
}
