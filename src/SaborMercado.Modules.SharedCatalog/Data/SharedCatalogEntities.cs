namespace SaborMercado.Modules.SharedCatalog.Data;

public sealed class Market
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? City { get; set; }

    public string? State { get; set; }
}

public sealed class SharedProduct
{
    public Guid Id { get; set; }

    public string NormalizedName { get; set; } = string.Empty;

    public string? Brand { get; set; }

    public string? Ean { get; set; }

    public decimal? QuantityValue { get; set; }

    public string? QuantityUnit { get; set; }

    public string? Category { get; set; }
}

public enum ObservationStatus
{
    Pending,
    Accepted,
    Rejected,
}

public sealed class PriceObservation
{
    public Guid Id { get; set; }

    public Guid SharedProductId { get; set; }

    public SharedProduct SharedProduct { get; set; } = null!;

    public Guid MarketId { get; set; }

    public Market Market { get; set; } = null!;

    public decimal Price { get; set; }

    public DateOnly ObservedOn { get; set; }

    public Guid ContributorPseudonymId { get; set; }

    public ObservationStatus Status { get; set; }

    public string? RejectionReason { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
