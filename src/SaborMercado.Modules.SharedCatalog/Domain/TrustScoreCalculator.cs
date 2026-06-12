namespace SaborMercado.Modules.SharedCatalog.Domain;

public static class TrustScoreCalculator
{
    public const int HideObservationNetScoreThreshold = -3;

    public const int RestrictionReportThreshold = 3;

    public static int Calculate(
        int totalUpvotesReceived,
        int totalDownvotesReceived,
        int acceptedContributions,
        int reportCount,
        bool isRestricted) =>
        Math.Clamp(
            50
            + (totalUpvotesReceived - totalDownvotesReceived) * 2
            + Math.Min(acceptedContributions, 20)
            - reportCount * 5
            - (isRestricted ? 15 : 0),
            0,
            100);

    public static string Label(int trustScore) => trustScore switch
    {
        >= 70 => "Confiável",
        >= 40 => "Neutro",
        _ => "Cautela",
    };

    public static bool ShouldHideObservation(int netScore) =>
        netScore <= HideObservationNetScoreThreshold;
}
