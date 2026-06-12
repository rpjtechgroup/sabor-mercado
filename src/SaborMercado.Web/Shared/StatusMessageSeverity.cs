using SaborMercado.Web.Domain.Status;

namespace SaborMercado.Web.Shared;

public static class StatusMessageSeverity
{
    public static ToastSeverity FromCode(string code) => code switch
    {
        StatusCodes.BudgetExceeded or StatusCodes.BudgetReached => ToastSeverity.Danger,
        StatusCodes.BudgetHigh90 => ToastSeverity.Warning,
        StatusCodes.BudgetWarn75 or StatusCodes.PaceProjectionOver or StatusCodes.OcrUnavailable => ToastSeverity.Warning,
        StatusCodes.SessionFinished or StatusCodes.PaceProjectionOk => ToastSeverity.Success,
        _ => ToastSeverity.Info,
    };

    public static string CssClass(ToastSeverity severity) => severity switch
    {
        ToastSeverity.Success => "toast-success",
        ToastSeverity.Warning => "toast-warning",
        ToastSeverity.Danger => "toast-danger",
        _ => "toast-info",
    };
}
