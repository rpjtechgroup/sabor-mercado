namespace SaborMercado.Web.Contracts.Community;

public sealed record VoteObservationRequest(int Value);

public sealed record VoteObservationResponse(
    Guid ObservationId,
    int NetScore,
    int UpvoteCount,
    int DownvoteCount,
    int? CurrentUserVote);

public sealed record SubmitContributorReportRequest(
    Guid TargetPseudonymId,
    Guid? ObservationId,
    string Reason,
    string? Details);

public sealed record ContributorTrustDto(
    Guid PseudonymId,
    int TrustScore,
    string TrustLabel,
    int AcceptedContributions,
    bool IsRestricted);

public sealed record SharedObservationDto(
    Guid ObservationId,
    decimal Price,
    DateOnly ObservedOn,
    string MarketName,
    string? MarketCity,
    string? MarketState,
    int NetScore,
    int UpvoteCount,
    int DownvoteCount,
    int? CurrentUserVote,
    ContributorTrustDto Contributor,
    bool CanVote,
    bool CanReport);

public sealed record SharedObservationListResponse(
    Guid ProductId,
    string ProductName,
    IReadOnlyList<SharedObservationDto> Observations);

public sealed record AchievementDto(
    string Code,
    string Title,
    string Description,
    DateTimeOffset UnlockedAt);

public sealed record AchievementListResponse(IReadOnlyList<AchievementDto> Items);

public static class ReportReasons
{
    public const string MisleadingPrice = "misleading-price";
    public const string Spam = "spam";
    public const string SuspectedBot = "suspected-bot";
    public const string DuplicateAbuse = "duplicate-abuse";
    public const string OffTopic = "off-topic";
    public const string Other = "other";

    public static readonly IReadOnlyList<(string Code, string Label)> Options =
    [
        (MisleadingPrice, "Preço enganoso"),
        (Spam, "Spam"),
        (SuspectedBot, "Suspeita de bot"),
        (DuplicateAbuse, "Abuso de duplicatas"),
        (OffTopic, "Fora do escopo"),
        (Other, "Outro"),
    ];
}
