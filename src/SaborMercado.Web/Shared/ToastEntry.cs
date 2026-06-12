namespace SaborMercado.Web.Shared;

public sealed record ToastEntry(
    Guid Id,
    string Text,
    ToastSeverity Severity,
    Action? OnDismiss);
