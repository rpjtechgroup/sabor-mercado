namespace SaborMercado.Shared.Community;

public static class AchievementCodes
{
    public const string FirstContribution = "first-contribution";
    public const string Contributor10 = "contributor-10";
    public const string Contributor50 = "contributor-50";
    public const string TrustedVoice = "trusted-voice";
    public const string CommunityHelper = "community-helper";
    public const string QualityObservation = "quality-observation";

    public static readonly IReadOnlyDictionary<string, (string Title, string Description)> Catalog =
        new Dictionary<string, (string, string)>(StringComparer.Ordinal)
        {
            [FirstContribution] = ("Primeira contribuição", "Compartilhou o primeiro preço aceito."),
            [Contributor10] = ("Colaborador ativo", "10 observações aceitas no catálogo colaborativo."),
            [Contributor50] = ("Pilar da comunidade", "50 observações aceitas no catálogo colaborativo."),
            [TrustedVoice] = ("Voz confiável", "Reputação de contribuidor na faixa confiável."),
            [CommunityHelper] = ("Ajudante da comunidade", "Recebeu 25 upvotes nas suas observações."),
            [QualityObservation] = ("Preço bem avaliado", "Uma observação atingiu avaliação muito positiva."),
        };
}
