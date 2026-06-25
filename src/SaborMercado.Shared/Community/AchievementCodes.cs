namespace SaborMercado.Shared.Community;

public static class AchievementCodes
{
    public const string FirstContribution = "first-contribution";
    public const string Contributor10 = "contributor-10";
    public const string Contributor50 = "contributor-50";
    public const string TrustedVoice = "trusted-voice";
    public const string CommunityHelper = "community-helper";
    public const string QualityObservation = "quality-observation";

    public const string FirstProduct = "first-product";
    public const string ProductCollector10 = "product-collector-10";
    public const string ProductCollector50 = "product-collector-50";
    public const string ProductCollector100 = "product-collector-100";
    public const string FirstStore = "first-store";
    public const string StoreExplorer5 = "store-explorer-5";
    public const string StoreExplorer10 = "store-explorer-10";
    public const string FirstPurchase = "first-purchase";
    public const string Shopper10 = "shopper-10";
    public const string Shopper50 = "shopper-50";
    public const string Shopper100 = "shopper-100";
    public const string BudgetMaster = "budget-master";
    public const string LoginStreak3 = "login-streak-3";
    public const string LoginStreak7 = "login-streak-7";
    public const string LoginStreak30 = "login-streak-30";
    public const string OcrMaster = "ocr-master";
    public const string PriceTracker = "price-tracker";

    public static readonly IReadOnlyDictionary<string, (string Title, string Description)> Catalog =
        new Dictionary<string, (string, string)>(StringComparer.Ordinal)
        {
            [FirstContribution] = ("Primeira contribuição", "Compartilhou o primeiro preço aceito."),
            [Contributor10] = ("Colaborador ativo", "10 observações aceitas no catálogo colaborativo."),
            [Contributor50] = ("Pilar da comunidade", "50 observações aceitas no catálogo colaborativo."),
            [TrustedVoice] = ("Voz confiável", "Reputação de contribuidor na faixa confiável."),
            [CommunityHelper] = ("Ajudante da comunidade", "Recebeu 25 upvotes nas suas observações."),
            [QualityObservation] = ("Preço bem avaliado", "Uma observação atingiu avaliação muito positiva."),
            [FirstProduct] = ("Primeiro produto", "Cadastrou seu primeiro produto no catálogo."),
            [ProductCollector10] = ("Colecionador", "Cadastrou 10 produtos no catálogo."),
            [ProductCollector50] = ("Catalogador master", "Cadastrou 50 produtos no catálogo."),
            [ProductCollector100] = ("Biblioteca de preços", "Cadastrou 100 produtos no catálogo."),
            [FirstStore] = ("Novo comércio", "Cadastrou seu primeiro comércio."),
            [StoreExplorer5] = ("Explorador de mercados", "Cadastrou 5 comércios diferentes."),
            [StoreExplorer10] = ("Conhecedor de lojas", "Cadastrou 10 comércios diferentes."),
            [FirstPurchase] = ("Primeira compra", "Registrou sua primeira compra."),
            [Shopper10] = ("Comprador frequente", "Registrou 10 compras."),
            [Shopper50] = ("Cliente VIP", "Registrou 50 compras."),
            [Shopper100] = ("Super comprador", "Registrou 100 compras."),
            [BudgetMaster] = ("Mestre do orçamento", "Finalizou 10 compras respeitando o orçamento."),
            [LoginStreak3] = ("Consistência", "Realizou login 3 dias consecutivos."),
            [LoginStreak7] = ("Dedicado", "Realizou login 7 dias consecutivos."),
            [LoginStreak30] = ("Usuário fiel", "Realizou login 30 dias consecutivos."),
            [OcrMaster] = ("Digitalizador", "Adicionou 10 itens via OCR."),
            [PriceTracker] = ("Rastreador de preços", "Possui histórico de preços para 10 produtos."),
        };

    public static readonly IReadOnlyDictionary<string, (string MetricKey, int Threshold)> MetricThresholds =
        new Dictionary<string, (string, int)>(StringComparer.Ordinal)
        {
            [FirstProduct] = ("products", 1),
            [ProductCollector10] = ("products", 10),
            [ProductCollector50] = ("products", 50),
            [ProductCollector100] = ("products", 100),
            [FirstStore] = ("stores", 1),
            [StoreExplorer5] = ("stores", 5),
            [StoreExplorer10] = ("stores", 10),
            [FirstPurchase] = ("purchases", 1),
            [Shopper10] = ("purchases", 10),
            [Shopper50] = ("purchases", 50),
            [Shopper100] = ("purchases", 100),
            [BudgetMaster] = ("budgetOk", 10),
            [LoginStreak3] = ("loginStreak", 3),
            [LoginStreak7] = ("loginStreak", 7),
            [LoginStreak30] = ("loginStreak", 30),
            [OcrMaster] = ("ocr", 10),
            [PriceTracker] = ("priceHistory", 10),
        };

    public static int GetMetricValue(string metricKey, UserMetricsValues metrics) =>
        metricKey switch
        {
            "products" => metrics.TotalProductsRegistered,
            "stores" => metrics.TotalStoresRegistered,
            "purchases" => metrics.TotalPurchasesCompleted,
            "budgetOk" => metrics.TotalPurchasesWithBudgetOk,
            "loginStreak" => metrics.CurrentLoginStreakDays,
            "ocr" => metrics.TotalOcrItemsAdded,
            "priceHistory" => metrics.TotalProductsWithPriceHistory,
            _ => 0,
        };
}

public sealed record UserMetricsValues(
    int TotalProductsRegistered,
    int TotalStoresRegistered,
    int TotalPurchasesCompleted,
    int TotalPurchasesWithBudgetOk,
    int TotalOcrItemsAdded,
    int TotalProductsWithPriceHistory,
    int CurrentLoginStreakDays);
