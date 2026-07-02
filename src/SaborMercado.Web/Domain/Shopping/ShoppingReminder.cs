using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Domain.Shopping;

public sealed class ShoppingReminder
{
    public Guid Id { get; set; } = Ids.NewId();

    public Guid? ProductId { get; set; }

    public required string DisplayName { get; set; }

    public int Quantity { get; set; } = 1;

    public DateTimeOffset CreatedAt { get; set; }

    public int SchemaVersion { get; set; } = StorageSchema.CurrentSchemaVersion;
}
