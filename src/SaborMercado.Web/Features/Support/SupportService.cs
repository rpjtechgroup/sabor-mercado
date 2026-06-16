using System.Net;
using System.Text.Json;
using SaborMercado.Web.Infrastructure;

namespace SaborMercado.Web.Features.Support;

public sealed class SupportService(SaborMercadoApiClient api, SupportDiagnosticsCollector diagnostics)
{
    public async Task SendAsync(
        string category,
        string subject,
        string message,
        string? contactName,
        string? contactEmail,
        CancellationToken cancellationToken = default)
    {
        if (!api.IsConfigured)
        {
            throw new SupportException("Serviço indisponível. Verifique a conexão com a internet.");
        }

        var payload = new FeedbackSubmitRequest(
            category,
            subject.Trim(),
            message.Trim(),
            string.IsNullOrWhiteSpace(contactName) ? null : contactName.Trim(),
            string.IsNullOrWhiteSpace(contactEmail) ? null : contactEmail.Trim(),
            await diagnostics.CollectAsync());

        var response = await api.SendAsync(HttpMethod.Post, "/api/v1/feedback", payload, cancellationToken: cancellationToken);
        if (response.StatusCode == HttpStatusCode.Accepted)
        {
            return;
        }

        var detail = await api.ReadErrorDetailAsync(response, cancellationToken);
        throw new SupportException(detail ?? "Não foi possível enviar sua mensagem.");
    }
}

public sealed record FeedbackSubmitRequest(
    string Category,
    string Subject,
    string Message,
    string? ContactName,
    string? ContactEmail,
    JsonElement Diagnostics);

public sealed class SupportException(string message) : Exception(message);
