using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SaborMercado.Infrastructure.Email;
using SaborMercado.Modules.Support.Contracts;

namespace SaborMercado.Modules.Support.Services;

public sealed class FeedbackEmailService(
    IEmailSender emailSender,
    IOptions<EmailOptions> emailOptions,
    ILogger<FeedbackEmailService> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public async Task SendAsync(FeedbackRequest request, CancellationToken cancellationToken)
    {
        var settings = emailOptions.Value;
        if (!settings.IsConfigured)
        {
            throw new FeedbackException(SupportErrorCodes.FeedbackUnavailable, "Serviço de feedback indisponível.");
        }

        var categoryLabel = CategoryLabel(request.Category);
        var subject = $"[Sabor Mercado][{categoryLabel}] {request.Subject.Trim()}";
        var textBody = BuildTextBody(request, categoryLabel);
        var htmlBody = BuildHtmlBody(request, categoryLabel);

        try
        {
            await emailSender.SendAsync(new EmailMessage
            {
                ToAddress = settings.SupportToAddress,
                ReplyToAddress = string.IsNullOrWhiteSpace(request.ContactEmail) ? null : request.ContactEmail.Trim(),
                ReplyToName = string.IsNullOrWhiteSpace(request.ContactName) ? null : request.ContactName.Trim(),
                Subject = subject,
                TextBody = textBody,
                HtmlBody = htmlBody,
            }, cancellationToken);
        }
        catch (EmailSendException ex)
        {
            logger.LogWarning(ex, "Feedback email failed");
            throw new FeedbackException(SupportErrorCodes.FeedbackUnavailable, "Não foi possível enviar o feedback.");
        }
    }

    private static string CategoryLabel(string category) => category switch
    {
        FeedbackCategories.Bug => "Bug",
        FeedbackCategories.Suggestion => "Sugestão",
        FeedbackCategories.Criticism => "Crítica",
        FeedbackCategories.Support => "Suporte",
        _ => "Outro",
    };

    private static string BuildTextBody(FeedbackRequest request, string categoryLabel)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Categoria: {categoryLabel}");
        if (!string.IsNullOrWhiteSpace(request.ContactName))
        {
            builder.AppendLine($"Nome: {request.ContactName.Trim()}");
        }

        if (!string.IsNullOrWhiteSpace(request.ContactEmail))
        {
            builder.AppendLine($"E-mail: {request.ContactEmail.Trim()}");
        }

        builder.AppendLine();
        builder.AppendLine("Mensagem:");
        builder.AppendLine(request.Message.Trim());
        builder.AppendLine();
        builder.AppendLine("--- Diagnóstico ---");
        builder.AppendLine(FormatDiagnostics(request.Diagnostics));
        return builder.ToString();
    }

    private static string BuildHtmlBody(FeedbackRequest request, string categoryLabel)
    {
        var diagnostics = System.Net.WebUtility.HtmlEncode(FormatDiagnostics(request.Diagnostics));
        var message = System.Net.WebUtility.HtmlEncode(request.Message.Trim()).Replace("\n", "<br/>", StringComparison.Ordinal);
        return $"""
            <p><strong>Categoria:</strong> {System.Net.WebUtility.HtmlEncode(categoryLabel)}</p>
            {(string.IsNullOrWhiteSpace(request.ContactName) ? "" : $"<p><strong>Nome:</strong> {System.Net.WebUtility.HtmlEncode(request.ContactName.Trim())}</p>")}
            {(string.IsNullOrWhiteSpace(request.ContactEmail) ? "" : $"<p><strong>E-mail:</strong> {System.Net.WebUtility.HtmlEncode(request.ContactEmail.Trim())}</p>")}
            <p><strong>Mensagem:</strong><br/>{message}</p>
            <hr/>
            <pre style="font-size:12px;white-space:pre-wrap;">{diagnostics}</pre>
            """;
    }

    private static string FormatDiagnostics(JsonElement? diagnostics)
    {
        if (diagnostics is null || diagnostics.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return "(sem diagnóstico)";
        }

        return JsonSerializer.Serialize(diagnostics.Value, JsonOptions);
    }
}

public sealed class FeedbackException(string code, string detail) : Exception(detail)
{
    public string Code { get; } = code;
}
