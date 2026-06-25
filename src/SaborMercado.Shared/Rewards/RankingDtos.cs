namespace SaborMercado.Shared.Rewards;

public sealed record RankingEntryDto(
    int RankPosition,
    string PseudonymDisplay,
    int Score,
    bool IsCurrentUser);

public sealed record RankingListResponse(
    string RankingType,
    string Title,
    IReadOnlyList<RankingEntryDto> Entries,
    int? CurrentUserRank,
    int? CurrentUserScore,
    DateTimeOffset CalculatedAt);
