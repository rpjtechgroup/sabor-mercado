namespace SaborMercado.Modules.SharedCatalog.Data;

public sealed class ContributionIdempotency
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }

    public string IdempotencyKey { get; set; } = string.Empty;

    public string RequestHash { get; set; } = string.Empty;

    public string ResponseJson { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}
