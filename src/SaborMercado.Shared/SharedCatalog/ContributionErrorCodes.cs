namespace SaborMercado.Shared.SharedCatalog;

public static class ContributionErrorCodes
{
    public const string InvalidDate = "INVALID_DATE";
    public const string InvalidPrice = "INVALID_PRICE";
    public const string DailyLimit = "DAILY_LIMIT";
    public const string Duplicate = "DUPLICATE";
    public const string PriceOutlier = "PRICE_OUTLIER";
    public const string IdempotencyConflict = "IDEMPOTENCY_CONFLICT";
}
