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

    /// <summary>Purchase-time snapshot: store where this line was bought (not catalog domain).</summary>
    public Guid? StoreId { get; set; }

    /// <summary>Denormalized store label at add time for purchase history.</summary>
    public string? StoreName { get; set; }

    public int SchemaVersion { get; set; } = StorageSchema.CurrentSchemaVersion;

    public decimal Subtotal => UnitPrice * Quantity;
}
