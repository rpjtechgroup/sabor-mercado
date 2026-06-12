namespace SaborMercado.Web.Domain.Status;

/// <summary>Faixas da barra de orçamento (docs/domain/status-messages.md).</summary>
public enum BudgetRange
{
    /// <summary>0% – 59,99% (verde, `budget-ok`).</summary>
    Ok,

    /// <summary>60% – 84,99% (amarelo, `budget-warn`).</summary>
    Warn,

    /// <summary>85% – 99,99% (laranja, `budget-high`).</summary>
    High,

    /// <summary>≥ 100% (vermelho, `budget-over`).</summary>
    Over,
}

public static class BudgetRanges
{
    /// <param name="percent">Percentual utilizado `P` em escala 0–100+.</param>
    public static BudgetRange FromPercent(decimal percent) => percent switch
    {
        < 60m => BudgetRange.Ok,
        < 85m => BudgetRange.Warn,
        < 100m => BudgetRange.High,
        _ => BudgetRange.Over,
    };

    /// <summary>Código CSS estável da faixa, conforme o catálogo.</summary>
    public static string ToCssCode(this BudgetRange range) => range switch
    {
        BudgetRange.Ok => "budget-ok",
        BudgetRange.Warn => "budget-warn",
        BudgetRange.High => "budget-high",
        _ => "budget-over",
    };
}
