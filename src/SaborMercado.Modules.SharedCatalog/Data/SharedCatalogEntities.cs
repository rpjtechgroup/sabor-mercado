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

    public int UpvoteCount { get; set; }

    public int DownvoteCount { get; set; }

    public bool IsHidden { get; set; }
}

public sealed class ObservationVote
{
    public Guid Id { get; set; }

    public Guid ObservationId { get; set; }

    public PriceObservation Observation { get; set; } = null!;

    public Guid VoterUserId { get; set; }

    public int Value { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class ContributorTrust
{
    public Guid PseudonymId { get; set; }

    
    public Guid? ContributorUserId { get; set; }

    public int TrustScore { get; set; } = 50;

    public int TotalUpvotesReceived { get; set; }

    public int TotalDownvotesReceived { get; set; }

    public int AcceptedContributions { get; set; }

    public int ReportCount { get; set; }

    public bool IsRestricted { get; set; }

    public DateTimeOffset? RestrictedUntil { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class ContributorReport
{
    public Guid Id { get; set; }

    public Guid ReporterUserId { get; set; }

    public Guid TargetPseudonymId { get; set; }

    public Guid? ObservationId { get; set; }

    public string Reason { get; set; } = string.Empty;

    public string? Details { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
