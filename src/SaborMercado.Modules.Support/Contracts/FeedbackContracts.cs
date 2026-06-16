using System.Text.Json;

namespace SaborMercado.Modules.Support.Contracts;

public sealed record FeedbackRequest(
    string Category,
    string Subject,
    string Message,
    string? ContactName,
    string? ContactEmail,
    JsonElement? Diagnostics);

public static class FeedbackCategories
{
    public const string Bug = "bug";
    public const string Suggestion = "suggestion";
    public const string Criticism = "criticism";
    public const string Support = "support";

    public static bool IsValid(string? category) =>
        category is Bug or Suggestion or Criticism or Support;
}

public static class SupportErrorCodes
{
    public const string InvalidFeedback = "INVALID_FEEDBACK";
    public const string FeedbackUnavailable = "FEEDBACK_UNAVAILABLE";
}
