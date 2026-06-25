namespace SaborMercado.Shared.Rewards;

public static class RankingTypes
{
    public const string Products = "products";
    public const string Stores = "stores";
    public const string Purchases = "purchases";
    public const string LoginStreak = "login-streak";
    public const string Achievements = "achievements";

    public static readonly IReadOnlyDictionary<string, string> Titles =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [Products] = "Mais produtos cadastrados",
            [Stores] = "Mais comércios cadastrados",
            [Purchases] = "Mais compras registradas",
            [LoginStreak] = "Maior sequência de login",
            [Achievements] = "Mais conquistas desbloqueadas",
        };

    public static bool IsValid(string rankingType) => Titles.ContainsKey(rankingType);
}
