namespace SaborMercado.Web.Domain.Status;

public static class StatusCodes
{
    public const string BudgetSet = "BUDGET_SET";
    public const string BudgetHalf = "BUDGET_HALF";
    public const string BudgetWarn75 = "BUDGET_WARN_75";
    public const string BudgetHigh90 = "BUDGET_HIGH_90";
    public const string BudgetReached = "BUDGET_REACHED";
    public const string BudgetExceeded = "BUDGET_EXCEEDED";
    public const string PaceProjectionOver = "PACE_PROJECTION_OVER";
    public const string PaceProjectionOk = "PACE_PROJECTION_OK";
    public const string SessionFinished = "SESSION_FINISHED";

    
    public const string ItemAddedOcr = "ITEM_ADDED_OCR";
    public const string ItemAddedOcrReview = "ITEM_ADDED_OCR_REVIEW";
    public const string OcrUnavailable = "OCR_UNAVAILABLE";
}
