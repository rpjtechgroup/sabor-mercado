namespace SaborMercado.Web.Domain.Status;

/// <summary>
/// Foto imutável do carrinho usada pelo avaliador (variáveis de cálculo do
/// catálogo: T, n, B, N e dados para a projeção E).
/// </summary>
/// <param name="Total">Total atual `T` = Σ(preço unitário × quantidade).</param>
/// <param name="DistinctItemCount">Quantidade `n` de itens distintos.</param>
/// <param name="BudgetAmount">Meta `B`; null quando não definida.</param>
/// <param name="SessionStartedAt">Início da sessão (projeção por ritmo).</param>
/// <param name="PlannedListSize">Tamanho esperado `N` da lista (não usado nesta feature; sempre null).</param>
/// <param name="AverageSessionDuration">Duração média de sessão do histórico do usuário (default 40 min).</param>
public sealed record CartSnapshot(
    decimal Total,
    int DistinctItemCount,
    decimal? BudgetAmount,
    DateTimeOffset SessionStartedAt,
    int? PlannedListSize = null,
    TimeSpan? AverageSessionDuration = null)
{
    public static readonly TimeSpan DefaultAverageSessionDuration = TimeSpan.FromMinutes(40);

    public TimeSpan EffectiveAverageSessionDuration =>
        AverageSessionDuration is { } avg && avg > TimeSpan.Zero
            ? avg
            : DefaultAverageSessionDuration;

    /// <summary>Percentual utilizado `P` em escala 0–100+ (0 sem meta).</summary>
    public decimal PercentUsed =>
        BudgetAmount is { } budget && budget > 0m ? Total / budget * 100m : 0m;
}
