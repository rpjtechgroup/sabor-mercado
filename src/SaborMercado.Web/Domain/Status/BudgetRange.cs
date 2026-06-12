namespace SaborMercado.Web.Domain.Status;

public enum BudgetRange
{
    
    Ok,

    
    Warn,

    
    High,

    
    Over,
}

public static class BudgetRanges
{
    
    public static BudgetRange FromPercent(decimal percent) => percent switch
    {
        < 60m => BudgetRange.Ok,
        < 85m => BudgetRange.Warn,
        < 100m => BudgetRange.High,
        _ => BudgetRange.Over,
    };

    
    public static string ToCssCode(this BudgetRange range) => range switch
    {
        BudgetRange.Ok => "budget-ok",
        BudgetRange.Warn => "budget-warn",
        BudgetRange.High => "budget-high",
        _ => "budget-over",
    };
}
