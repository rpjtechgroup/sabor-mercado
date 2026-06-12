using SaborMercado.Web.Storage;

namespace SaborMercado.Web.Domain.Shopping;

public sealed class ShoppingSession
{
    public Guid Id { get; set; } = Ids.NewId();

    public string? MarketName { get; set; }

    public decimal? BudgetAmount { get; set; }

    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset? FinishedAt { get; set; }

    public SessionStatus Status { get; set; } = SessionStatus.Active;

    public SessionKind Kind { get; set; } = SessionKind.Sporadic;

    public Guid? PatternId { get; set; }

    public BudgetAlertState AlertState { get; set; } = BudgetAlertState.Initial;

    public int SchemaVersion { get; set; } = StorageSchema.CurrentSchemaVersion;
}
