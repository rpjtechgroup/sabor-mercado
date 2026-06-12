using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Domain.Shopping;

public sealed class CartItem
{
    public Guid Id { get; set; } = Ids.NewId();

    public Guid SessionId { get; set; }

    public required ProductSnapshot ProductSnapshot { get; set; }

    public Guid? ProductId { get; set; }

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; } = 1;

    public CartItemSource Source { get; set; } = CartItemSource.Manual;

    public DateTimeOffset AddedAt { get; set; }

    public int SchemaVersion { get; set; } = StorageSchema.CurrentSchemaVersion;

    public decimal Subtotal => UnitPrice * Quantity;
}
