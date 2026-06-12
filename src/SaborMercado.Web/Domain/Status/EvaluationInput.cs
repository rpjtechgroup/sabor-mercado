using SaborMercado.Web.Domain.Shopping;

namespace SaborMercado.Web.Domain.Status;

public sealed record EvaluationInput(
    BudgetAlertState Before,
    CartSnapshot Cart,
    CartMutation Mutation,
    DateTimeOffset Now,
    decimal? OcrConfidence = null,
    string? OcrProductName = null);

public sealed record EvaluationResult(
    StatusMessage? Message,
    BudgetAlertState After);
