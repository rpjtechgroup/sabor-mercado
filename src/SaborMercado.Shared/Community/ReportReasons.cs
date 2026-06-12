namespace SaborMercado.Shared.Community;

public static class ReportReasons
{
    public const string MisleadingPrice = "misleading-price";
    public const string Spam = "spam";
    public const string SuspectedBot = "suspected-bot";
    public const string DuplicateAbuse = "duplicate-abuse";
    public const string OffTopic = "off-topic";
    public const string Other = "other";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        MisleadingPrice,
        Spam,
        SuspectedBot,
        DuplicateAbuse,
        OffTopic,
        Other,
    };

    public static string Label(string reason) => reason switch
    {
        MisleadingPrice => "Preço enganoso",
        Spam => "Spam",
        SuspectedBot => "Suspeita de bot",
        DuplicateAbuse => "Abuso de duplicatas",
        OffTopic => "Fora do escopo",
        Other => "Outro",
        _ => reason,
    };
}
