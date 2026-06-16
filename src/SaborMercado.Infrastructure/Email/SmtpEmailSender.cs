using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace SaborMercado.Infrastructure.Email;

public sealed class SmtpEmailSender(
    IOptions<EmailOptions> options,
    ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        if (!settings.IsConfigured)
        {
            throw new EmailSendException("EMAIL_NOT_CONFIGURED", "Serviço de e-mail não configurado.");
        }

        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(settings.FromName, settings.FromAddress));
        mime.To.Add(new MailboxAddress(message.ToName ?? message.ToAddress, message.ToAddress));
        mime.Subject = message.Subject;

        if (!string.IsNullOrWhiteSpace(message.ReplyToAddress))
        {
            mime.ReplyTo.Add(new MailboxAddress(message.ReplyToName ?? message.ReplyToAddress, message.ReplyToAddress));
        }

        var builder = new BodyBuilder
        {
            TextBody = message.TextBody,
            HtmlBody = message.HtmlBody,
        };
        mime.Body = builder.ToMessageBody();

        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(settings.Host, settings.Port, SecureSocketOptions.StartTlsWhenAvailable, cancellationToken);
            await client.AuthenticateAsync(settings.UserName, settings.Password, cancellationToken);
            await client.SendAsync(mime, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception ex) when (ex is not EmailSendException)
        {
            logger.LogError(ex, "SMTP send failed to {To}", message.ToAddress);
            throw new EmailSendException("EMAIL_SEND_FAILED", "Não foi possível enviar o e-mail.");
        }
    }
}

public sealed class EmailSendException(string code, string detail) : Exception(detail)
{
    public string Code { get; } = code;
}
