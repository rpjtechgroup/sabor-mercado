namespace SaborMercado.Modules.Rewards.Data;

public enum CreditReason
{
    ContributionAccepted,
    NewProductBonus,
    EanBonus,
    UnlockSpend,
}

public sealed class CreditLedgerEntry
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public int Amount { get; set; }

    public CreditReason Reason { get; set; }

    public Guid? ReferenceId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class FeatureUnlock
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string FeatureCode { get; set; } = string.Empty;

    public DateTimeOffset UnlockedAt { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }
}
